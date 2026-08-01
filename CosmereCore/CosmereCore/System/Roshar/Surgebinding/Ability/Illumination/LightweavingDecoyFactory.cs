using Cosmere.Core.Util;
using RimWorld;
using Verse;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public static class LightweavingDecoyFactory {
    public static Pawn Spawn(Pawn caster, IntVec3 cell, Map map) {
        Pawn decoy = IllusoryPawnUtility.Create(caster, caster.kindDef, Faction.OfPlayer);

        decoy.Name = new NameSingle(caster.Name?.ToStringShort + " (Decoy)");

        GenSpawn.Spawn(decoy, cell, map);

        DecoyHediff hediff = (DecoyHediff)HediffMaker.MakeHediff(DecoyHediff.Def, decoy);
        hediff.caster = caster;
        hediff.Severity = 1f;
        decoy.health.AddHediff(hediff);

        return decoy;
    }
}
