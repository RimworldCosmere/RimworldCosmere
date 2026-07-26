using Cosmere.Core;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.Core.Extension;
using Cosmere.Core.Util;
using Cosmere.System.Roshar;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.DefModExtension;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Extension;

public static class Pawn_GeneTrackerExtension {
    public static Pawn? SpawnBondedSpren(
        this Pawn radiant,
        RadiantOrderDef orderDef,
        string? customName = null,
        bool showNamingDialog = false
    ) {
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Honor)) return null;
        if (orderDef == RadiantOrderDefOf.Bondsmith) return null;
        if (orderDef.sprenNamePool.Count == 0) return null;

        string sprenDefName = "Cosmere_Roshar_Race_" + orderDef.sprenLabel.Replace(" ", "");
        PawnKindDef? sprenKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(sprenDefName);
        if (sprenKind == null) {
            Logger.Error($"Could not find bonded spren PawnKindDef: {sprenDefName}");
            return null;
        }

        PawnGenerationRequest request = new PawnGenerationRequest(
            sprenKind,
            radiant.Faction,
            forceGenerateNewPawn: true
        ) {
            ForceBodyType = BodyTypeDefOf.Thin,
            ForceNoGear = true,
            ForbidAnyTitle = true,
        };

        Pawn spren = PawnGenerator.GeneratePawn(request);

        SanitizeSpren(spren);

        string name = customName ?? orderDef.sprenNamePool.RandomElement();
        spren.Name = new NameSingle(name);

        if (spren.story != null) {
            spren.story.bodyType = BodyTypeDefOf.Thin;
            spren.story.headType = DefDatabase<HeadTypeDef>.AllDefsListForReading[0];
            spren.story.Title = orderDef.sprenLabel;
        }

        if (spren.skills != null) {
            List<SkillRecord> allSkills = spren.skills.skills;
            for (int i = 0; i < allSkills.Count; i++) {
                allSkills[i].Level = 0;
                allSkills[i].passion = Passion.None;
            }

            SkillRecord? social = spren.skills.GetSkill(RimWorld.SkillDefOf.Social);
            if (social != null) {
                social.Level = 10;
            }
        }

        if (radiant.Map != null) {
            IntVec3 cell = CellFinder.RandomSpawnCellForPawnNear(radiant.Position, radiant.Map, 2);
            GenSpawn.Spawn(spren, cell, radiant.Map);
        }

        SprenBond? bond = spren.TryGetComp<SprenBond>();
        bond?.SetupBond(radiant);

        SpiritWeb.Instance?.SetConnection(radiant, spren, 1f);

        if (showNamingDialog && Current.ProgramState == ProgramState.Playing) {
            Find.WindowStack.Add(new Dialog_NameSprenDialog(spren));
        }

        PawnRelationDef? nahelBondDef =
            DefDatabase<PawnRelationDef>.GetNamedSilentFail("Cosmere_Roshar_Relation_NahelBond");
        if (nahelBondDef != null) {
            radiant.relations.AddDirectRelation(nahelBondDef, spren);
            spren.relations.AddDirectRelation(nahelBondDef, radiant);
        }

        return spren;
    }

    private static void SanitizeSpren(Pawn spren) {
        if (spren.inventory != null) {
            spren.inventory.DestroyAll();
        }

        if (spren.equipment != null) {
            spren.equipment.DestroyAllEquipment();
        }

        if (spren.apparel != null) {
            spren.apparel.DestroyAll();
        }
    }

    public static Surgebinder? TryAddRadiantOrder(
        this Pawn_GeneTracker genes,
        GeneDef geneDef,
        int ideal = 0,
        bool xenogene = false,
        string? sprenName = null,
        bool showNamingDialog = false
    ) {
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Honor)) return null;

        Pawn pawn = genes.pawn;
        RadiantOrderDef? orderDef = geneDef.GetModExtension<RadiantOrder>()?.order;
        if (orderDef != null && pawn != null) {
            RadiantTracker? tracker = Current.Game?.GetComponent<RadiantTracker>();
            if (tracker != null && !tracker.CanRebond(pawn, orderDef.defName)) {
                Logger.Warning(
                    $"{pawn.NameShortColored} cannot rebond order {orderDef.defName} due to broken bond restrictions"
                );
                return null;
            }
        }

        Surgebinder gene = (Surgebinder)genes.EnsureGene(geneDef, xenogene);
        gene.CurrentIdeal = ideal;

        if (orderDef != null && pawn != null) {
            Pawn? spren = pawn.SpawnBondedSpren(orderDef, sprenName, showNamingDialog);
            if (spren != null) {
                gene.bondedSpren = spren;
            }
        }

        return gene;
    }

    public static Surgebinder? GetSurgebindingGeneForGem(this Pawn_GeneTracker genes, GemDef def) {
        GeneDef? geneDef = def.GetSurgebindingGene();

        return geneDef == null ? null : (Surgebinder)genes.GetGene(geneDef);
    }

    public static bool HasSurgebindingGeneForGem(this Pawn_GeneTracker genes, GemDef def) {
        return genes.HasActiveGene(def.GetSurgebindingGene());
    }

    public static Surgebinder? GetSurgebindingGeneForOrder(this Pawn_GeneTracker genes, RadiantOrderDef def) {
        GeneDef geneDef = def.GetSurgebindingGene();

        return genes.GetGene(geneDef) as Surgebinder;
    }

    public static bool HasSurgebindingGeneForOrder(this Pawn_GeneTracker genes, RadiantOrderDef def) {
        return genes.HasActiveGene(def.GetSurgebindingGene());
    }
}