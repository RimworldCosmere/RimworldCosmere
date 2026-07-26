using RimWorld;
using Verse;
using Verse.Sound;
using SoundDefOf = Cosmere.Core.SoundDefOf;

namespace Cosmere.System.Roshar.Surgebinding.Ability;

public class Shardplate : SurgebindingAbility {
    private static ThingDef? _shardplateDef;
    private static ThingDef ShardplateDef => _shardplateDef ??= ThingDefOf.Cosmere_Roshar_Apparel_RadiantShardplate;
    private static ThingDef? _shardhelmDef;
    private static ThingDef ShardhelmDef => _shardhelmDef ??= ThingDefOf.Cosmere_Roshar_Apparel_RadiantShardhelm;

    public Shardplate(Pawn pawn) : base(pawn) { }
    public Shardplate(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    protected override void OnEnable() {
        Apparel shardplate = (Apparel)ThingMaker.MakeThing(ShardplateDef, radiantOrder.gemstone.Item);
        Apparel shardhelm = (Apparel)ThingMaker.MakeThing(ShardhelmDef, radiantOrder.gemstone.Item);

        pawn.apparel?.Wear(shardplate, false, true);
        pawn.apparel?.Wear(shardhelm, false, true);

        SoundDefOf.Cosmere_Core_Sound_LoadingQuantumRiser.PlayOneShot(
            SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map)) with { volumeFactor = .25f }
        );

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);
    }

    protected override void OnDisable() {
        base.OnDisable();

        Apparel? shardplate = pawn.apparel.WornApparel.Find(x => x.def == ShardplateDef);
        Apparel? shardhelm = pawn.apparel.WornApparel.Find(x => x.def == ShardhelmDef);
        if (shardplate != null) {
            pawn.apparel.Unlock(shardplate);
            shardplate.Destroy();
        }

        if (shardhelm != null) {
            pawn.apparel.Unlock(shardhelm);
            shardhelm.Destroy();
        }

        SoundDefOf.Cosmere_Core_Sound_LoadingQuantumRiser.PlayOneShot(
            SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map)) with { volumeFactor = .25f }
        );

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);
    }
}