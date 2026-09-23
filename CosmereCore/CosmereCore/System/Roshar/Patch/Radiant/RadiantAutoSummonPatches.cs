using Concord;
using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;
using Shardblade = Cosmere.System.Roshar.Surgebinding.Ability.Shardblade;
using Shardplate = Cosmere.System.Roshar.Surgebinding.Ability.Shardplate;

namespace Cosmere.System.Roshar.Patch.Radiant;

public static class RadiantAutoSummon {
    private const int DismissGraceTicks = 5000;
    private static readonly HashSet<int> AutoSummonedPawns = [];
    private static readonly Dictionary<int, int> LastCombatTick = new Dictionary<int, int>();

    private static AbilityDef? cachedBladeAbilityDef;
    private static AbilityDef? cachedPlateAbilityDef;
    private static bool defsResolved;

    private static void ResolveDefsIfNeeded() {
        if (defsResolved) return;
        cachedBladeAbilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_ToggleShardblade");
        cachedPlateAbilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_ToggleShardplate");
        defsResolved = cachedBladeAbilityDef != null || cachedPlateAbilityDef != null;
    }

    public static bool TryAutoSummon(Pawn pawn) {
        if (pawn.Drafted) return false;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return false;

        ResolveDefsIfNeeded();

        bool summoned = false;

        if (cachedBladeAbilityDef != null) {
            Ability? bladeAbility = pawn.abilities?.GetAbility(cachedBladeAbilityDef);
            if (bladeAbility is Shardblade blade && blade.status.active == Active.Off) {
                blade.UpdateStatus(Active.On);
                summoned = true;
                Log.Debug($"RadiantAutoSummonPatch: {pawn.LabelShort} auto-summoned Shardblade");
            }
        }

        if (cachedPlateAbilityDef != null) {
            Ability? plateAbility = pawn.abilities?.GetAbility(cachedPlateAbilityDef);
            if (plateAbility is Shardplate plate && plate.status.active == Active.Off) {
                plate.UpdateStatus(Active.On);
                summoned = true;
                Log.Debug($"RadiantAutoSummonPatch: {pawn.LabelShort} auto-summoned Shardplate");
            }
        }

        if (summoned) {
            AutoSummonedPawns.Add(pawn.thingIDNumber);
        }

        LastCombatTick[pawn.thingIDNumber] = GenTicks.TicksGame;
        return summoned;
    }

    public static void TryAutoDismiss(Pawn pawn) {
        if (pawn.Drafted) return;

        int pawnId = pawn.thingIDNumber;
        if (!AutoSummonedPawns.Contains(pawnId)) return;

        if (LastCombatTick.TryGetValue(pawnId, out int lastTick) &&
            GenTicks.TicksGame - lastTick < DismissGraceTicks) {
            return;
        }

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            AutoSummonedPawns.Remove(pawnId);
            return;
        }

        if (cachedBladeAbilityDef != null) {
            Ability? bladeAbility = pawn.abilities?.GetAbility(cachedBladeAbilityDef);
            if (bladeAbility is Shardblade blade && blade.status.active == Active.On) {
                blade.UpdateStatus(Active.Off);
                Log.Debug($"RadiantAutoSummonPatch: {pawn.LabelShort} auto-dismissed Shardblade");
            }
        }

        if (cachedPlateAbilityDef != null) {
            Ability? plateAbility = pawn.abilities?.GetAbility(cachedPlateAbilityDef);
            if (plateAbility is Shardplate plate && plate.status.active == Active.On) {
                plate.UpdateStatus(Active.Off);
                Log.Debug($"RadiantAutoSummonPatch: {pawn.LabelShort} auto-dismissed Shardplate");
            }
        }

        AutoSummonedPawns.Remove(pawnId);
        LastCombatTick.Remove(pawnId);
    }
}

[Patch]
public abstract class RadiantAutoSummonJobPatch : JobGiver_ConfigurableHostilityResponse {
    [Inject(At.Return, "TryGetAttackNearbyEnemyJob")]
    private void AfterTryGetAttackNearbyEnemyJob(Pawn pawn, ControlHandle<Verse.AI.Job> ch) {
        if (ch.ReturnValue == null) return;
        if (pawn.playerSettings?.hostilityResponse != HostilityResponseMode.Attack) return;
        RadiantAutoSummon.TryAutoSummon(pawn);
    }

    [Inject(At.Return, nameof(TryGiveJob))]
    private void AfterTryGiveJob(Pawn pawn, ControlHandle<Verse.AI.Job> ch) {
        if (ch.ReturnValue != null) return;
        RadiantAutoSummon.TryAutoDismiss(pawn);
    }
}

[Patch]
public abstract class RadiantAutoSummonDamagePatch : Pawn {
    [Inject(At.Return, nameof(PreApplyDamage))]
    private void AfterPreApplyDamage(ref DamageInfo dinfo) {
        Pawn self = this;
        if (self.Drafted) return;
        if (dinfo.Instigator is not Pawn attacker) return;
        if (!attacker.HostileTo(self)) return;
        RadiantAutoSummon.TryAutoSummon(self);
    }
}
