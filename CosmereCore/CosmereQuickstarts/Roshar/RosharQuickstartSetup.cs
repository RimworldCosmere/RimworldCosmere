using Cosmere.Core.Quickstart;
using Cosmere.System.Roshar.Extension;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Quickstart;

[StaticConstructorOnStartup]
internal static class RosharQuickstartSetup {
    /// <summary>
    ///     TryAddRadiantOrder takes the zero-based CurrentIdeal, so the Third Ideal is 2. Every pawn
    ///     here used to pass 5, out of range and silently clamped to the Fifth, maxing the roster.
    /// </summary>
    private const int FirstIdeal = 0;
    private const int SecondIdeal = 1;
    private const int ThirdIdeal = 2;
    private const int FourthIdeal = 3;
    private const int FifthIdeal = 4;

    static RosharQuickstartSetup() {
        // 0: Wit — Lightweaver
        QuickstartCharacterSetupRegistry.Register(0, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantLightweaver, ThirdIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 1: Dalinar — Bondsmith (Stormfather)
        QuickstartCharacterSetupRegistry.Register(1, pawn => {
            Surgebinder? bondsmith =
                pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, ThirdIdeal);
            if (bondsmith != null) {
                bondsmith.godsprenName = "Stormfather";
                bondsmith.UpdateAbilities();
            }

            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed, 25);
        });

        // 2: Kaladin — Windrunner
        QuickstartCharacterSetupRegistry.Register(2, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner, FifthIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 20;
        });

        // 3: Szeth — Skybreaker
        QuickstartCharacterSetupRegistry.Register(3, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantSkybreaker, FifthIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ArrestsMade, 5);
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ZoneComplianceDays, 30);
        });

        // 5: Malata — Dustbringer
        QuickstartCharacterSetupRegistry.Register(5, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantDustbringer, ThirdIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 6: Lift — Edgedancer + Bondsmith (Nightwatcher)
        QuickstartCharacterSetupRegistry.Register(6, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantEdgedancer, ThirdIdeal);
            Surgebinder? bondsmith =
                pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, FirstIdeal);
            if (bondsmith != null) {
                bondsmith.godsprenName = "Nightwatcher";
                bondsmith.UpdateAbilities();
            }

            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 7: Renarin — Truthwatcher
        QuickstartCharacterSetupRegistry.Register(7, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantTruthwatcher, ThirdIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 8: Jasnah — Elsecaller
        QuickstartCharacterSetupRegistry.Register(8, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantElsecaller, FourthIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 9: Venli — Willshaper
        QuickstartCharacterSetupRegistry.Register(9, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWillshaper, SecondIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 10: Tsazo — Stoneward
        QuickstartCharacterSetupRegistry.Register(10, pawn => {
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantStoneward, FirstIdeal);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });

        // 11: Navani — Bondsmith (Sibling)
        QuickstartCharacterSetupRegistry.Register(11, pawn => {
            Surgebinder? bondsmith =
                pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, SecondIdeal);
            if (bondsmith != null) {
                bondsmith.godsprenName = "Sibling";
                bondsmith.UpdateAbilities();
            }

            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
        });
    }
}
