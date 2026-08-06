using System.Collections.Generic;
using Concord;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.Core.UI;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Patch;

/// <summary>
///     Puts the world and Shard pickers on the world-generation page, replacing the separate
///     SelectShards page.
/// </summary>
/// <remarks>
///     Draws at At.Return rather than inside vanilla's group. Vanilla walks its left column with
///     a local cursor that no injection can read, so the rows are anchored from the bottom of
///     that column instead - deterministic, and immune to vanilla adding or removing rows. The
///     column is 628px tall and vanilla's densest case ends at y=350, so there is room.
///     <para>
///         The world is committed at At.Head of CanDoNext, which runs before the queued long
///         event that calls WorldGenerator.GenerateWorld. The Game and its components already
///         exist by then - Page_SelectScenario.BeginScenarioConfiguration creates the Game
///         before the page chain is even built.
///     </para>
/// </remarks>
[Patch]
public abstract class WorldParamsPatch : Page_CreateWorldParams {
    private const float RowHeight = 30f;
    private const float RowPitch = 40f;
    private const float LabelWidth = 200f;

    /// <summary>
    ///     The world the faction list was last built for. Static because only one worldgen page
    ///     is ever open, and because an instance field on a patch declaration does not exist on
    ///     the real object.
    /// </summary>
    private static CosmereWorldDef? factionsBuiltFor;

    /// <summary>
    ///     Rebuilds the configurable-faction list from FactionGenerator.ConfigurableFactions,
    ///     which the world gate filters. Not just display - this list is passed straight into
    ///     WorldGenerator.GenerateWorld, so a stale one generates the previous world's factions.
    /// </summary>
    [InjectMethod("ResetFactionCounts")]
    protected abstract void RebuildFactionCounts();

    [Inject(At.Return, nameof(DoWindowContents))]
    private void AfterDoWindowContents(Rect rect) {
        List<CosmereWorldDef> worlds = WorldUtility.All;
        if (worlds.Count == 0) return;

        CosmereWorld? comp = WorldUtility.component;
        if (comp == null) return;

        // Seeding here rather than in Reset covers back-navigation for free: returning to the
        // scenario page builds a fresh Game, so the component comes back null and re-seeds on
        // the next draw.
        if (comp.primary == null) WorldUtility.SeedFromScenario();

        // Picking a world happens in a FloatMenu callback, which runs outside this injection
        // wrapper and so cannot reach RebuildFactionCounts. Notice the change here instead.
        if (!ReferenceEquals(factionsBuiltFor, comp.primary)) {
            factionsBuiltFor = comp.primary;
            RebuildFactionCounts();
        }

        Rect main = GetMainRect(rect);
        float columnWidth = (main.width - Margin) * 0.5f;
        float controlWidth = columnWidth - LabelWidth;

        // Two rows at the bottom of the left column. Vanilla's own rows grow downward from the
        // top, so this stays clear of them and of the buttons below the column.
        int rowCount = comp.primary?.crossWorld == true ? 3 : 2;
        float y = main.yMax - RowPitch * rowCount;
        if (y < main.y) return;

        bool locked = !ScenarioDefUtility.AllowsChange;
        bool showWorldRow = worlds.Count > 1;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
            if (showWorldRow) {
                DrawWorldRow(main.x, y, controlWidth, worlds, comp, locked);
            }

            DrawShardRow(main.x, y + RowPitch, controlWidth, comp, locked);

            // Only the cross-world save asks this. Every other world answers it for itself -
            // you do not choose whether Roshar has highstorms.
            if (comp.primary?.crossWorld == true) {
                DrawFeatureRow(main.x, y + RowPitch * 2, controlWidth, locked);
            }
        }
    }

    [Inject(At.Head, nameof(CanDoNext))]
    private Control BeforeCanDoNext(ControlHandle<bool> ch) {
        CosmereWorldDef? world = WorldUtility.Primary ?? WorldUtility.SeedFromScenario();

        if (world == null) {
            Messages.Message("CC_World_NoneChosen".Translate(), MessageTypeDefOf.RejectInput, false);
            ch.ReturnValue = false;
            return Control.Cancel;
        }

        Logger.Important($"World generation starting on {world.defName}.");
        return Control.Continue;
    }

    private static void DrawWorldRow(
        float x, float y, float controlWidth, List<CosmereWorldDef> worlds, CosmereWorld comp, bool locked
    ) {
        Widgets.Label(new Rect(x, y, LabelWidth, RowHeight), "CC_World_Label".Translate());

        Rect control = new Rect(x + LabelWidth, y, controlWidth, RowHeight);
        string label = comp.primary?.LabelCap ?? "CC_World_None".Translate().Resolve();

        if (locked) {
            DrawLocked(control, label);
            return;
        }

        if (!Widgets.ButtonText(control, label)) return;

        List<FloatMenuOption> options = [];
        for (int i = 0; i < worlds.Count; i++) {
            CosmereWorldDef world = worlds[i];
            options.Add(new FloatMenuOption(world.LabelCap, () => Choose(world)));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static void DrawShardRow(float x, float y, float controlWidth, CosmereWorld comp, bool locked) {
        Widgets.Label(new Rect(x, y, LabelWidth, RowHeight), "CC_World_ShardsLabel".Translate());

        Rect control = new Rect(x + LabelWidth, y, controlWidth, RowHeight);
        string label = "CC_World_Edit".Translate();

        if (locked) {
            DrawLocked(control, label);
            return;
        }

        if (!Widgets.ButtonText(control, label)) return;

        // Every Shard, not just this world's. Which Shards are active is a fact about the
        // cosmere the save runs in; the world only decides where their magic functions and what
        // a pawn born here is Connected to. Enabling Honor on Scadrial is how you get a Roshar
        // to travel to later.
        Find.WindowStack.Add(new Dialog_SelectShards(DefDatabase<ShardDef>.AllDefsListForReading));
    }

    private static void DrawFeatureRow(float x, float y, float controlWidth, bool locked) {
        Widgets.Label(new Rect(x, y, LabelWidth, RowHeight), "CC_World_FeaturesLabel".Translate());

        Rect control = new Rect(x + LabelWidth, y, controlWidth, RowHeight);
        string label = "CC_World_Edit".Translate();

        if (locked) {
            DrawLocked(control, label);
            return;
        }

        if (Widgets.ButtonText(control, label)) Find.WindowStack.Add(new Dialog_CosmereFeatures());
    }

    private static void DrawLocked(Rect control, string label) {
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, ColoredText.SubtleGrayColor)) {
            Widgets.DrawBoxSolid(control, new Color(0.14f, 0.14f, 0.16f, 0.55f));
            Widgets.Label(control, label);
        }

        TooltipHandler.TipRegion(control, "CC_World_LockedTip".Translate());
    }

    /// <summary>
    ///     Picks a world and switches its Shards on. Vanilla's EnableShard disables everything a
    ///     Shard conflicts with, so applying a world's era set in sequence is only safe because
    ///     CosmereWorldDef.ConfigErrors rejects a self-conflicting set at load.
    /// </summary>
    private static void Choose(CosmereWorldDef world) {
        RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        WorldUtility.Set(world);
        ApplyDefaultShards(world);

        // The faction list is rebuilt on the next draw - see factionsBuiltFor.
    }

    private static void ApplyDefaultShards(CosmereWorldDef world) {
        Shards? shards = ShardUtility.shards;
        if (shards == null) return;

        // A scenario's own list wins - it is what PreSelectShardForScenarioPatch already applied.
        if (ScenarioDefUtility.CurrentShards?.shards is { Count: > 0 }) return;

        List<ShardDef> starting = WorldUtility.StartingShards(world, ScenarioDefUtility.CurrentEra);
        List<ShardDef> permitted = world.crossWorld ? AllPermitted() : world.nativeShards;

        for (int i = 0; i < permitted.Count; i++) {
            shards.DisableShard(permitted[i]);
        }

        for (int i = 0; i < starting.Count; i++) {
            shards.EnableShard(starting[i]);
        }
    }

    private static List<ShardDef> AllPermitted() {
        List<ShardDef> all = [];
        List<CosmereWorldDef> worlds = DefDatabase<CosmereWorldDef>.AllDefsListForReading;
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) continue;
            for (int j = 0; j < worlds[i].nativeShards.Count; j++) {
                if (!all.Contains(worlds[i].nativeShards[j])) all.Add(worlds[i].nativeShards[j]);
            }
        }

        return all;
    }
}
