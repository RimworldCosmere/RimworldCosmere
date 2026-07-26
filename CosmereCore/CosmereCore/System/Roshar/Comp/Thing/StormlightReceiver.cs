using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.GameCondition;
using Cosmere.System.Roshar.Comp.Map;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightReceiverProperties : CompProperties {
    public float baseAbsorptionRate = 5.0f;

    public StormlightReceiverProperties() {
        compClass = typeof(StormlightReceiver);
    }
}

public class StormlightReceiver : StormlightNode {
    private const int TickInterval = 30;

    private StormlightReceiverProperties Props => (StormlightReceiverProperties)props;

    public override void CompTick() {
        base.CompTick();
        if (!GenTicks.IsTickInterval(TickInterval)) return;

        Verse.Map? map = parent.Map;
        if (map == null) return;

        AbsorbFromHighstorm(map);
        AbsorbFromGameConditions(map);
    }

    private void AbsorbFromHighstorm(Verse.Map map) {
        Highstorm? highstorm = map.gameConditionManager.GetActiveCondition<Highstorm>();
        if (highstorm == null) return;

        float intensity = highstorm.CurrentIntensity;
        if (intensity <= 0f) return;

        float exposure = CalculateExposure(map);
        if (exposure <= 0f) return;

        InvestitureHolder? holder = Investiture;
        if (holder == null || holder.isFull) return;

        float amount = Props.baseAbsorptionRate * intensity * exposure * TickInterval;
        holder.currentInvestitureSelf += amount;

        if (exposure >= 1.0f && intensity > 0.4f) {
            ApplyStormDamage(intensity);
        }
    }

    private void AbsorbFromGameConditions(Verse.Map map) {
        InvestitureHolder? holder = Investiture;
        if (holder == null || holder.isFull) return;

        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            string defName = conditions[i].def.defName;
            if (defName == "Cosmere_Roshar_GameCondition_SiblingBlessing") {
                holder.currentInvestitureSelf += 0.5f;
            }
            else if (defName == "Cosmere_Roshar_GameCondition_HonorPerpendicularity") {
                holder.currentInvestitureSelf += 2.0f;
            }
        }
    }

    private float CalculateExposure(Verse.Map map) {
        if (parent.Position.Roofed(map)) return 0f;
        if (!StormShelterManager.IsInsideShelter(parent.Position)) return 1.0f;
        return 0.5f;
    }

    private void ApplyStormDamage(float intensity) {
        float damage = 5f * intensity;
        DamageInfo dinfo = new DamageInfo(
            DamageDefOf.TornadoScratch,
            damage,
            instigatorGuilty: false
        );
        parent.TakeDamage(dinfo);
    }
}