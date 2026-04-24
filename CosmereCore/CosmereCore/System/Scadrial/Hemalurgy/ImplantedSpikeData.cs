using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

public class ImplantedSpikeData : IExposable {
    public float chargeStrength = 1f;
    public bool isThinNeedle;
    public string metalDefName = "";
    public HemalurgicStealType stealType;
    public string stolenDefName = "";
    public List<string> stolenDefNames = [];
    public float storedInvestiture;

    public void ExposeData() {
        Scribe_Values.Look(ref metalDefName, "metalDefName", "");
        Scribe_Values.Look(ref stealType, "stealType");
        Scribe_Values.Look(ref stolenDefName, "stolenDefName", "");
        Scribe_Collections.Look(ref stolenDefNames, "stolenDefNames", LookMode.Value);
        Scribe_Values.Look(ref chargeStrength, "chargeStrength", 1f);
        Scribe_Values.Look(ref storedInvestiture, "storedInvestiture");
        Scribe_Values.Look(ref isThinNeedle, "isThinNeedle");
        stolenDefNames ??= [];
    }
}