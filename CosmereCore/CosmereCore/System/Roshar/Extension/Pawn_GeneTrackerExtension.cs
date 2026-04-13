using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Extension;

public static class Pawn_GeneTrackerExtension {
    public static Verse.Pawn? SpawnBondedSpren(this Verse.Pawn radiant, RadiantOrderDef orderDef, string? customName = null, bool showNamingDialog = false) {
        if (orderDef.defName == "Bondsmith") return null;
        if (orderDef.sprenNamePool.Count == 0) return null;

        string sprenDefName = "Cosmere_Roshar_Race_" + orderDef.sprenLabel.Replace(" ", "");
        PawnKindDef? sprenKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(sprenDefName);
        if (sprenKind == null) {
            Cosmere.Core.Logger.Error($"Could not find bonded spren PawnKindDef: {sprenDefName}");
            return null;
        }

        Verse.Pawn spren = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
            sprenKind,
            radiant.Faction,
            PawnGenerationContext.NonPlayer,
            forceGenerateNewPawn: true
        ));

        string name = customName ?? orderDef.sprenNamePool.RandomElement();
        spren.Name = new NameSingle(name);

        if (spren.story != null) {
            spren.story.bodyType ??= BodyTypeDefOf.Thin;
            spren.story.headType ??= DefDatabase<HeadTypeDef>.AllDefsListForReading[0];
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

        CompSprenBond? bond = spren.TryGetComp<CompSprenBond>();
        bond?.SetupBond(radiant);

        SpiritWeb.Instance?.SetConnection(radiant, spren, 1f);

        if (showNamingDialog && Current.ProgramState == ProgramState.Playing) {
            Find.WindowStack.Add(new Cosmere.System.Roshar.Dialog.NameSprenDialog(spren));
        }

        PawnRelationDef? nahelBondDef = DefDatabase<PawnRelationDef>.GetNamedSilentFail("Cosmere_Roshar_Relation_NahelBond");
        if (nahelBondDef != null) {
            radiant.relations.AddDirectRelation(nahelBondDef, spren);
            spren.relations.AddDirectRelation(nahelBondDef, radiant);
        }

        return spren;
    }

    public static Surgebinder? TryAddRadiantOrder(
        this Pawn_GeneTracker genes,
        GeneDef geneDef,
        int ideal = 0,
        bool xenogene = false,
        string? sprenName = null,
        bool showNamingDialog = false
    ) {
        Verse.Pawn pawn = genes.pawn;
        RadiantOrderDef? orderDef = geneDef.GetModExtension<DefModExtension.RadiantOrder>()?.order;
        if (orderDef != null && pawn != null) {
            RadiantTracker? tracker = Current.Game?.GetComponent<RadiantTracker>();
            if (tracker != null && !tracker.CanRebond(pawn, orderDef.defName)) {
                Cosmere.Core.Logger.Warning(
                    $"{pawn.NameShortColored} cannot rebond order {orderDef.defName} due to broken bond restrictions"
                );
                return null;
            }
        }

        Surgebinder gene = (Surgebinder)genes.TryAddGene(geneDef, xenogene);
        gene.currentIdeal = ideal;

        if (orderDef != null && pawn != null) {
            Verse.Pawn? spren = pawn.SpawnBondedSpren(orderDef, sprenName, showNamingDialog);
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

        return (Surgebinder)genes.GetGene(geneDef);
    }

    public static bool HasSurgebindingGeneForOrder(this Pawn_GeneTracker genes, RadiantOrderDef def) {
        return genes.HasActiveGene(def.GetSurgebindingGene());
    }
}