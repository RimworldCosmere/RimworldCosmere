using Cosmere.Core.Ability;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Roshar.UI;

// Surge marks ship as flat gold line art, and GUI.color multiplies - white is the
// identity, so tinting a gold mark white leaves it gold. Repainting the pixels is
// the only way. Built on demand rather than at startup because SurgeDef resolves its
// icon inside a LongEventHandler callback, so there is no ordering to rely on.
internal static class SurgeMarks {
    private static readonly Dictionary<string, Texture2D?> Cache = new Dictionary<string, Texture2D?>();

    public static Texture2D? White(SurgeDef surge) {
        if (Cache.TryGetValue(surge.defName, out Texture2D? cached)) return cached;

        Texture2D? mark = surge.icon == null ? null : surge.icon.CloneTexture().Silhouette(Color.white);
        Cache[surge.defName] = mark;

        return mark;
    }
}

public sealed class SurgebindingDockSection : DockSectionBase {
    private const float HeaderHeight = 28f;

    // The dock body is 346px at its narrowest, once the accordion's padding and a
    // scrollbar come off, so the single 16f unit the UI rules suggest is too coarse
    // to use everywhere. Three steps instead: lines that belong together, blocks
    // that do not, and the break before the buttons.
    private const float Pad = 8f;
    private const float LineGap = 4f;
    private const float BlockGap = 12f;
    private const float ButtonGap = 16f;

    private const float GaugeHeight = 16f;
    private const float NotchGutter = 8f;
    private const float SurgeRowHeight = 44f;
    private const float SurgeIconSize = 28f;
    private const float AbilityRowHeight = 26f;
    private const float AbilityIconSize = 20f;
    private const float OathButtonHeight = 26f;
    private const float InfoButtonHeight = 22f;
    private const float TabRowHeight = 22f;
    private const float TabGap = 4f;

    private static readonly Color MutedText = new Color(0.522f, 0.612f, 0.706f);
    private static readonly Color DimText = new Color(0.365f, 0.427f, 0.502f);
    private static readonly Color LockedText = new Color(0.325f, 0.361f, 0.412f);
    private static readonly Color SecondaryAccent = new Color(0.302f, 0.396f, 0.478f);

    // Matched to the Metallic Arts tables rather than invented: a translucent wash so
    // the panel still shows through, a muted border, and an open cell marked by an
    // accent tint plus an edge bar - not by swapping the background out from under it.
    private const float CellPad = 6f;
    private const float StripPad = 6f;
    private const float ActiveWash = 0.18f;

    // The open Surge cell runs down through this gap into its detail panel, so the
    // panel clears the other Surge rather than butting against the whole row.
    private const float JoinGap = LineGap;

    private static readonly Color CellBack = new Color(0.055f, 0.075f, 0.110f, 0.38f);
    private static readonly Color CellBorder = new Color(0.220f, 0.286f, 0.353f);

    // A step darker than the cell, because the ability rows sit inside a panel drawn
    // in the cell's own fill - matching it would leave them invisible against it.
    private static readonly Color AbilityBack = new Color(0.047f, 0.063f, 0.090f, 0.45f);
    private static readonly Color AbilityBorder = new Color(0.204f, 0.259f, 0.318f);
    private static readonly Color HoverBorder = new Color(0.443f, 0.541f, 0.639f);
    private static readonly Color TabOn = new Color(0.141f, 0.192f, 0.235f);
    private static readonly Color TabOff = new Color(0.090f, 0.106f, 0.118f);
    private static readonly Color TabBorder = new Color(0.200f, 0.255f, 0.298f);
    private static readonly Color TabTextOff = new Color(0.435f, 0.502f, 0.565f);

    private readonly List<Surgebinder> bonds = [];
    private readonly List<SurgeCount> cachedSurges = [];
    private readonly List<AbilityDef> cachedGeneral = [];
    private readonly Reveal reveal = new Reveal();

    private string? cachedOrder;
    private int cachedIdeal = -1;
    private string? selectedOrder;
    private string? expandedSurge;

    // Which panel is on screen, which is not the same as which Surge the player has
    // open: a closing panel keeps drawing until it has finished sliding away.
    private string? revealedSurge;
    private float revealedHeight;

    // Picked while another Surge was still open, and held until that one has gone.
    private string? pendingSurge;

    // Where the open cell was drawn this frame. The panel needs it to know which span
    // of its own top edge to leave open.
    private Rect revealedCell;

    public override string SystemId => "Surgebinding";

    public override float GetHeaderHeight() {
        return HeaderHeight;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        Surgebinder? gene = ActiveBond(pawn);
        if (gene == null || snapshot.PrimaryBar == null) return 0f;

        RefreshAbilities(gene, gene.radiantOrderDef);
        StepReveal();

        float height = Pad
                       + Crest.HeightFor(SubtitleFor(gene) != null)
                       + BlockGap
                       + GaugeHeight
                       + NotchGutter
                       + Text.LineHeightOf(GameFont.Tiny)
                       + LineGap
                       + Text.LineHeightOf(GameFont.Tiny)
                       + BlockGap
                       + SurgeRowHeight
                       + BlockGap
                       + ButtonGap
                       + InfoButtonHeight
                       + Pad;

        // A second bond is rare enough that everyone else should not pay a row for it.
        if (bonds.Count > 1) height += TabRowHeight + BlockGap;

        // The Words only take up room while they are there to be spoken.
        if (gene.PendingOath) height += OathButtonHeight + LineGap;

        height += AbilityGridHeight(cachedGeneral.Count);

        if (revealedSurge != null) height += JoinGap + revealedHeight;

        return height;
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        Surgebinder? gene = ActiveBond(pawn);
        if (gene == null || snapshot.PrimaryBar == null) return;

        RadiantOrderDef order = gene.radiantOrderDef;
        RefreshAbilities(gene, order);
        StepReveal();

        Rect inner = new Rect(rect.x + Pad, rect.y + Pad, rect.width - Pad * 2f, rect.height - Pad * 2f);
        float y = inner.y;

        if (bonds.Count > 1) {
            DrawOrderTabs(new Rect(inner.x, y, inner.width, TabRowHeight));
            y += TabRowHeight + BlockGap;
        }

        string? subtitle = SubtitleFor(gene);
        Rect crestRect = new Rect(inner.x, y, inner.width, Crest.HeightFor(subtitle != null));
        Crest.Draw(
            crestRect,
            order.whiteIcon,
            order.LabelCap,
            subtitle,
            IdealLabel(gene.CurrentIdealDisplay),
            new CrestPalette(Color.white, MutedText, Skin.AccentColor)
        );

        // Stormlight is Stormlight in every order, so the fill stays on the Roshar
        // accent. The order's own colour runs from near-black to near-white across
        // the ten, which no fill or body text can survive.
        Rect gaugeRect = new Rect(inner.x, crestRect.yMax + BlockGap, inner.width, GaugeHeight);
        float target = TargetGauge.Draw(
            gaugeRect,
            gene.Value,
            gene.Max,
            gene.targetValue,
            new GaugePalette(
                Skin.BarBackgroundColor,
                Skin.BarFillColor,
                new Color(0.918f, 0.965f, 1.000f),
                new Color(0.478f, 0.290f, 0.290f)
            ),
            SystemId
        );
        if (!Mathf.Approximately(target, gene.targetValue)) gene.targetValue = target;

        float tinyHeight = Text.LineHeightOf(GameFont.Tiny);
        Rect readingRect = new Rect(inner.x, gaugeRect.yMax + NotchGutter, inner.width, tinyHeight);
        UIText.EllipsisLabel(
            readingRect,
            "CC_Dock_Gauge_Reading".Translate(
                Mathf.RoundToInt(gene.Value).Named("CURRENT"),
                Mathf.RoundToInt(gene.Max).Named("MAX"),
                gene.InvestitureLabel.Named("RESOURCE")
            ),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            MutedText
        );

        // The live numbers share the first line - what is in the reserve and how fast
        // it is going. The refill threshold drops to the second, next to the ceiling
        // caption, because it is a setting the player chose rather than a reading. The
        // sign is carried by the number format rather than the key, so a reserve that
        // is not draining reads "0.00/s" instead of a nonsensical "-0.00/s".
        UIText.EllipsisLabel(
            readingRect,
            "CC_Dock_Gauge_Rate".Translate(gene.DrainPerSecond.ToString("-0.00;-0.00;0.00")),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            gene.DrainPerSecond > 0f ? MutedText : DimText
        );

        Rect captionRect = new Rect(inner.x, readingRect.yMax + LineGap, inner.width, tinyHeight);
        UIText.EllipsisLabel(
            captionRect,
            CeilingCaption(gene, order),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            gene.PendingOath ? Skin.AccentColor : DimText
        );
        UIText.EllipsisLabel(captionRect, RefillLabel(gene), GameFont.Tiny, TextAnchor.MiddleRight, DimText);

        Rect surgeRow = new Rect(inner.x, captionRect.yMax + BlockGap, inner.width, SurgeRowHeight);
        DrawSurgeRow(surgeRow, pawn, order);
        y = surgeRow.yMax;

        // The clicked Surge opens beneath the pair rather than replacing them, so the
        // player never loses sight of which one they opened. It unfolds from the cell
        // itself: the open cell runs down through the join gap into the panel, and the
        // panel leaves its top edge open across that cell's span.
        SurgeDef? shown = SurgeByName(revealedSurge);
        if (shown != null && revealedHeight > 0.5f) {
            Rect strip = new Rect(inner.x, y + JoinGap, inner.width, revealedHeight);
            float full = StripHeightFor(shown);

            if (revealedHeight < full - 0.5f) {
                // Mid-reveal. Drawn at its finished size inside a clip only as tall as
                // it has opened, so it slides out from under the cell rather than
                // squashing the ability rows into a sliver.
                Widgets.BeginGroup(strip);
                DrawAbilityStrip(
                    new Rect(0f, 0f, strip.width, full),
                    new Rect(revealedCell.x - strip.x, 0f, revealedCell.width, revealedCell.height),
                    pawn,
                    gene,
                    order,
                    shown.abilities
                );
                Widgets.EndGroup();
            } else {
                DrawAbilityStrip(strip, revealedCell, pawn, gene, order, shown.abilities);
            }

            y = strip.yMax;
        }

        y += BlockGap;

        // Abilities the Oaths granted rather than either Surge. They belong to the
        // Radiant, not to a power, so they sit outside the Surge pair.
        DrawAbilityGrid(
            new Rect(inner.x, y, inner.width, AbilityGridHeight(cachedGeneral.Count)),
            pawn,
            gene,
            order,
            cachedGeneral
        );

        y += AbilityGridHeight(cachedGeneral.Count) + ButtonGap;

        if (gene.PendingOath) {
            Rect oathRect = new Rect(inner.x, y, inner.width, OathButtonHeight);
            TooltipHandler.TipRegion(oathRect, OathTooltip(gene, order));
            if (DockButton.Draw(oathRect, "CC_Dock_Oath_Speak".Translate(), Skin.AccentColor, true)) {
                Find.WindowStack.Add(new Dialog_RadiantOrderInfoDialog(pawn, gene, RadiantOrderInfoMode.SpeakOath));
            }

            y = oathRect.yMax + LineGap;
        }

        Rect infoRect = new Rect(inner.x, y, inner.width, InfoButtonHeight);
        if (DockButton.Draw(infoRect, "CC_Dock_Order_Info".Translate(), SecondaryAccent, false)) {
            Find.WindowStack.Add(new Dialog_RadiantOrderInfoDialog(pawn, gene, RadiantOrderInfoMode.View));
        }
    }

    private SurgeDef? SurgeByName(string? defName) {
        if (defName == null) return null;

        for (int i = 0; i < cachedSurges.Count; i++) {
            if (cachedSurges[i].Def.defName == defName) return cachedSurges[i].Def;
        }

        return null;
    }

    private static float StripHeightFor(SurgeDef surge) {
        return AbilityGridHeight(surge.abilities.Count) + StripPad * 2f;
    }

    // Idempotent, because height is asked for several times a frame. Reveal itself only
    // advances once per frame; this just re-reads where it got to. Must run after
    // RefreshAbilities, since the panel's finished height is an ability count.
    private void StepReveal() {
        // One panel at a time, in order: the open Surge rolls up, then the new one
        // drops. Swapping the contents mid-slide reads as a glitch rather than a swap.
        if (expandedSurge == null && pendingSurge != null && revealedHeight < 1f) {
            expandedSurge = pendingSurge;
            pendingSurge = null;
        }

        SurgeDef? open = SurgeByName(expandedSurge);
        revealedHeight = reveal.Toward(open == null ? 0f : StripHeightFor(open));
        if (expandedSurge != null) revealedSurge = expandedSurge;
        else if (revealedHeight < 1f) revealedSurge = null;
    }

    // Clicking the open Surge closes it; clicking the other closes it first and queues
    // the new one behind it.
    private void ToggleSurge(string defName) {
        if (expandedSurge == defName) {
            CloseSurge();
            return;
        }

        if (expandedSurge == null && revealedHeight < 1f) {
            expandedSurge = defName;
            pendingSurge = null;
            return;
        }

        expandedSurge = null;
        pendingSurge = defName;
    }

    private void CloseSurge() {
        expandedSurge = null;
        pendingSurge = null;
    }

    // A pawn can hold more than one Nahel bond. They share a single Stormlight
    // reserve but nothing else, so the body shows one bond at a time rather than
    // leaving the reading, the ceiling and the buttons pointing at whichever gene
    // happened to come first.
    private Surgebinder? ActiveBond(Pawn pawn) {
        bonds.Clear();
        if (pawn.genes == null) return null;

        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Surgebinder bond && !bond.Overridden) bonds.Add(bond);
        }

        if (bonds.Count == 0) return null;

        for (int i = 0; i < bonds.Count; i++) {
            if (bonds[i].radiantOrderDef.defName == selectedOrder) return bonds[i];
        }

        // Either nothing is chosen yet, or the chosen bond is gone - broken, or this
        // is simply a different pawn than the one last looked at.
        selectedOrder = bonds[0].radiantOrderDef.defName;
        CloseSurge();

        return bonds[0];
    }

    private void DrawOrderTabs(Rect row) {
        float width = (row.width - TabGap * (bonds.Count - 1)) / bonds.Count;

        for (int i = 0; i < bonds.Count; i++) {
            RadiantOrderDef order = bonds[i].radiantOrderDef;
            bool active = order.defName == selectedOrder;
            Rect tab = new Rect(row.x + i * (width + TabGap), row.y, width, row.height);

            Widgets.DrawBoxSolid(tab, active ? TabOn : TabOff);
            Widgets.DrawBoxSolidWithOutline(tab, Color.clear, active ? Skin.AccentColor : TabBorder);
            UIText.EllipsisLabel(
                tab,
                order.LabelCap,
                GameFont.Tiny,
                TextAnchor.MiddleCenter,
                active ? Skin.HeaderTextColor : TabTextOff
            );

            TooltipHandler.TipRegion(
                tab,
                "CC_Dock_Order_Tab_Tip".Translate(
                    order.LabelCap.Named("ORDER"),
                    IdealLabel(bonds[i].CurrentIdealDisplay).Named("IDEAL")
                )
            );
            Widgets.DrawHighlightIfMouseover(tab);
            MouseoverSounds.DoRegion(tab);

            if (!Widgets.ButtonInvisible(tab)) continue;

            selectedOrder = order.defName;
            CloseSurge();
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            Event.current?.Use();
        }
    }

    // The spren is the other half of the bond, so it belongs under the order name
    // rather than buried in a dialog. An unbonded Surgebinder simply loses the line.
    private static string? SubtitleFor(Surgebinder gene) {
        string? name = gene.bondedSpren?.Name?.ToStringShort;
        if (name.NullOrEmpty()) name = gene.godsprenName.NullOrEmpty() ? null : gene.godsprenName;
        if (name.NullOrEmpty()) return null;

        return "CC_Dock_Crest_BondedTo".Translate(name!.Named("SPREN")).Resolve();
    }

    // One key per Ideal rather than a computed ordinal: "Third Ideal" is how the
    // books say it, and ordinals do not survive translation as arithmetic.
    private static string IdealLabel(int display) {
        return ("CC_Dock_Crest_Ideal" + Mathf.Clamp(display, 1, 5)).Translate().Resolve();
    }

    // A threshold of zero is off, not low. Saying "refill below 0%" reads as a
    // broken control rather than as a setting the player has not chosen yet.
    private static string RefillLabel(Surgebinder gene) {
        return gene.targetValue <= 0f || gene.Max <= 0f
            ? "CC_Dock_Gauge_NoRefill".Translate()
            : "CC_Dock_Gauge_RefillBelow".Translate(
                Mathf.RoundToInt(gene.targetValue / gene.Max * 100f).Named("PERCENT")
            );
    }

    // Capacity rises with each Ideal, so what the next Oath is worth is worth
    // stating. The gauge itself stays honest and to scale.
    private static string CeilingCaption(Surgebinder gene, RadiantOrderDef order) {
        int next = gene.CurrentIdeal + 1;
        if (next >= order.ideals.Count || order.ideals[next].stormlightMax <= 0) {
            return "CC_Dock_Gauge_AllSpoken".Translate();
        }

        return "CC_Dock_Gauge_NextCeiling".Translate(
            IdealLabel(next + 1).Named("IDEAL"),
            order.ideals[next].stormlightMax.Named("MAX")
        );
    }

    private static string OathTooltip(Surgebinder gene, RadiantOrderDef order) {
        int next = Mathf.Min(gene.CurrentIdeal + 1, order.ideals.Count - 1);
        return "CC_Dock_Oath_Tip".Translate(
            IdealLabel(next + 1).Named("IDEAL"),
            order.ideals[next].description.Named("WORDS")
        );
    }

    // Two Surges per order, always. Clicking one opens what it can actually do -
    // the counts say how many, which is no use when you want to know which.
    private void DrawSurgeRow(Rect row, Pawn pawn, RadiantOrderDef order) {
        if (cachedSurges.Count == 0) return;

        float cellWidth = (row.width - LineGap * (cachedSurges.Count - 1)) / cachedSurges.Count;

        for (int i = 0; i < cachedSurges.Count; i++) {
            SurgeCount surge = cachedSurges[i];
            Rect cell = new Rect(row.x + i * (cellWidth + LineGap), row.y, cellWidth, row.height);

            // Tracked against what is on screen rather than what is selected, so a
            // closing cell stays merged for the whole of its panel's slide away.
            bool open = surge.Def.defName == revealedSurge;

            if (open) {
                revealedCell = cell;
                Panel.DrawJoinedDown(
                    cell,
                    CellBack,
                    Skin.AccentColor,
                    JoinGap,
                    Skin.AccentColor,
                    ActiveWash,
                    false
                );
            } else {
                Panel.Draw(cell, CellBack, CellBorder);
            }

            Rect icon = new Rect(
                cell.x + CellPad,
                cell.y + (cell.height - SurgeIconSize) / 2f,
                SurgeIconSize,
                SurgeIconSize
            );

            Texture2D? mark = SurgeMarks.White(surge.Def);
            if (mark != null) {
                Color previous = GUI.color;
                GUI.color = Color.white;
                GUI.DrawTexture(icon, mark);
                GUI.color = previous;
            }

            string count = "CC_Dock_Surge_Readiness".Translate(
                surge.Unlocked.Named("UNLOCKED"),
                surge.Total.Named("TOTAL")
            );

            float countWidth;
            using (new TextBlock(GameFont.Tiny)) {
                countWidth = Text.CalcSize(count).x + 4f;
            }

            bool ready = !AnyOnCooldown(pawn, surge.Def);

            float textRight = cell.xMax - CellPad;
            UIText.EllipsisLabel(
                new Rect(icon.xMax + CellPad, cell.y, textRight - icon.xMax - CellPad - countWidth, cell.height),
                surge.Def.LabelCap,
                GameFont.Tiny,
                TextAnchor.MiddleLeft,
                ready ? MutedText : DimText
            );
            UIText.EllipsisLabel(
                new Rect(textRight - countWidth, cell.y, countWidth, cell.height),
                count,
                GameFont.Tiny,
                TextAnchor.MiddleRight,
                open ? Skin.AccentColor : ready ? MutedText : DimText
            );

            TooltipHandler.TipRegion(
                cell,
                "CC_Dock_Surge_Tip".Translate(
                    surge.Def.LabelCap.Named("SURGE"),
                    surge.Unlocked.Named("UNLOCKED"),
                    surge.Total.Named("TOTAL")
                )
            );
            Panel.Hover(cell, HoverBorder, open ? JoinGap : 0f);
            MouseoverSounds.DoRegion(cell);

            if (!Widgets.ButtonInvisible(cell)) continue;

            ToggleSurge(surge.Def.defName);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            Event.current?.Use();
        }
    }

    // Two across, because one ability per full-width row wastes most of the panel and
    // makes four abilities look like a wall.
    private static float AbilityGridHeight(int count) {
        if (count == 0) return 0f;

        int rows = (count + 1) / 2;
        return rows * AbilityRowHeight + (rows - 1) * LineGap;
    }

    private void DrawAbilityStrip(
        Rect rect,
        Rect openCell,
        Pawn pawn,
        Surgebinder gene,
        RadiantOrderDef order,
        List<AbilityDef> abilities
    ) {
        // Same fill and stroke as the Surge cell, with its top edge left open across
        // that cell's span. The two are one surface the Surge unfolded into, not a
        // second box parked underneath it.
        // No edge rail. On a metal the rail reports burning, a live state worth
        // catching at a glance; a Surge has no such state, so here it would only be
        // restating that the panel the player just opened is open.
        Panel.DrawNotchedTop(
            rect,
            CellBack,
            Skin.AccentColor,
            openCell.x,
            openCell.xMax,
            Skin.AccentColor,
            ActiveWash,
            false
        );

        DrawAbilityGrid(rect.ContractedBy(StripPad), pawn, gene, order, abilities);
    }

    private void DrawAbilityGrid(
        Rect rect,
        Pawn pawn,
        Surgebinder gene,
        RadiantOrderDef order,
        List<AbilityDef> abilities
    ) {
        float cellWidth = (rect.width - LineGap) / 2f;

        for (int i = 0; i < abilities.Count; i++) {
            Rect cell = new Rect(
                rect.x + i % 2 * (cellWidth + LineGap),
                rect.y + i / 2 * (AbilityRowHeight + LineGap),
                cellWidth,
                AbilityRowHeight
            );
            DrawAbilityCell(cell, pawn, abilities[i], gene, order);
        }
    }

    // One ability: what it is, whether the pawn can actually use it right now, and a
    // way to use it. A locked ability still shows, because knowing what is an Oath away
    // is the point - it just does not answer the mouse.
    private void DrawAbilityCell(
        Rect row,
        Pawn pawn,
        AbilityDef def,
        Surgebinder gene,
        RadiantOrderDef order
    ) {
        Rect hit = row;

        int minIdeal = def is SurgebindingAbilityDef surgeDef ? surgeDef.GetMinIdealForOrder(order.defName) : 0;
        bool locked = gene.CurrentIdeal < minIdeal;

        // A toggleable ability that is running is a live state, so it takes the accent
        // and the edge rail - the same language a burning metal uses.
        bool running = pawn.abilities?.GetAbility(def) is IToggleableAbility {
            IsToggleable: true,
            IsActive: true,
        };

        Panel.Draw(row, AbilityBack, running ? Skin.AccentColor : AbilityBorder);
        if (running) Panel.Active(row, Skin.AccentColor, ActiveWash);
        if (!locked) Panel.Hover(row, HoverBorder);

        row = row.ContractedBy(CellPad, 0f);

        Rect icon = new Rect(row.x, row.y + (row.height - AbilityIconSize) / 2f, AbilityIconSize, AbilityIconSize);
        if (def.uiIcon != null) {
            Color previous = GUI.color;
            GUI.color = locked ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
            GUI.DrawTexture(icon, def.uiIcon);
            GUI.color = previous;
        }

        string note = locked
            ? "CC_Dock_Ability_Locked".Translate(IdealLabel(minIdeal + 1).Named("IDEAL")).Resolve()
            : CooldownNote(pawn, def);

        float noteWidth = 0f;
        if (!note.NullOrEmpty()) {
            using (new TextBlock(GameFont.Tiny)) {
                noteWidth = Text.CalcSize(note).x + 6f;
            }
        }

        UIText.EllipsisLabel(
            new Rect(icon.xMax + 6f, row.y, row.xMax - icon.xMax - 6f - noteWidth, row.height),
            def.LabelCap,
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            locked ? LockedText : MutedText
        );

        if (!note.NullOrEmpty()) {
            UIText.EllipsisLabel(
                new Rect(row.xMax - noteWidth, row.y, noteWidth, row.height),
                note,
                GameFont.Tiny,
                TextAnchor.MiddleRight,
                LockedText
            );
        }

        if (!def.description.NullOrEmpty()) TooltipHandler.TipRegion(row, def.description);

        // Locked abilities are shown for information only, so they take no click. The
        // rest go through the same dispatcher the ability wheel uses, which is what
        // keeps toggling off, affordability and targeting behaving identically here.
        if (locked) return;

        MouseoverSounds.DoRegion(hit);
        if (!Widgets.ButtonInvisible(hit)) return;

        RadialDispatcher.CastOrToggle(pawn, def);
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        Event.current?.Use();
    }

    private static string CooldownNote(Pawn pawn, AbilityDef def) {
        if (pawn.abilities == null) return string.Empty;

        List<Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i].def != def) continue;
            int ticks = all[i].CooldownTicksRemaining;

            return ticks <= 0
                ? string.Empty
                : "CC_Dock_Ability_Cooldown".Translate(ticks.ToStringTicksToPeriod().Named("TIME")).Resolve();
        }

        return string.Empty;
    }

    // Surge counts and the Oath-granted list only move when an Oath is spoken, so
    // both are cached against the order and the Ideal.
    private void RefreshAbilities(Surgebinder gene, RadiantOrderDef order) {
        if (cachedOrder == order.defName && cachedIdeal == gene.CurrentIdeal) return;

        cachedOrder = order.defName;
        cachedIdeal = gene.CurrentIdeal;
        cachedSurges.Clear();
        cachedGeneral.Clear();

        List<SurgeDef> surges = order.surges;
        for (int i = 0; i < surges.Count; i++) {
            List<AbilityDef> abilities = surges[i].abilities;
            int unlocked = 0;
            for (int j = 0; j < abilities.Count; j++) {
                int minIdeal = abilities[j] is SurgebindingAbilityDef def
                    ? def.GetMinIdealForOrder(order.defName)
                    : 0;
                if (gene.CurrentIdeal >= minIdeal) unlocked++;
            }

            cachedSurges.Add(new SurgeCount(surges[i], unlocked, abilities.Count));
        }

        // The order's own list plus everything the Oaths have granted so far. Neither
        // belongs to a Surge, which is what makes them the general set.
        for (int i = 0; i < order.abilities.Count; i++) cachedGeneral.Add(order.abilities[i]);

        for (int i = 0; i <= Mathf.Min(gene.CurrentIdeal, order.ideals.Count - 1); i++) {
            List<AbilityDef> granted = order.ideals[i].abilities;
            for (int j = 0; j < granted.Count; j++) {
                if (!cachedGeneral.Contains(granted[j])) cachedGeneral.Add(granted[j]);
            }
        }
    }

    private static bool AnyOnCooldown(Pawn pawn, SurgeDef surge) {
        if (pawn.abilities == null) return false;

        List<Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i].CooldownTicksRemaining <= 0) continue;

            List<AbilityDef> abilities = surge.abilities;
            for (int j = 0; j < abilities.Count; j++) {
                if (abilities[j] == all[i].def) return true;
            }
        }

        return false;
    }

    private readonly struct SurgeCount {
        public SurgeCount(SurgeDef def, int unlocked, int total) {
            Def = def;
            Unlocked = unlocked;
            Total = total;
        }

        public SurgeDef Def { get; }

        public int Unlocked { get; }

        public int Total { get; }
    }
}
