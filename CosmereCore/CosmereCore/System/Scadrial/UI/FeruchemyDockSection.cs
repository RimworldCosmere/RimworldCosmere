using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
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
        StripPadding * 2f + Text.LineHeightOf(GameFont.Tiny) * 3f + DialHeight * 2f + StripButtonHeight + 26f;
    private const float IdleTarget = 50f;
    private static readonly Color ActiveTint = new Color(0.490f, 0.604f, 0.659f);
    private static readonly Color QuadHeader = new Color(0.475f, 0.588f, 0.655f);
    private static readonly Color StoreFill = new Color(0.373f, 0.549f, 0.627f);
    private static readonly Color TapFill = new Color(0.659f, 0.435f, 0.290f);
    private static readonly Color CompoundTint = new Color(0.851f, 0.667f, 0.286f);

    private readonly Dictionary<string, string> labelCache = new Dictionary<string, string>();
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;
    private string? expandedMetal;
    private string? draggingDial;

    public override string SystemId => "Feruchemy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        return MetallicArtsTable.HeightFor(GroupsFor(pawn, snapshot), expandedMetal, StripHeight);
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        MetallicArtsTable.Draw(
            rect,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            expandedMetal,
            StripHeight,
            (tileRect, row) => DrawTile(tileRect, pawn, row),
            (stripRect, row) => DrawStrip(stripRect, pawn, row)
        );
    }

    private void DrawTile(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        Capacity capacity = CapacityOf(gene);

        bool compounding = gene?.isCompounding ?? false;
        MetalTileState state = !capacity.HasMetalmind
            ? MetalTileState.Inert
            : compounding
                ? MetalTileState.Flaring
                : gene != null && (gene.isTapping || gene.isStoring)
                    ? MetalTileState.Active
                    : MetalTileState.Idle;

        Color tint = compounding
            ? CompoundTint
            : gene is { isStoring: true }
                ? StoreFill
                : gene is { isTapping: true }
                    ? TapFill
                    : ActiveTint;

        MetalTile.Draw(
            rect,
            cell.Icon,
            MetalLabel(cell),
            capacity.HasMetalmind ? $"{capacity.Stored:0}+{capacity.Compounded:0}/{capacity.Max:0}" : "—",
            capacity.StoredFraction,
            MetalPalette.For(cell.SubsystemId),
            state,
            tint,
            capacity.CompoundedFraction,
            capacity.CanStoreCompounded || capacity.Compounded > 0f ? CompoundTint : null
        );

        TooltipHandler.TipRegion(rect, () => Tooltip(cell, capacity), cell.SubsystemId.GetHashCode());

        if (!capacity.HasMetalmind) return;
        if (!Widgets.ButtonInvisible(rect)) return;

        Event? ev = Event.current;

        expandedMetal = expandedMetal == cell.SubsystemId ? null : cell.SubsystemId;
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        ev?.Use();
    }

    private void DrawStrip(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return;

        Capacity capacity = CapacityOf(gene);
        Widgets.DrawBoxSolid(rect, new Color(0.082f, 0.075f, 0.059f));
        Widgets.DrawBoxSolidWithOutline(rect, Color.clear, new Color(0.239f, 0.216f, 0.188f));

        Rect inner = rect.ContractedBy(StripPadding);
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        string direction = gene.isCompounding
            ? "CC_Dock_Feruchemy_Compounding".Translate()
            : gene.isTapping
                ? "CC_Dock_Feruchemy_Tapping".Translate()
                : gene.isStoring
                    ? "CC_Dock_Feruchemy_Storing".Translate()
                    : "CC_Dock_Feruchemy_Idle".Translate();
        UIText.EllipsisLabel(
            new Rect(inner.x, inner.y, inner.width * 0.6f, tinyH),
            MetalLabel(cell) + " - " + direction,
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            new Color(0.604f, 0.659f, 0.678f)
        );
        UIText.EllipsisLabel(
            new Rect(inner.x + inner.width * 0.6f, inner.y, inner.width * 0.4f, tinyH),
            $"{capacity.Stored:0} + {capacity.Compounded:0} / {capacity.Max:0}",
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            new Color(0.435f, 0.404f, 0.361f)
        );

        // Below fifty taps, above stores. The reachable span is bounded by what
        // the metalminds can actually give or accept right now.
        Rect sliderRect = new Rect(inner.x, inner.y + tinyH + 6f, inner.width, DialHeight);
        DrawDial(sliderRect, cell.SubsystemId, gene, capacity);

        Rect endsRect = new Rect(inner.x, sliderRect.yMax + 3f, inner.width, tinyH);
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Tap".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            capacity.CanTap ? new Color(0.498f, 0.541f, 0.565f) : new Color(0.310f, 0.286f, 0.255f)
        );
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Store".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            capacity.CanStore ? new Color(0.498f, 0.541f, 0.565f) : new Color(0.310f, 0.286f, 0.255f)
        );

        float rate = gene.TransferRatePerSecond;
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Rate".Translate($"{rate:+0.00;-0.00;0.00}".Named("RATE")),
            GameFont.Tiny,
            TextAnchor.MiddleCenter,
            gene.isCompounding
                ? CompoundTint
                : Mathf.Approximately(rate, 0f)
                    ? new Color(0.376f, 0.353f, 0.318f)
                    : rate < 0f
                        ? TapFill
                        : StoreFill
        );

        float buttonY = endsRect.yMax + 8f;
        if (capacity.CanStoreCompounded || capacity.Compounded > 0f) {
            buttonY = DrawCompoundedDial(inner, endsRect.yMax + 6f, pawn, cell, gene, capacity) + 8f;
        }

        if (DockChrome.Button(
                new Rect(inner.x, buttonY, inner.width, StripButtonHeight),
                "CC_Dock_Feruchemy_Idle".Translate(),
                true,
                ActiveTint
            )) {
            gene.Reset();
            Event.current?.Use();
        }
    }

    /// The compounded pool gets its own dial: drag left to tap it, right to
    /// compound into it, burning allomantic reserve to do so.
    private float DrawCompoundedDial(
        Rect inner,
        float y,
        Pawn pawn,
        InvestitureCell cell,
        Feruchemist gene,
        Capacity capacity
    ) {
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        Rect dial = new Rect(inner.x, y, inner.width, DialHeight);
        DrawCompoundedDialBacking(dial, gene, capacity);

        float handleX = dial.x + dial.width * (Mathf.Clamp(gene.compoundedTargetValue, 0f, 100f) / 100f);
        Widgets.DrawBoxSolid(
            new Rect(handleX - 1.5f, dial.y - 2f, 3f, dial.height + 4f),
            new Color(0.898f, 0.812f, 0.588f)
        );

        Rect ends = new Rect(inner.x, dial.yMax + 3f, inner.width, tinyH);
        UIText.EllipsisLabel(
            ends,
            "CC_Dock_Feruchemy_Tap".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            capacity.CanTapCompounded ? CompoundTint : new Color(0.310f, 0.286f, 0.255f)
        );
        UIText.EllipsisLabel(
            ends,
            "CC_Dock_Feruchemy_Compound".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            capacity.CanStoreCompounded ? CompoundTint : new Color(0.310f, 0.286f, 0.255f)
        );

        float rate = gene.CompoundedRatePerSecond;
        UIText.EllipsisLabel(
            ends,
            "CC_Dock_Feruchemy_Rate".Translate($"{rate:+0.00;-0.00;0.00}".Named("RATE")),
            GameFont.Tiny,
            TextAnchor.MiddleCenter,
            Mathf.Approximately(rate, 0f) ? new Color(0.376f, 0.353f, 0.318f) : CompoundTint
        );

        HandleDialDrag(dial, cell.SubsystemId + ":compounded", gene, capacity, true);
        SyncCompounding(pawn, cell, gene);
        return ends.yMax;
    }

    /// The dial is the control, so the ability follows it. A dial pushed into the
    /// compound half that cannot start falls back to idle rather than sitting on a
    /// setting the pawn is not honouring.
    private void SyncCompounding(Pawn pawn, InvestitureCell cell, Feruchemist gene) {
        AllomancyAbility? ability = CompoundingAccess.AbilityFor(pawn, cell.SubsystemId);
        if (ability == null) return;

        bool wants = gene.compoundedTargetValue > IdleTarget;
        if (wants == gene.isCompounding) return;

        if (!wants) {
            ability.UpdateStatus(BurningStatus.Off);
            return;
        }

        if (!CompoundingAccess.Gate(pawn, gene, ability).Accepted) {
            gene.compoundedTargetValue = IdleTarget;
            return;
        }

        ability.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
    }

    private static void DrawCompoundedDialBacking(Rect rect, Feruchemist gene, Capacity capacity) {
        Widgets.DrawBoxSolid(rect, new Color(0.047f, 0.043f, 0.035f));

        Color blocked = new Color(0.098f, 0.090f, 0.075f);
        if (!capacity.CanTapCompounded) {
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        if (!capacity.CanStoreCompounded) {
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        float delta = gene.compoundedTargetValue - IdleTarget;
        if (delta < 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(-delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x - width, rect.y, width, rect.height), TapFill);
        }
        else if (delta > 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, width, rect.height), CompoundTint);
        }

        Widgets.DrawBoxSolid(
            new Rect(rect.center.x, rect.y - 1f, 1f, rect.height + 2f),
            new Color(0.353f, 0.322f, 0.271f)
        );
    }

    /// Hand-drawn so the dial keeps the section's chrome. The vanilla slider
    /// brings its own tan gradient, which fights everything around it.
    private void DrawDial(Rect rect, string metalId, Feruchemist gene, Capacity capacity) {
        DrawDialBacking(rect, gene, capacity);

        float min = capacity.CanTap || capacity.CanTapCompounded ? 0f : IdleTarget;
        float max = capacity.CanStore ? 100f : IdleTarget;

        float handleX = rect.x + rect.width * (Mathf.Clamp(gene.targetValue, 0f, 100f) / 100f);
        Widgets.DrawBoxSolid(
            new Rect(handleX - 1.5f, rect.y - 2f, 3f, rect.height + 4f),
            new Color(0.816f, 0.851f, 0.871f)
        );

        HandleDialDrag(rect, metalId, gene, capacity, false);
    }

    /// Shared by both dials. Compounded runs tap-only, so its reachable span stops
    /// at the idle point rather than continuing into the store half.
    private void HandleDialDrag(Rect rect, string dialId, Feruchemist gene, Capacity capacity, bool compounded) {
        float min = (compounded ? capacity.CanTapCompounded : capacity.CanTap || capacity.CanTapCompounded)
            ? 0f
            : IdleTarget;
        float max = (compounded ? capacity.CanStoreCompounded : capacity.CanStore) ? 100f : IdleTarget;

        Event? e = Event.current;
        if (e == null) return;

        if (e.type == EventType.MouseDown && Mouse.IsOver(rect)) {
            draggingDial = dialId;
            e.Use();
        }

        if (draggingDial != dialId) return;

        // MouseDrag only reaches a control that claimed the hot control, which a
        // hand-drawn dial never does, so follow the button state directly.
        if (!Input.GetMouseButton(0)) {
            draggingDial = null;
            return;
        }

        float snapped = gene.SnapTarget(Mathf.Clamp((e.mousePosition.x - rect.x) / rect.width * 100f, min, max));
        if (compounded) gene.compoundedTargetValue = snapped;
        else gene.targetValue = snapped;
    }


    private static void DrawDialBacking(Rect rect, Feruchemist gene, Capacity capacity) {
        Widgets.DrawBoxSolid(rect, new Color(0.047f, 0.043f, 0.035f));

        Color blocked = new Color(0.098f, 0.090f, 0.075f);
        if (!capacity.CanTap) {
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        if (!capacity.CanStore) {
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height), blocked);
        }

        float delta = gene.targetValue - IdleTarget;
        if (delta < 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(-delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x - width, rect.y, width, rect.height), TapFill);
        }
        else if (delta > 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, width, rect.height), StoreFill);
        }

        Widgets.DrawBoxSolid(
            new Rect(rect.center.x, rect.y - 1f, 1f, rect.height + 2f),
            new Color(0.353f, 0.322f, 0.271f)
        );
    }




    private string Tooltip(InvestitureCell cell, Capacity capacity) {
        if (!capacity.HasMetalmind) {
            return "CC_Dock_Feruchemy_NoMetalmind".Translate(MetalLabel(cell).Named("METAL"));
        }


        return "CC_Dock_Feruchemy_Tip".Translate(
            MetalLabel(cell).Named("METAL"),
            capacity.Stored.ToString("0").Named("STORED"),
            capacity.Max.ToString("0").Named("MAX")
        );
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

    private static Capacity CapacityOf(Feruchemist? gene) {
        if (gene == null) return default;

        List<IMetalmindSource> sources = gene.metalminds;
        float stored = 0f;
        float compounded = 0f;
        float max = 0f;
        for (int i = 0; i < sources.Count; i++) {
            stored += sources[i].StoredAmount;
            compounded += sources[i].CompoundedAmount;
            max += sources[i].MaxAmount;
        }

        return new Capacity(
            stored,
            compounded,
            max,
            sources.Count > 0,
            gene.canTap,
            gene.canStore,
            gene.canTapCompounded,
            gene.canStoreCompounded
        );
    }


    private static Feruchemist? FindGene(Pawn pawn, string metalDefName) {
        if (pawn.genes == null) return null;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && f.metal.defName == metalDefName && !f.Overridden) return f;
        }

        return null;
    }

    private readonly struct Capacity {
        public Capacity(
            float stored,
            float compounded,
            float max,
            bool hasMetalmind,
            bool canTap,
            bool canStore,
            bool canTapCompounded,
            bool canStoreCompounded
        ) {
            Stored = stored;
            Compounded = compounded;
            Max = max;
            HasMetalmind = hasMetalmind;
            CanTap = canTap;
            CanStore = canStore;
            CanTapCompounded = canTapCompounded;
            CanStoreCompounded = canStoreCompounded;
        }

        public float Stored { get; }
        public float Compounded { get; }
        public float Max { get; }
        public bool HasMetalmind { get; }
        public bool CanTap { get; }
        public bool CanStore { get; }
        public bool CanTapCompounded { get; }
        public bool CanStoreCompounded { get; }
        public float Fraction => Max > 0f ? (Stored + Compounded) / Max : 0f;
        public float StoredFraction => Max > 0f ? Stored / Max : 0f;
        public float CompoundedFraction => Max > 0f ? Compounded / Max : 0f;
    }
}
