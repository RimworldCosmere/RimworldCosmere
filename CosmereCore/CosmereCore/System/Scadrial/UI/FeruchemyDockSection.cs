using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
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
    private const float StripHeight = 78f;
    private const float IdleTarget = 50f;
    private static readonly Color ActiveTint = new Color(0.490f, 0.604f, 0.659f);
    private static readonly Color QuadHeader = new Color(0.365f, 0.463f, 0.525f);
    private static readonly Color StoreFill = new Color(0.373f, 0.549f, 0.627f);
    private static readonly Color TapFill = new Color(0.659f, 0.435f, 0.290f);

    private readonly Dictionary<string, string> labelCache = new Dictionary<string, string>();
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;
    private string? expandedMetal;

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

        MetalTileState state = !capacity.HasMetalmind
            ? MetalTileState.Inert
            : gene != null && (gene.isTapping || gene.isStoring)
                ? MetalTileState.Active
                : MetalTileState.Idle;

        MetalTile.Draw(
            rect,
            cell.Icon,
            MetalLabel(cell),
            capacity.HasMetalmind ? $"{capacity.Stored:0}/{capacity.Max:0}" : "—",
            capacity.Fraction,
            MetalPalette.For(cell.SubsystemId),
            state,
            ActiveTint
        );

        TooltipHandler.TipRegion(rect, () => Tooltip(cell, capacity), cell.SubsystemId.GetHashCode());

        if (!capacity.HasMetalmind) return;
        if (!Widgets.ButtonInvisible(rect)) return;

        expandedMetal = expandedMetal == cell.SubsystemId ? null : cell.SubsystemId;
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        Event.current?.Use();
    }

    private void DrawStrip(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return;

        Capacity capacity = CapacityOf(gene);
        Widgets.DrawBoxSolid(rect, new Color(0.082f, 0.075f, 0.059f));
        Widgets.DrawBoxSolidWithOutline(rect, Color.clear, new Color(0.239f, 0.216f, 0.188f));

        Rect inner = rect.ContractedBy(7f, 6f);
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        string direction = gene.isTapping
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
            $"{capacity.Stored:0} / {capacity.Max:0}",
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            new Color(0.435f, 0.404f, 0.361f)
        );

        // Below fifty taps, above stores. The reachable span is bounded by what
        // the metalminds can actually give or accept right now.
        Rect sliderRect = new Rect(inner.x, inner.y + tinyH + 4f, inner.width, 12f);
        DrawDialBacking(sliderRect, gene, capacity);

        float min = capacity.CanTap ? 0f : IdleTarget;
        float max = capacity.CanStore ? 100f : IdleTarget;
        float current = Mathf.Clamp(gene.targetValue, min, max);
        float next = Widgets.HorizontalSlider(sliderRect, current, min, max);
        if (!Mathf.Approximately(next, gene.targetValue)) gene.targetValue = next;

        Rect endsRect = new Rect(inner.x, sliderRect.yMax + 1f, inner.width, tinyH);
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

        float buttonY = endsRect.yMax + 3f;
        AllomanticAbilityDef? compound = CompoundFor(pawn, cell.SubsystemId);
        float buttonWidth = compound != null ? (inner.width - 5f) / 2f : inner.width;

        if (Widgets.ButtonText(new Rect(inner.x, buttonY, buttonWidth, 20f), "CC_Dock_Feruchemy_Idle".Translate())) {
            gene.Reset();
            Event.current?.Use();
        }

        if (compound == null) return;

        Ability? ability = pawn.abilities?.GetAbility(compound);
        bool canCompound = ability != null && ability.CanCast;
        Rect compoundRect = new Rect(inner.x + buttonWidth + 5f, buttonY, buttonWidth, 20f);
        if (Widgets.ButtonText(compoundRect, "CC_Dock_Twinborn_Compound".Translate(), active: canCompound) && canCompound) {
            ability!.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
            Event.current?.Use();
        }
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
        float max = 0f;
        for (int i = 0; i < sources.Count; i++) {
            stored += sources[i].StoredAmount;
            max += sources[i].MaxAmount;
        }

        return new Capacity(stored, max, sources.Count > 0, gene.canTap, gene.canStore);
    }

    private static AllomanticAbilityDef? CompoundFor(Pawn pawn, string metalDefName) {
        if (pawn.genes == null) return null;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && a.metal.defName == metalDefName && !a.Overridden) {
                return a.metal.GetCompoundAbility();
            }
        }

        return null;
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
        public Capacity(float stored, float max, bool hasMetalmind, bool canTap, bool canStore) {
            Stored = stored;
            Max = max;
            HasMetalmind = hasMetalmind;
            CanTap = canTap;
            CanStore = canStore;
        }

        public float Stored { get; }
        public float Max { get; }
        public bool HasMetalmind { get; }
        public bool CanTap { get; }
        public bool CanStore { get; }
        public float Fraction => Max > 0f ? Stored / Max : 0f;
    }
}
