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

    private static float StripHeight =>
        StripPadding * 2f + Text.LineHeightOf(GameFont.Tiny) * 3f + DialHeight * 2f + StripButtonHeight +
        DockDropdownRow.Height + 32f;

    private static readonly Color ActiveTint = new Color(0.490f, 0.604f, 0.659f);
    private static readonly Color QuadHeader = new Color(0.475f, 0.588f, 0.655f);

    private readonly ScadrialCrest crest = new ScadrialCrest(true);
    private readonly Dictionary<string, string> labelCache = new Dictionary<string, string>();
    private readonly Reveal reveal = new Reveal();
    private readonly FeruchemyDialWidget dial = new FeruchemyDialWidget();
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;
    private string? expandedMetal;

    // Which strip is on screen, which is not the same as which one the player has open:
    // a closing strip has to keep drawing until it has finished sliding away.
    private string? revealedMetal;
    private float revealedHeight;

    // Picked while another metal is still open, and held until that one has finished
    // sliding away.
    private string? pendingMetal;

    // Idempotent, because height is asked for several times a frame. Reveal itself only
    // advances once per frame; this just re-reads where it got to.
    private void StepReveal() {
        // One panel at a time, in order: the open metal slides up, then the new one
        // slides down. Swapping the contents mid-slide makes the panel jump rows while
        // it is moving, which reads as a glitch rather than as an exchange.
        if (expandedMetal == null && pendingMetal != null && revealedHeight < 1f) {
            expandedMetal = pendingMetal;
            pendingMetal = null;
        }

        revealedHeight = reveal.Toward(expandedMetal == null ? 0f : StripHeight);
        if (expandedMetal != null) revealedMetal = expandedMetal;
        else if (revealedHeight < 1f) revealedMetal = null;
    }

    // Clicking the open metal closes it; clicking a different one closes it first and
    // queues the new one behind it.
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

    public override string SystemId => "Feruchemy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        StepReveal();
        return crest.Height + MetallicArtsTable.HeightFor(GroupsFor(pawn, snapshot), revealedMetal, revealedHeight);
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        crest.Draw(new Rect(rect.x, rect.y, rect.width, crest.Height - ScadrialCrest.Gap), Skin);
        StepReveal();

        Rect table = new Rect(rect.x, rect.y + crest.Height, rect.width, rect.height - crest.Height);
        MetallicArtsTable.Draw(
            table,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            revealedMetal,
            revealedHeight,
            StripHeight,
            (tileRect, row) => DrawTile(tileRect, pawn, row),
            (stripRect, row, tileRect) => DrawStrip(stripRect, pawn, row, tileRect)
        );
    }

    private void DrawTile(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        FeruchemyCapacity capacity = FeruchemyCapacity.Of(gene);

        bool compounding = gene?.isCompounding ?? false;
        MetalTileState state = !capacity.HasMetalmind
            ? MetalTileState.Inert
            : compounding
                ? MetalTileState.Flaring
                : gene != null && (gene.isTapping || gene.isStoring)
                    ? MetalTileState.Active
                    : MetalTileState.Idle;

        Color tint = compounding
            ? FeruchemyPalette.CompoundTint
            : gene is { isStoring: true }
                ? FeruchemyPalette.StoreFill
                : gene is { isTapping: true }
                    ? FeruchemyPalette.TapFill
                    : ActiveTint;

        MetalTile.Draw(
            rect,
            cell.Icon,
            MetalLabel(cell),
            capacity.HasMetalmind ? capacity.Readout : "\u2014",
            capacity.StoredFraction,
            MetalPalette.For(cell.SubsystemId),
            state,
            tint,
            capacity.CompoundedFraction,
            capacity.CanStoreCompounded || capacity.Internal > 0f ? FeruchemyPalette.CompoundTint : null,
            SavantStageFor(pawn, cell),
            revealedMetal == cell.SubsystemId
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

        // Same fill and stroke as the tile, opened along the tile's span: the two are
        // one merged surface. A working metalmind carries its accent down through the
        // join as well, so the pair reads as one thing that is running.
        bool compounding = gene.isCompounding;
        bool hot = compounding || gene.isTapping || gene.isStoring;
        Color tint = compounding
            ? FeruchemyPalette.CompoundTint
            : gene.isStoring
                ? FeruchemyPalette.StoreFill
                : gene.isTapping
                    ? FeruchemyPalette.TapFill
                    : ActiveTint;
        Panel.DrawNotchedTop(
            rect,
            MetalTile.Fill,
            hot ? tint : MetalTile.Border,
            openTile.x,
            openTile.xMax,
            hot ? tint : null,
            Panel.LitWash(compounding)
        );

        Rect inner = rect.ContractedBy(StripPadding);
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        // Burning the metalmind reads first: it is the loudest thing happening and
        // the only one that destroys something.
        string direction = capacity.CompoundedRate < 0f
            ? "CC_Dock_Feruchemy_BurningMetalmind".Translate()
            : capacity.CompoundedRate > 0f
                ? "CC_Dock_Feruchemy_Compounding".Translate()
                : gene.isTapping
                    ? "CC_Dock_Feruchemy_Tapping".Translate()
                    : gene.isStoring
                        ? "CC_Dock_Feruchemy_Storing".Translate()
                        : "CC_Dock_Feruchemy_Idle".Translate();
        UIText.EllipsisLabel(
            new Rect(inner.x, inner.y, inner.width * 0.6f, tinyH),
            direction,
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

        // Below fifty taps, above stores. The reachable span is bounded by what
        // the metalminds can actually give or accept right now.
        Rect sliderRect = new Rect(inner.x, inner.y + tinyH + 6f, inner.width, DialHeight);
        dial.DrawDial(sliderRect, cell.SubsystemId, gene, capacity, compounded);

        Rect endsRect = new Rect(inner.x, sliderRect.yMax + 3f, inner.width, tinyH);
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Tap".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            (compounded ? capacity.CanTapCompounded : capacity.CanTap)
                ? compounded ? FeruchemyPalette.CompoundTint : new Color(0.498f, 0.541f, 0.565f)
                : new Color(0.310f, 0.286f, 0.255f)
        );
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Store".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            (compounded ? capacity.CanStoreCompounded : capacity.CanStore)
                ? compounded ? FeruchemyPalette.CompoundTint : new Color(0.498f, 0.541f, 0.565f)
                : new Color(0.310f, 0.286f, 0.255f)
        );

        float rate = gene.TransferRatePerSecond;
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Rate".Translate($"{rate:+0.00;-0.00;0.00}".Named("RATE")),
            GameFont.Tiny,
            TextAnchor.MiddleCenter,
            gene.isCompounding
                ? FeruchemyPalette.CompoundTint
                : Mathf.Approximately(rate, 0f)
                    ? new Color(0.376f, 0.353f, 0.318f)
                    : rate < 0f
                        ? FeruchemyPalette.TapFill
                        : FeruchemyPalette.StoreFill
        );

        float afterTarget = FeruchemyTargetRow.Draw(inner, endsRect.yMax + 6f, gene);
        float buttonY = dial.DrawCompoundToggle(inner, afterTarget + 6f, pawn, gene, StripButtonHeight) + 8f;

        if (DockButton.Draw(
                new Rect(inner.x, buttonY, inner.width, StripButtonHeight),
                "CC_Dock_Feruchemy_Idle".Translate(),
                ActiveTint,
                kind: DockButtonKind.Ghost
            )) {
            gene.Reset();
            Event.current?.Use();
        }
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

    // What this metal stores and taps, in the metal def's own words. The tooltip
    // described the click rather than the power before this.
    private static string MetalEffect(Pawn pawn, InvestitureCell cell) {
        MetallicArtsMetalDef? metal =
            DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId);
        string? description = metal?.feruchemy?.description;

        return string.IsNullOrEmpty(description)
            ? string.Empty
            : description!.Formatted(pawn.LabelShort.Named("PAWN")).Resolve();
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

    private static Feruchemist? FindGene(Pawn pawn, string metalDefName) {
        if (pawn.genes == null) return null;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && f.metal.defName == metalDefName && !f.Overridden) return f;
        }

        return null;
    }
}
