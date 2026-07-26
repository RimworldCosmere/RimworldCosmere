using System;
using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.Core.Lib.FloatSubMenu;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

public class Soulcast : SurgebindingAbility {
    private const float AirWallMultiplier = 3f;
    private const float GeyserCost = 120f;
    private const float GeyserRemoveCost = 15f;
    private const float PawnSoulcastCost = 60f;
    private const float CorpseSoulcastCost = 20f;
    private const float TerrainBaseCost = 10f;
    private const int StonecuttingYield = 20;

    public Soulcast(Pawn pawn) : base(pawn) { }
    public Soulcast(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    internal SoulcastMode? storedMode { get; set; }
    internal ThingDef? storedMaterial { get; set; }
    internal TerrainDef? storedTerrain { get; set; }

    private float BaseCost => def.beuPerTick / (1 << Gene.CurrentIdeal);

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        ShowMaterialPicker();
        if (status.IsActive) UpdateStatus(Active.Off);
        return true;
    }

    public void ExecuteStoredAction(LocalTargetInfo target) {
        if (!storedMode.HasValue) return;

        SoulcastOverlay.Remove(target.Cell, pawn.Map);

        if (!target.HasThing &&
            storedMode.Value is not (SoulcastMode.Wall or SoulcastMode.Terraform or SoulcastMode.Geyser)) {
            Verse.Thing? found = FindTargetThingAt(target.Cell, storedMode.Value);
            if (found != null) target = new LocalTargetInfo(found);
        }

        if (target.HasThing && IsBondedSpren(target.Thing)) {
            Messages.Message("Cannot soulcast a bonded spren.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        switch (storedMode.Value) {
            case SoulcastMode.ConvertDrop:
                if (storedMaterial != null && target.HasThing) DoConvertDrop(target.Thing, storedMaterial);
                break;
            case SoulcastMode.ChangeStuff:
                if (storedMaterial != null && target.HasThing) DoChangeStuffOnTarget(target.Thing, storedMaterial);
                break;
            case SoulcastMode.Wall:
                if (storedMaterial != null)
                    DoFortify(
                        target.Cell,
                        pawn.Map,
                        storedMaterial,
                        BaseCost * SoulcastMaterials.GetMaterialCost(storedMaterial) * AirWallMultiplier
                    );
                break;
            case SoulcastMode.Terraform:
                if (storedTerrain != null)
                    DoTerraform(
                        target.Cell,
                        pawn.Map,
                        storedTerrain,
                        TerrainBaseCost * SoulcastMaterials.GetTerrainCost(storedTerrain)
                    );
                break;
            case SoulcastMode.Geyser:
                DoSpawnGeyser(target.Cell, pawn.Map);
                break;
            case SoulcastMode.Sculpture:
                if (storedMaterial != null && target.HasThing) DoSculpture(target, storedMaterial);
                break;
            case SoulcastMode.Destroy:
                DoDestroy(target);
                break;
        }
    }

    private Verse.Thing? FindTargetThingAt(IntVec3 cell, SoulcastMode mode) {
        List<Verse.Thing> things = cell.GetThingList(pawn.Map);
        for (int i = 0; i < things.Count; i++) {
            Verse.Thing thing = things[i];
            if (thing.Destroyed) continue;
            if (IsBondedSpren(thing)) continue;
            bool valid = mode switch {
                SoulcastMode.ConvertDrop => thing.def.category == ThingCategory.Item ||
                                            thing.def.plant != null ||
                                            thing.def.mineable,
                SoulcastMode.ChangeStuff => thing.def.MadeFromStuff && thing.Stuff != null || thing.def.mineable,
                SoulcastMode.Sculpture => thing is Pawn or Corpse,
                SoulcastMode.Destroy => thing is not Mote,
                _ => false,
            };
            if (valid) return thing;
        }

        return null;
    }

    private static bool IsBondedSpren(Verse.Thing thing) {
        return thing is Pawn p && p.TryGetComp<SprenBond>() != null;
    }

    private void ShowMaterialPicker() {
        List<FloatMenuOption> options = [];

        List<ThingDef> dropOutputs = SoulcastMaterials.GetAvailableDropOutputs(Gene.CurrentIdeal);
        List<FloatMenuOption> matOpts = [];
        for (int i = 0; i < dropOutputs.Count; i++) {
            ThingDef mat = dropOutputs[i];
            float cost = BaseCost * SoulcastMaterials.GetMaterialCost(mat);
            matOpts.Add(PickerOption(mat.LabelCap, cost, () => StartTargeting(SoulcastMode.ConvertDrop, mat, null)));
        }

        options.Add(new FloatSubMenu("CRO_Soulcast_Category_Into".Translate(), matOpts));

        List<ThingDef> stuffList = SoulcastMaterials.GetAvailableStuffs(Gene.CurrentIdeal);
        List<ThingDef> stuffOnly = [];
        for (int i = 0; i < stuffList.Count; i++) {
            if (stuffList[i].IsStuff) stuffOnly.Add(stuffList[i]);
        }

        List<FloatMenuOption> stuffOpts = [];
        for (int i = 0; i < stuffOnly.Count; i++) {
            ThingDef mat = stuffOnly[i];
            float cost = BaseCost * SoulcastMaterials.GetMaterialCost(mat);
            stuffOpts.Add(PickerOption(mat.LabelCap, cost, () => StartTargeting(SoulcastMode.ChangeStuff, mat, null)));
        }

        options.Add(new FloatSubMenu("CRO_Soulcast_Category_Structure".Translate(), stuffOpts));

        List<FloatMenuOption> wallOpts = [];
        for (int i = 0; i < stuffOnly.Count; i++) {
            ThingDef mat = stuffOnly[i];
            float cost = BaseCost * SoulcastMaterials.GetMaterialCost(mat) * AirWallMultiplier;
            wallOpts.Add(PickerOption(mat.LabelCap, cost, () => StartTargeting(SoulcastMode.Wall, mat, null)));
        }

        options.Add(new FloatSubMenu("CRO_Soulcast_Category_AirToWall".Translate(), wallOpts));

        List<FloatMenuOption> terrainOpts = [];
        List<TerrainDef> terrains = SoulcastMaterials.GetTerrainOptions();
        for (int i = 0; i < terrains.Count; i++) {
            TerrainDef terrain = terrains[i];
            float cost = TerrainBaseCost * SoulcastMaterials.GetTerrainCost(terrain);
            terrainOpts.Add(
                PickerOption(terrain.LabelCap, cost, () => StartTargeting(SoulcastMode.Terraform, null, terrain))
            );
        }

        if (Gene.CurrentIdeal >= 2) {
            terrainOpts.Add(
                PickerOption(
                    "CRO_Soulcast_CreateGeyser".Translate(),
                    GeyserCost,
                    () => StartTargeting(SoulcastMode.Geyser, null, null)
                )
            );
        }

        options.Add(new FloatSubMenu("CRO_Soulcast_Category_Terraform".Translate(), terrainOpts));

        List<FloatMenuOption> sculptOpts = [];
        for (int i = 0; i < stuffOnly.Count; i++) {
            ThingDef mat = stuffOnly[i];
            sculptOpts.Add(
                PickerOption(mat.LabelCap, PawnSoulcastCost, () => StartTargeting(SoulcastMode.Sculpture, mat, null))
            );
        }

        options.Add(new FloatSubMenu("CRO_Soulcast_Category_Sculpture".Translate(), sculptOpts));

        float destroyCost = BaseCost;
        options.Add(
            PickerOption(
                "CRO_Soulcast_ToAir".Translate(),
                destroyCost,
                () => StartTargeting(SoulcastMode.Destroy, null, null)
            )
        );

        Find.WindowStack.Add(new PausingFloatMenu(options));
    }

    private void StartTargeting(SoulcastMode mode, ThingDef? material, TerrainDef? terrain) {
        storedMode = mode;
        storedMaterial = material;
        storedTerrain = terrain;

        Designator_Soulcast designator = new Designator_Soulcast(this, mode, material, terrain);
        Find.DesignatorManager.Select(designator);
    }

    private FloatMenuOption PickerOption(string label, float cost, Action onPick) {
        bool canAfford = Gene.CanLowerReserve(cost);
        string full = $"{label} ({"CRO_Soulcast_Cost".Translate(cost.ToString("F0"))})";
        if (canAfford) return new FloatMenuOption(full, onPick);
        return new FloatMenuOption($"{full} — {"CRO_Soulcast_NotEnoughInvestiture".Translate()}", null);
    }

    private void DoConvertDrop(Verse.Thing thing, ThingDef material) {
        if (thing.Destroyed) return;

        if (thing.def.mineable) {
            int yield = 1;
            if (thing.def.building?.mineableThing != null) {
                yield = Mathf.Max(1, thing.def.building.mineableYield);
            }

            float cost = BaseCost * SoulcastMaterials.GetMaterialCost(material);
            DestroyAndSpawn(thing, material, yield, cost);
            return;
        }

        if (thing.def.plant != null) {
            int yield = Mathf.Max(1, (int)thing.def.plant.harvestYield);
            float cost = BaseCost * SoulcastMaterials.GetMaterialCost(material);
            DestroyAndSpawn(thing, material, yield, cost);
            return;
        }

        ThingDef? blockDef = SoulcastMaterials.GetBlocksForChunk(thing.def);
        if (blockDef != null && material == blockDef) {
            float cost = BaseCost * 0.5f;
            DestroyAndSpawn(thing, blockDef, StonecuttingYield, cost);
            return;
        }

        float matCost = BaseCost * SoulcastMaterials.GetMaterialCost(material);
        ReplaceDrop(thing, material, matCost);
    }

    private void DoChangeStuffOnTarget(Verse.Thing thing, ThingDef material) {
        if (thing.Destroyed) return;

        if (thing.def.mineable) {
            ThingDef? newMineable = SoulcastMaterials.GetMineableForMaterial(material);
            if (newMineable == null || newMineable == thing.def) return;
            float cost = BaseCost * SoulcastMaterials.GetMaterialCost(material);
            ReplaceMineable(thing, newMineable, cost);
            return;
        }

        if (!thing.def.MadeFromStuff || thing.Stuff == material) return;
        float stuffCost = BaseCost * SoulcastMaterials.GetMaterialCost(material);
        ChangeStuff(thing, material, stuffCost);
    }

    private void DoSculpture(LocalTargetInfo target, ThingDef material) {
        if (!target.HasThing) return;

        if (target.Thing is Corpse corpse) {
            SoulcastCorpse(corpse, material, CorpseSoulcastCost);
            return;
        }

        if (target.Thing is Pawn targetPawn) {
            float hitChance = GetSoulcastHitChance(targetPawn);
            Find.WindowStack.Add(
                Dialog_MessageBox.CreateConfirmation(
                    "CRO_Soulcast_PawnWarning".Translate(
                        targetPawn.LabelShortCap.Named("PAWN"),
                        material.label.Named("MATERIAL"),
                        hitChance.ToString("P0").Named("CHANCE")
                    ),
                    () => SoulcastPawn(targetPawn, material, PawnSoulcastCost),
                    true,
                    "CRO_Soulcast_PawnWarningTitle".Translate()
                )
            );
        }
    }

    private void DoDestroy(LocalTargetInfo target) {
        if (!target.HasThing) return;
        Verse.Thing thing = target.Thing;
        if (thing.Destroyed) return;

        if (thing is Fire) {
            if (!TryPayCost(BaseCost * 0.5f)) return;
            SpawnFleck(thing.Position, pawn.Map);
            thing.Destroy();
            return;
        }

        if (thing.def == RimWorld.ThingDefOf.SteamGeyser) {
            if (!TryPayCost(GeyserRemoveCost)) return;
            IntVec3 pos = thing.Position;
            Map map = thing.Map;
            thing.Destroy();
            map.terrainGrid.SetTerrain(pos, TerrainDefOf.Soil);
            SpawnFleck(pos, map);
            return;
        }

        if (!TryPayCost(BaseCost)) return;
        SpawnFleck(thing.Position, pawn.Map);
        thing.Destroy();
    }

    private bool TryPayCost(float cost) {
        if (!Gene.CanLowerReserve(cost)) return false;
        Gene.RemoveFromReserve(cost);
        return true;
    }

    private bool TryPayCost(float cost, Verse.Thing target) {
        return !target.Destroyed && TryPayCost(cost);
    }

    private void SpawnFleck(IntVec3 pos, Map map) {
        FleckMaker.Static(pos, map, FleckDefOf.PsycastAreaEffect);
    }

    private void DestroyAndSpawn(Verse.Thing target, ThingDef outputDef, int count, float cost) {
        if (!TryPayCost(cost, target)) return;
        IntVec3 pos = target.Position;
        Map map = target.Map;
        target.Destroy();
        Verse.Thing result = ThingMaker.MakeThing(outputDef);
        result.stackCount = Mathf.Max(1, count);
        GenSpawn.Spawn(result, pos, map);
        SpawnFleck(pos, map);
    }

    private void ReplaceDrop(Verse.Thing target, ThingDef outputDef, float cost) {
        DestroyAndSpawn(target, outputDef, target.stackCount, cost);
    }

    private void ReplaceMineable(Verse.Thing target, ThingDef newMineableDef, float cost) {
        if (!TryPayCost(cost, target)) return;
        IntVec3 pos = target.Position;
        Map map = target.Map;
        target.Destroy();
        GenSpawn.Spawn(ThingMaker.MakeThing(newMineableDef), pos, map);
        SpawnFleck(pos, map);
    }

    private void ChangeStuff(Verse.Thing target, ThingDef newStuff, float cost) {
        if (!TryPayCost(cost, target)) return;
        target.SetStuffDirect(newStuff);
        target.HitPoints = target.MaxHitPoints;
        target.Notify_ColorChanged();
        SpawnFleck(target.Position, pawn.Map);
    }

    private void DoFortify(IntVec3 cell, Map map, ThingDef stuffDef, float cost) {
        if (!cell.Standable(map) || cell.GetFirstBuilding(map) != null) return;
        if (!TryPayCost(cost)) return;
        Verse.Thing wall = ThingMaker.MakeThing(RimWorld.ThingDefOf.Wall, stuffDef);
        wall.SetFactionDirect(pawn.Faction ?? Faction.OfPlayer);
        GenSpawn.Spawn(wall, cell, map);
        SpawnFleck(cell, map);
    }

    private void SpawnClaimedSculpture(ThingDef sculptureDef, ThingDef stuffDef, IntVec3 pos, Map map) {
        Verse.Thing sculpture = ThingMaker.MakeThing(sculptureDef, stuffDef);
        sculpture.SetFactionDirect(pawn.Faction ?? Faction.OfPlayer);
        GenSpawn.Spawn(sculpture, pos, map);
    }

    private void DoTerraform(IntVec3 cell, Map map, TerrainDef newTerrain, float cost) {
        if (!TryPayCost(cost)) return;
        List<Verse.Thing> thingsOnCell = cell.GetThingList(map);
        for (int i = thingsOnCell.Count - 1; i >= 0; i--) {
            if (thingsOnCell[i] is Fire fire) fire.Destroy();
        }

        map.snowGrid.SetDepth(cell, 0f);
        map.terrainGrid.SetTerrain(cell, newTerrain);
        SpawnFleck(cell, map);
    }

    private void DoSpawnGeyser(IntVec3 cell, Map map) {
        if (!TryPayCost(GeyserCost)) return;
        GenSpawn.Spawn(ThingMaker.MakeThing(RimWorld.ThingDefOf.SteamGeyser), cell, map);
        SpawnFleck(cell, map);
    }

    private void SoulcastPawn(Pawn target, ThingDef stuffDef, float cost) {
        if (target.Dead || target.Destroyed || !TryPayCost(cost)) return;
        float hitChance = GetSoulcastHitChance(target);
        if (!Rand.Chance(hitChance)) {
            Messages.Message(
                "CRO_Soulcast_Resisted".Translate(target.LabelShortCap.Named("PAWN")),
                target,
                MessageTypeDefOf.NegativeEvent
            );
            SpawnFleck(target.Position, pawn.Map);
            return;
        }

        IntVec3 pos = target.Position;
        Map map = target.Map;
        target.Kill(null);
        Verse.Thing? corpse = pos.GetThingList(map).Find(t => t is Corpse);
        corpse?.Destroy();
        ThingDef sculptureDef = ThingDefOf.SculptureLarge;
        SpawnClaimedSculpture(sculptureDef, stuffDef, pos, map);
        SpawnFleck(pos, map);
    }

    private void SoulcastCorpse(Corpse target, ThingDef stuffDef, float cost) {
        if (!TryPayCost(cost, target)) return;
        IntVec3 pos = target.Position;
        Map map = target.Map;
        Pawn? innerPawn = target.InnerPawn;
        target.Destroy();
        ThingDef sculptureDef = ThingDefOf.SculptureSmall;
        SpawnClaimedSculpture(sculptureDef, stuffDef, pos, map);
        SpawnFleck(pos, map);

        ThoughtDef? burialThought = ThoughtDefOf.Cosmere_Roshar_Thought_SoulcastBurial;
        if (burialThought != null && innerPawn != null) {
            foreach (Pawn colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists) {
                if (colonist.needs?.mood == null || colonist.relations == null) continue;
                PawnRelationDef? relation = colonist.GetMostImportantRelation(innerPawn);
                if (relation != null || colonist.Faction == innerPawn.Faction) {
                    colonist.needs.mood.thoughts.memories.TryGainMemory(burialThought);
                }
            }
        }
    }

    private float GetSoulcastHitChance(Pawn target) {
        float baseChance = 0.5f + Gene.CurrentIdeal * 0.1f;
        baseChance *= pawn.GetStatValue(StatDefOf.MeleeHitChance);
        baseChance *= 1f - target.GetStatValue(StatDefOf.MeleeDodgeChance);
        return Mathf.Clamp(baseChance, 0.05f, 0.95f);
    }
}