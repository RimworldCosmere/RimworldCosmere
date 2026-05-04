using Verse;
using Cosmere.Core.Quickstart;
using Cosmere.System.Roshar.Extension;
using Cosmere.System.Roshar.Gene;

namespace Cosmere.System.Roshar.Quickstart;

[StaticConstructorOnStartup]
internal static class RosharQuickstartSetup {
    static RosharQuickstartSetup() {
        // 0: Wit — Lightweaver
        QuickstartCharacterSetupRegistry.Register(0, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantLightweaver, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 1: Dalinar — Bondsmith (Stormfather)
        QuickstartCharacterSetupRegistry.Register(1, pawn => {
            Surgebinder? bondsmith = pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, 5);
            if (bondsmith != null) {
                bondsmith.godsprenName = "Stormfather";
                bondsmith.UpdateAbilities();
            }
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed, 25);
        });

        // 2: Kaladin — Windrunner
        QuickstartCharacterSetupRegistry.Register(2, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 20;
        });

        // 3: Szeth — Skybreaker
        QuickstartCharacterSetupRegistry.Register(3, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantSkybreaker, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ArrestsMade, 5);
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ZoneComplianceDays, 30);
        });

        // 5: Malata — Dustbringer
        QuickstartCharacterSetupRegistry.Register(5, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantDustbringer, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 6: Lift — Edgedancer + Bondsmith (Nightwatcher)
        QuickstartCharacterSetupRegistry.Register(6, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantEdgedancer, 5);
            Surgebinder? bondsmith = pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, 3);
            if (bondsmith != null) {
                bondsmith.godsprenName = "Nightwatcher";
                bondsmith.UpdateAbilities();
            }
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 7: Renarin — Truthwatcher
        QuickstartCharacterSetupRegistry.Register(7, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantTruthwatcher, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 8: Jasnah — Elsecaller
        QuickstartCharacterSetupRegistry.Register(8, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantElsecaller, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 9: Venli — Willshaper
        QuickstartCharacterSetupRegistry.Register(9, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWillshaper, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 10: Tsazo — Stoneward
        QuickstartCharacterSetupRegistry.Register(10, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantStoneward, 5);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 11: Navani — Bondsmith (Sibling)
        QuickstartCharacterSetupRegistry.Register(11, pawn => {
            Surgebinder? bondsmith = pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, 3);
            if (bondsmith != null) {
                bondsmith.godsprenName = "Sibling";
                bondsmith.UpdateAbilities();
            }
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });
    }
}
