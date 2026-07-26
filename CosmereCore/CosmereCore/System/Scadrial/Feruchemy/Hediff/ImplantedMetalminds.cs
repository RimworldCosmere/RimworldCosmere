using System.Text;
using Cosmere.Core.Def;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class ImplantedMetalminds : HediffWithComps {
    public List<ImplantedMetalmindData> metalminds = [];

    public int metalmindCount => metalminds.Count;

    public override string LabelBase => metalminds.Count <= 1
        ? "implanted metalmind"
        : $"implanted metalminds x{metalminds.Count}";

    public override bool ShouldRemove => metalminds.Count == 0;

    /// Finds or creates the pawn's implant hediff and files the metalmind under it.
    public static void Attach(Pawn pawn, ImplantedMetalmindData data, BodyPartRecord part) {
        ImplantedMetalminds? hediff =
            pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_ImplantedMetalminds) as
                ImplantedMetalminds;
        if (hediff == null) {
            hediff = (ImplantedMetalminds)HediffMaker.MakeHediff(
                HediffDefOf.Cosmere_Scadrial_Hediff_ImplantedMetalminds,
                pawn,
                part
            );
            pawn.health.AddHediff(hediff, part);
        }

        hediff.AddMetalmind(data);
    }

    public void AddMetalmind(ImplantedMetalmindData data) {
        if (data.loadId < 0) data.loadId = NextLoadId();

        metalminds.Add(data);
        Severity = metalminds.Count;
    }

    /// Ids only have to be unique among this pawn's implants, and they must not be
    /// reused after one burns out or the player's chosen target would silently move
    /// to a different metalmind.
    private int NextLoadId() {
        int next = 0;
        for (int i = 0; i < metalminds.Count; i++) {
            if (metalminds[i].loadId >= next) next = metalminds[i].loadId + 1;
        }

        return next;
    }

    public ImplantedMetalmindData? RemoveMetalmindAt(int index) {
        if (index < 0 || index >= metalminds.Count) return null;
        ImplantedMetalmindData data = metalminds[index];
        metalminds.RemoveAt(index);
        Severity = metalminds.Count;
        return data;
    }

    public override string GetTooltip(Pawn pawn, bool showHediffsDebugInfo) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(def.LabelCap);
        sb.AppendLine(def.Description);
        sb.AppendLine();
        sb.AppendLine("CS_Feruchemy_ImplantedCount".Translate(metalminds.Count));
        for (int i = 0; i < metalminds.Count; i++) {
            ImplantedMetalmindData data = metalminds[i];
            string label = GetMetalmindLabel(data);
            string line = data.CompoundedAmount > 0f
                ? "CS_Feruchemy_ImplantLineCompounded".Translate(
                    label.Named("LABEL"),
                    data.TotalStored.ToString("F1").Named("AMOUNT"),
                    data.MaxAmount.ToString("F0").Named("MAX"),
                    data.CompoundedAmount.ToString("F1").Named("COMPOUNDED")
                )
                : "CS_Feruchemy_ImplantLine".Translate(
                    label.Named("LABEL"),
                    data.TotalStored.ToString("F1").Named("AMOUNT"),
                    data.MaxAmount.ToString("F0").Named("MAX")
                );
            sb.AppendLine("  - " + line);
        }

        return sb.ToString().TrimEnd();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref metalminds, "metalminds", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && metalminds != null) {
            for (int i = 0; i < metalminds.Count; i++) {
                metalminds[i].ReconcileCapacity();
                // Saves written before implants carried an id load them all as -1,
                // which would make every one of them answer to the same target.
                if (metalminds[i].loadId < 0) metalminds[i].loadId = i;
            }
        }
        metalminds ??= [];
    }

    private static string GetMetalmindLabel(ImplantedMetalmindData data) {
        string metalLabel = DefDatabase<MetalDef>.GetNamedSilentFail(data.metalDefName)?.label ?? data.metalDefName;
        string metalmindLabel =
            DefDatabase<ThingDef>.GetNamedSilentFail(data.metalmindType)?.label ?? data.metalmindType;
        metalmindLabel = metalmindLabel.Replace("metalmind ", "");
        return $"{metalLabel} {metalmindLabel}".ToLower();
    }
}