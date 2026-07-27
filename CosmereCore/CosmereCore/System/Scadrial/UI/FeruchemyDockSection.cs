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

    private const float TargetRowHeight = 20f;

    private static float StripHeight =>
        StripPadding * 2f + Text.LineHeightOf(GameFont.Tiny) * 3f + DialHeight * 2f + StripButtonHeight +
        TargetRowHeight + 32f;

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
            capacity.HasMetalmind ? capacity.Readout : "\u2014",
            capacity.StoredFraction,
            MetalPalette.For(cell.SubsystemId),
            state,
            tint,
            capacity.CompoundedFraction,
            capacity.CanStoreCompounded || capacity.Internal > 0f ? CompoundTint : null
        );

        TooltipHandler.TipRegion(rect, () => Tooltip(pawn, cell, capacity), cell.SubsystemId.GetHashCode());

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
        Widgets.DrawBoxSolid(rect, new Color(0.055f, 0.043f, 0.031f, 0.45f));
        Widgets.DrawBoxSolidWithOutline(rect, Color.clear, new Color(0.239f, 0.216f, 0.188f));

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
            MetalLabel(cell) + " - " + direction,
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
        DrawDial(sliderRect, cell.SubsystemId, gene, capacity, compounded);

        Rect endsRect = new Rect(inner.x, sliderRect.yMax + 3f, inner.width, tinyH);
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Tap".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            (compounded ? capacity.CanTapCompounded : capacity.CanTap)
                ? compounded ? CompoundTint : new Color(0.498f, 0.541f, 0.565f)
                : new Color(0.310f, 0.286f, 0.255f)
        );
        UIText.EllipsisLabel(
            endsRect,
            "CC_Dock_Feruchemy_Store".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            (compounded ? capacity.CanStoreCompounded : capacity.CanStore)
                ? compounded ? CompoundTint : new Color(0.498f, 0.541f, 0.565f)
                : new Color(0.310f, 0.286f, 0.255f)
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

        float afterTarget = DrawTargetRow(inner, endsRect.yMax + 6f, gene);
        float buttonY = DrawCompoundToggle(inner, afterTarget + 6f, pawn, cell, gene) + 8f;

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

    // Which metalmind the dials act on. A pawn wearing a band and carrying three
    // implants needs to say which one they mean before compounding makes sense,
    // since burning one destroys it.
    private static float DrawTargetRow(Rect inner, float y, Feruchemist gene) {
        Rect row = new Rect(inner.x, y, inner.width, TargetRowHeight);
        string label = TargetLabel(gene);

        Widgets.DrawBoxSolid(row, new Color(0.055f, 0.063f, 0.071f));
        Widgets.DrawHighlightIfMouseover(row);
        TooltipHandler.TipRegion(
            row,
            "CC_Dock_Feruchemy_TargetTip".Translate(TargetUnits(gene).Named("UNITS"))
        );

        UIText.EllipsisLabel(
            new Rect(row.x + 6f, row.y, row.width - 26f, row.height),
            "CC_Dock_Feruchemy_TargetLabel".Translate(label.Named("TARGET")),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            new Color(0.604f, 0.659f, 0.678f)
        );
        UIText.EllipsisLabel(
            new Rect(row.xMax - 20f, row.y, 14f, row.height),
            "v",
            GameFont.Tiny,
            TextAnchor.MiddleCenter,
            new Color(0.435f, 0.478f, 0.498f)
        );

        if (Widgets.ButtonInvisible(row)) {
            OpenTargetMenu(gene);
            Event.current?.Use();
        }

        return row.yMax;
    }

    // The raw figures behind the percentage, for the hover.
    private static string TargetUnits(Feruchemist gene) {
        float held = 0f;
        float max = 0f;

        List<IMetalmindSource> sources = gene.metalminds;
        for (int i = 0; i < sources.Count; i++) {
            if (!MatchesDisplayTarget(gene, sources[i])) continue;

            held += sources[i].TotalStored;
            max += sources[i].MaxAmount;
        }

        return $"{held:0} / {max:0}";
    }

    private static bool MatchesDisplayTarget(Feruchemist gene, IMetalmindSource source) {
        return gene.targetMetalmindId switch {
            Feruchemist.TargetAll => true,
            Feruchemist.TargetInternal => source.IsImplanted,
            Feruchemist.TargetExternal => !source.IsImplanted,
            _ => source.SourceId == gene.targetMetalmindId,
        };
    }

    private static string Percent(float held, float max) {
        return max > 0f ? $"{held / max * 100f:0}%" : "0%";
    }

    // What the row reads while pointed at a group, or at one metalmind.
    private static string TargetLabel(Feruchemist gene) {
        if (!Feruchemist.IsGroupTarget(gene.targetMetalmindId)) {
            IMetalmindSource? selected = gene.SelectedSource;

            return selected == null
                ? "CC_Dock_Feruchemy_TargetAll".Translate().Resolve()
                : $"{selected.SourceLabel} ({Percent(selected.TotalStored, selected.MaxAmount)})";
        }

        string key = gene.targetMetalmindId switch {
            Feruchemist.TargetInternal => "CC_Dock_Feruchemy_TargetInternal",
            Feruchemist.TargetExternal => "CC_Dock_Feruchemy_TargetExternal",
            _ => "CC_Dock_Feruchemy_TargetAll",
        };

        return GroupSummary(gene, key, gene.targetMetalmindId);
    }

    // Groups carry their own running total, so choosing one does not hide how much
    // is actually in there.
    private static string GroupSummary(Feruchemist gene, string key, string target) {
        float stored = 0f;
        float max = 0f;
        int count = 0;

        List<IMetalmindSource> sources = gene.metalminds;
        for (int i = 0; i < sources.Count; i++) {
            bool internalOnly = target == Feruchemist.TargetInternal;
            bool externalOnly = target == Feruchemist.TargetExternal;
            if (internalOnly && !sources[i].IsImplanted) continue;
            if (externalOnly && sources[i].IsImplanted) continue;

            stored += sources[i].TotalStored;
            max += sources[i].MaxAmount;
            count++;
        }

        return $"{key.Translate(count.Named("COUNT")).Resolve()} ({Percent(stored, max)})";
    }

    private static void OpenTargetMenu(Feruchemist gene) {
        List<FloatMenuOption> options = [
            new FloatMenuOption(
                GroupSummary(gene, "CC_Dock_Feruchemy_TargetAll", Feruchemist.TargetAll),
                () => gene.targetMetalmindId = Feruchemist.TargetAll
            ),
            new FloatMenuOption(
                GroupSummary(gene, "CC_Dock_Feruchemy_TargetInternal", Feruchemist.TargetInternal),
                () => gene.targetMetalmindId = Feruchemist.TargetInternal
            ),
            new FloatMenuOption(
                GroupSummary(gene, "CC_Dock_Feruchemy_TargetExternal", Feruchemist.TargetExternal),
                () => gene.targetMetalmindId = Feruchemist.TargetExternal
            ),
        ];

        List<IMetalmindSource> sources = gene.metalminds;
        for (int i = 0; i < sources.Count; i++) {
            IMetalmindSource source = sources[i];
            string id = source.SourceId;
            options.Add(
                new FloatMenuOption(
                    $"{source.SourceLabel} ({Percent(source.TotalStored, source.MaxAmount)})",
                    () => gene.targetMetalmindId = id
                )
            );
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    // Compounding is only offered on an implanted metalmind, because burning one
    // destroys it and a worn band is not what the pawn is setting alight. The
    // toggle parks the other pool so only one is ever moving.
    private float DrawCompoundToggle(Rect inner, float y, Pawn pawn, InvestitureCell cell, Feruchemist gene) {
        // The internal group is every implant, so compounding reaches it just as it
        // reaches a single one.
        bool eligible = gene.TargetIsInternalOnly;
        AcceptanceReport report = CompoundingAccess.Gate(pawn, gene);

        // Losing the implant or the gate mid-compound parks the pool rather than
        // leaving it draining behind a control the player can no longer see.
        if (gene.compounding && (!eligible || !report.Accepted)) {
            gene.compounding = false;
            gene.compoundedTargetValue = IdleTarget;
        }

        if (!eligible) return y - 6f;

        bool on = gene.compounding;
        Rect rect = new Rect(inner.x, y, inner.width, StripButtonHeight);

        TooltipHandler.TipRegion(
            rect,
            report.Accepted
                ? "CC_Dock_Feruchemy_CompoundTip".Translate()
                : "CC_Dock_Feruchemy_CompoundBlocked".Translate(
                    (report.Reason.NullOrEmpty()
                        ? "CC_Dock_Feruchemy_CompoundUnavailable".Translate().Resolve()
                        : report.Reason).Named("REASON")
                )
        );

        string label = on
            ? "CC_Dock_Feruchemy_StopCompound".Translate()
            : "CC_Dock_Feruchemy_Compound".Translate();
        if (!DockChrome.Button(rect, label, report.Accepted && eligible, CompoundTint)) return rect.yMax;

        // Carry whatever the dial was set to across, and no further. The rate is the
        // player's to choose, so an idle dial stays idle rather than being pegged
        // somewhere on their behalf.
        if (on) {
            gene.compounding = false;
            gene.targetValue = gene.compoundedTargetValue;
            gene.compoundedTargetValue = IdleTarget;
        } else {
            gene.compounding = true;
            gene.compoundedTargetValue = gene.targetValue;
            gene.targetValue = IdleTarget;
        }

        Event.current?.Use();

        return rect.yMax;
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
        } else if (delta > 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, width, rect.height), CompoundTint);
        }

        Widgets.DrawBoxSolid(
            new Rect(rect.center.x, rect.y - 1f, 1f, rect.height + 2f),
            new Color(0.353f, 0.322f, 0.271f)
        );
    }

    // Hand-drawn so the dial keeps the section's chrome. The vanilla slider
    // brings its own tan gradient, which fights everything around it.
    private void DrawDial(Rect rect, string metalId, Feruchemist gene, Capacity capacity, bool compounded) {
        if (compounded) DrawCompoundedDialBacking(rect, gene, capacity);
        else DrawDialBacking(rect, gene, capacity);

        float value = compounded ? gene.compoundedTargetValue : gene.targetValue;
        float handleX = rect.x + rect.width * (Mathf.Clamp(value, 0f, 100f) / 100f);
        Widgets.DrawBoxSolid(
            new Rect(handleX - 1.5f, rect.y - 2f, 3f, rect.height + 4f),
            compounded ? new Color(0.898f, 0.812f, 0.588f) : new Color(0.816f, 0.851f, 0.871f)
        );

        HandleDialDrag(rect, metalId, gene, capacity, compounded);
    }

    // Shared by both dials. Compounded runs tap-only, so its reachable span stops
    // at the idle point rather than continuing into the store half.
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
        } else if (delta > 0f) {
            float width = rect.width / 2f * Mathf.Clamp01(delta / IdleTarget);
            Widgets.DrawBoxSolid(new Rect(rect.center.x, rect.y, width, rect.height), StoreFill);
        }

        Widgets.DrawBoxSolid(
            new Rect(rect.center.x, rect.y - 1f, 1f, rect.height + 2f),
            new Color(0.353f, 0.322f, 0.271f)
        );
    }

    private string Tooltip(Pawn pawn, InvestitureCell cell, Capacity capacity) {
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

    private static Capacity CapacityOf(Feruchemist? gene) {
        if (gene == null) return default;

        List<IMetalmindSource> sources = gene.metalminds;
        float internalHeld = 0f;
        float externalHeld = 0f;
        float internalMax = 0f;
        float externalMax = 0f;
        float max = 0f;
        for (int i = 0; i < sources.Count; i++) {
            if (sources[i].IsImplanted) {
                internalHeld += sources[i].TotalStored;
                internalMax += sources[i].MaxAmount;
            } else {
                externalHeld += sources[i].TotalStored;
                externalMax += sources[i].MaxAmount;
            }

            max += sources[i].MaxAmount;
        }

        return new Capacity(
            internalHeld,
            externalHeld,
            internalMax,
            externalMax,
            max,
            sources.Count > 0,
            gene.canTap,
            gene.canStore,
            gene.canTapCompounded,
            gene.canStoreCompounded,
            gene.CompoundedRatePerSecond
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
            float internalHeld,
            float externalHeld,
            float internalMax,
            float externalMax,
            float max,
            bool hasMetalmind,
            bool canTap,
            bool canStore,
            bool canTapCompounded,
            bool canStoreCompounded,
            float compoundedRate
        ) {
            Internal = internalHeld;
            External = externalHeld;
            InternalMax = internalMax;
            ExternalMax = externalMax;
            Max = max;
            HasMetalmind = hasMetalmind;
            CanTap = canTap;
            CanStore = canStore;
            CanTapCompounded = canTapCompounded;
            CanStoreCompounded = canStoreCompounded;
            CompoundedRate = compoundedRate;
        }

        // Charge held in implanted metalminds.
        public float Internal { get; }

        // Charge held in worn and carried metalminds.
        public float External { get; }

        public float InternalMax { get; }

        public float ExternalMax { get; }

        // Each kind of metalmind reads against its own capacity, and a kind the
        // pawn has none of is left out rather than shown as a flat zero.
        public string Readout {
            get {
                bool hasInternal = InternalMax > 0f;
                bool hasExternal = ExternalMax > 0f;

                if (hasInternal && hasExternal) {
                    return $"{Internal / InternalMax * 100f:0}% + {External / ExternalMax * 100f:0}%";
                }

                if (hasInternal) return $"{Internal / InternalMax * 100f:0}%";
                if (hasExternal) return $"{External / ExternalMax * 100f:0}%";

                return "0%";
            }
        }

        // The same split in raw units, for the hover.
        public string UnitsReadout {
            get {
                bool hasInternal = InternalMax > 0f;
                bool hasExternal = ExternalMax > 0f;

                if (hasInternal && hasExternal) {
                    return $"{Internal:0}/{InternalMax:0} implanted, {External:0}/{ExternalMax:0} worn";
                }

                if (hasInternal) return $"{Internal:0}/{InternalMax:0} implanted";
                if (hasExternal) return $"{External:0}/{ExternalMax:0} worn";

                return "0";
            }
        }

        public float Max { get; }

        public bool HasMetalmind { get; }

        public bool CanTap { get; }

        public bool CanStore { get; }

        public bool CanTapCompounded { get; }

        public bool CanStoreCompounded { get; }

        // Positive while filling the pool, negative while burning the metalmind.
        public float CompoundedRate { get; }

        public float Fraction => Max > 0f ? (Internal + External) / Max : 0f;

        // Each band fills against its own capacity, so a full set of implants reads
        // as a full bar rather than as its share of the combined total. That keeps
        // the bars saying the same thing as the percentages above them.
        public float StoredFraction => ExternalMax > 0f ? External / ExternalMax : 0f;

        public float CompoundedFraction => InternalMax > 0f ? Internal / InternalMax : 0f;
    }
}
