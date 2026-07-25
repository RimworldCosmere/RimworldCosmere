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

    public void AddMetalmind(ImplantedMetalmindData data) {
        metalminds.Add(data);
        Severity = metalminds.Count;
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
            sb.AppendLine($"  - {label}: {data.StoredAmount:F1} / {data.MaxAmount:F0}");
        }

        return sb.ToString().TrimEnd();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref metalminds, "metalminds", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && metalminds != null) {
            for (int i = 0; i < metalminds.Count; i++) {
                metalminds[i].ReconcileCapacity();
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