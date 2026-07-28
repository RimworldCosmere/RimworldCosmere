using Concord;
using RimWorld;
using Verse;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;
using LightweavingDecoyRegistry = Cosmere.System.Roshar.Surgebinding.Ability.Illumination.LightweavingDecoyRegistry;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[Patch]
public abstract class LightweavingDecoyVanishOnDamagePatch : Pawn {
    // Both parameters must be declared byref to match the target. Taking absorbed by value compiles
    // the assignments to starg against a bool& slot, which the runtime rejects as invalid IL.
    [Inject(At.Head, nameof(PreApplyDamage))]
    private Control BeforePreApplyDamage(ref DamageInfo dinfo, ref bool absorbed) {
        absorbed = false;
        Pawn self = this;
        if (!DecoyHediff.IsDecoy(self)) return Control.Continue;

        absorbed = true;
        if (self.Spawned) {
            FleckMaker.Static(self.Position, self.Map, FleckDefOf.PsycastAreaEffect);
            self.DeSpawn();
        }

        LightweavingDecoyRegistry.Remove(self);
        if (!self.Destroyed) {
            self.Discard(true);
        }

        return Control.Cancel;
    }
}
