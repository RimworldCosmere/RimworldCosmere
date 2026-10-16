using Cosmere.Core.Tab;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.UI.Feruchemy;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyDockSection : DockSectionBase {
    private const float StripPadding = 7f;
    private const float DialHeight = 12f;
    private const float StripButtonHeight = 22f;

    /// <summary>
    ///     A closed strip never asks its gene for anything; an open one adds whatever
    ///     FeruchemyLedgerRow will actually draw for it, so the two never drift apart.
    /// </summary>
    /// <returns>The open strip's full height in pixels, or 0 when the pawn has no Feruchemist gene.</returns>
    private static float StripHeightFor(Feruchemist? gene) {
        if (gene == null) return 0f;

        float compoundToggleHeight =
            FeruchemyDialWidget.ShowsCompoundToggle(gene) ? StripButtonHeight : 0f;
        return StripPadding * 2f + Text.LineHeightOf(GameFont.Tiny) * 3f + DialHeight * 2f + compoundToggleHeight +
            DockDropdownRow.Height + 32f + FeruchemyLedgerRow.HeightFor(gene);
    }

    private static readonly Color ActiveTint = new Color(0.490f, 0.604f, 0.659f);
    private static readonly Color EndLabelLive = new Color(0.498f, 0.541f, 0.565f);
    private static readonly Color EndLabelBlocked = new Color(0.310f, 0.286f, 0.255f);
    private static readonly Color RateIdle = new Color(0.376f, 0.353f, 0.318f);
    private static readonly Color QuadHeader = new Color(0.475f, 0.588f, 0.655f);

    private readonly ScadrialCrest crest = new ScadrialCrest(true);
    private readonly Dictionary<string, string> labelCache = [];
    private readonly Reveal reveal = new Reveal();
    private readonly FeruchemyDialWidget dial = new FeruchemyDialWidget();

    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;
    private string? expandedMetal;

    /// <summary>
    ///     Which strip is on screen, not the same as which one the player has open: a closing
    ///     strip has to keep drawing until it has finished sliding away.
    /// </summary>
    private string? revealedMetal;
    private float revealedHeight;

    // picked while another metal is open, held until that one finishes sliding away.
    private string? pendingMetal;

    /// <summary>
    ///     Idempotent, because height is asked for several times a frame. Reveal itself only
    ///     advances once per frame; this just re-reads where it got to.
    /// </summary>
    private void StepReveal(float openHeight) {
        // one panel at a time - swapping mid-slide would jump rows and read as a glitch.
        if (expandedMetal == null && pendingMetal != null && revealedHeight < 1f) {
            expandedMetal = pendingMetal;
            pendingMetal = null;
        }

        revealedHeight = reveal.Toward(expandedMetal == null ? 0f : openHeight);
        if (expandedMetal != null) revealedMetal = expandedMetal;
        else if (revealedHeight < 1f) revealedMetal = null;
    }

    /// <summary>
    ///     Clicking the open metal closes it; clicking a different one closes it first and
    ///     queues the new one behind it.
    /// </summary>
    private void ToggleMetal(string subsystemId) {
        if (expandedMetal == subsystemId) {
            expandedMetal = null;
            pendingMetal = null;
            return;
        }

        if (expandedMetal == null && revealedHeight < 1f) {
            expandedMetal = subsystemId;
            pendingMetal = null;
            return;
        }

        expandedMetal = null;
        pendingMetal = subsystemId;
    }

    private bool IsCollapsed(MetalGroup group) {
        return IsGroupCollapsed(group.LabelKey);
    }

    /// <summary>
    ///     Folding a quadrant closes any metal open inside it, so the strip does not keep
    ///     its height with nothing above it to say where it came from.
    /// </summary>
    private void ToggleGroup(MetalGroup group) {
        if (!ToggleGroupFold(group.LabelKey)) return;

        for (int i = 0; i < group.Rows.Count; i++) {
            string id = group.Rows[i].Cell.SubsystemId;
            if (expandedMetal == id) expandedMetal = null;
            if (pendingMetal == id) pendingMetal = null;
        }
    }

    public override string SystemId => "Feruchemy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        StepReveal(StripHeightFor(FindGene(pawn, expandedMetal ?? pendingMetal)));
        return crest.Height + MetallicArtsTable.HeightFor(
            GroupsFor(pawn, snapshot),
            revealedMetal,
            revealedHeight,
            IsCollapsed
        );
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        crest.Draw(new Rect(rect.x, rect.y, rect.width, crest.Height - ScadrialCrest.Gap), Skin);
        StepReveal(StripHeightFor(FindGene(pawn, expandedMetal ?? pendingMetal)));
        float revealedStripHeight = StripHeightFor(FindGene(pawn, revealedMetal));

        Rect table = new Rect(rect.x, rect.y + crest.Height, rect.width, rect.height - crest.Height);
        MetallicArtsTable.Draw(
            table,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            revealedMetal,
            revealedHeight,
            revealedStripHeight,
            (tileRect, row) => DrawTile(tileRect, pawn, row),
            (stripRect, row, tileRect) => DrawStrip(stripRect, pawn, row, tileRect),
            IsCollapsed,
            ToggleGroup
        );
    }

    private void DrawTile(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        FeruchemyCapacity capacity = FeruchemyCapacity.Of(gene);

        bool compounding = gene?.IsCompounding ?? false;
        MetalTileState state = MetalTileState.Idle;
        if (!capacity.HasMetalmind) state = MetalTileState.Inert;
        else if (compounding) state = MetalTileState.Flaring;
        else if (gene != null && (gene.IsTapping || gene.IsStoring)) state = MetalTileState.Active;

        string? foldLabel = null;
        if (capacity.HasMetalmind) {
            foldLabel = revealedMetal == cell.SubsystemId
                ? (string)"CC_Dock_Fold_Hide".Translate()
                : (string)"CC_Dock_Fold_Show".Translate();
        }

        MetalTile.Draw(
            rect,
            cell.Icon,
            MetalLabel(cell),
            capacity.HasMetalmind ? capacity.Readout : "\u2014",
            capacity.StoredFraction,
            MetalPalette.For(cell.SubsystemId),
            state,
            capacity.CompoundedFraction,
            capacity.CanStoreCompounded || capacity.Internal > 0f ? FeruchemyPalette.CompoundTint : null,
            SavantStageFor(pawn, cell),
            revealedMetal == cell.SubsystemId,
            foldLabel
        );

        TooltipHandler.TipRegion(rect, () => Tooltip(pawn, cell, capacity), cell.SubsystemId.GetHashCode());

        if (!capacity.HasMetalmind) return;
        if (!Widgets.ButtonInvisible(rect)) return;

        Event? ev = Event.current;

        ToggleMetal(cell.SubsystemId);
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        ev?.Use();
    }

    private void DrawStrip(Rect rect, Pawn pawn, MetalRow row, Rect openTile) {
        InvestitureCell cell = row.Cell;
        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return;

        FeruchemyCapacity capacity = FeruchemyCapacity.Of(gene);

        // same fill/stroke as the tile, and no accent either - the rows below already say what the metalmind is doing.
        Panel.DrawNotchedTop(rect, DockPalette.StripFill, MetalTile.Border, openTile.x, openTile.xMax);

        Rect inner = rect.ContractedBy(StripPadding);
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        UIText.EllipsisLabel(
            new Rect(inner.x, inner.y, inner.width * 0.6f, tinyH),
            DirectionLabel(gene, capacity),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            new Color(0.604f, 0.659f, 0.678f)
        );
        UIText.EllipsisLabel(
            new Rect(inner.x + inner.width * 0.6f, inner.y, inner.width * 0.4f, tinyH),
            capacity.Readout,
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            new Color(0.435f, 0.404f, 0.361f)
        );

        bool compounded = gene.compounding;

        // below fifty taps, above stores - span is bounded by what the metalminds can give or take.
        Rect sliderRect = new Rect(inner.x, inner.y + tinyH + 6f, inner.width, DialHeight);
        dial.DrawDial(sliderRect, cell.SubsystemId, gene, capacity, compounded);

        Rect endsRect = new Rect(inner.x, sliderRect.yMax + 3f, inner.width, tinyH);
        bool tapReachable = compounded ? capacity.CanTapCompounded : capacity.CanTap;
        bool storeReachable = compounded ? capacity.CanStoreCompounded : capacity.CanStore;
        Color endLive = compounded ? FeruchemyPalette.CompoundTint : EndLabelLive;

        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Tap".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            tapReachable ? endLive : EndLabelBlocked
        );
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Store".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            storeReachable ? endLive : EndLabelBlocked
        );

        float rate = gene.TransferRatePerSecond;
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Rate".Translate($"{rate:+0.00;-0.00;0.00}".Named("RATE")),
            GameFont.Tiny,
            TextAnchor.MiddleCenter,
            RateTint(gene, rate)
        );

        float afterTarget = FeruchemyTargetRow.Draw(inner, endsRect.yMax + 6f, gene);
        afterTarget = FeruchemyLedgerRow.Draw(inner, afterTarget, gene);
        float buttonY = FeruchemyDialWidget.DrawCompoundToggle(inner, afterTarget + 6f, pawn, gene, StripButtonHeight) + 8f;

        if (DockButton.Draw(
                new Rect(inner.x, buttonY, inner.width, StripButtonHeight),
                "CC_Dock_Feruchemy_Idle".Translate(),
                ActiveTint
            )) {
            gene.Reset();
            Event.current?.Use();
        }
    }

    // Burning reads first - the loudest thing happening, and the only one that destroys something.
    private static string DirectionLabel(Feruchemist gene, FeruchemyCapacity capacity) {
        if (capacity.CompoundedRate < 0f) return "CC_Dock_Feruchemy_BurningMetalmind".Translate();
        if (capacity.CompoundedRate > 0f) return "CC_Dock_Feruchemy_Compounding".Translate();
        if (gene.IsTapping) return "CC_Dock_Feruchemy_Tapping".Translate();
        if (gene.IsStoring) return "CC_Dock_Feruchemy_Storing".Translate();

        return "CC_Dock_Feruchemy_Idle".Translate();
    }

    private static Color RateTint(Feruchemist gene, float rate) {
        if (gene.IsCompounding) return FeruchemyPalette.CompoundTint;
        if (Mathf.Approximately(rate, 0f)) return RateIdle;

        return rate < 0f ? FeruchemyPalette.TapFill : FeruchemyPalette.StoreFill;
    }

    private string Tooltip(Pawn pawn, InvestitureCell cell, FeruchemyCapacity capacity) {
        string effect = MetalEffect(pawn, cell);

        if (!capacity.HasMetalmind) {
            return "CC_Dock_Feruchemy_NoMetalmind".Translate(
                MetalLabel(cell).Named("METAL"),
                effect.Named("EFFECT")
            );
        }

        return "CC_Dock_Feruchemy_Tip".Translate(
            MetalLabel(cell).Named("METAL"),
            capacity.UnitsReadout.Named("STORED"),
            effect.Named("EFFECT")
        );
    }

    /// <summary>
    ///     What this metal stores and taps, in the metal def's own words, with the pawn's
    ///     name filled in.
    /// </summary>
    /// <returns>The metal's feruchemy description, or an empty string when the def carries none.</returns>
    private static string MetalEffect(Pawn pawn, InvestitureCell cell) {
        MetallicArtsMetalDef? metal =
            DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId);
        string? description = metal?.feruchemy?.description;

        return string.IsNullOrEmpty(description)
            ? string.Empty
            : description.Formatted(pawn.LabelShort.Named("PAWN")).Resolve();
    }

    private IReadOnlyList<MetalGroup> GroupsFor(Pawn pawn, InvestitureSnapshot snapshot) {
        if (cachedGroups == null || cachedPawnId != pawn.thingIDNumber || cachedCellCount != snapshot.Cells.Count) {
            cachedGroups = MetalGroupTable.FeruchemyGroups(snapshot.Cells);
            cachedPawnId = pawn.thingIDNumber;
            cachedCellCount = snapshot.Cells.Count;
        }

        MetalGroupTable.RefreshCells(cachedGroups, snapshot.Cells);
        return cachedGroups;
    }

    private string MetalLabel(InvestitureCell cell) {
        if (labelCache.TryGetValue(cell.SubsystemId, out string? cached)) return cached;
        string label = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId)?.LabelCap ?? cell.SubsystemId;
        labelCache[cell.SubsystemId] = label;
        return label;
    }

    private static int SavantStageFor(Pawn pawn, InvestitureCell cell) {
        if (pawn.records == null) return 0;

        MetallicArtsMetalDef? def = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId);
        return def == null ? 0 : ScadrialSavantUtility.GetFeruchemicalSavantStage(pawn, def);
    }

    private static Feruchemist? FindGene(Pawn pawn, string? metalDefName) {
        if (metalDefName == null || pawn.genes == null) return null;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && f.Metal.defName == metalDefName && !f.Overridden) return f;
        }

        return null;
    }
}
