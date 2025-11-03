using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

[StaticConstructorOnStartup]
public class ChooseRadiantOrder : ThingComp {
    private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Icons/KnightsRadiant", false);
    private Pawn pawn => (Pawn)parent;

    public override IEnumerable<Verse.Gizmo> CompGetGizmosExtra() {
        yield return new Command_Action {
            defaultLabel = "CRO_Choose_Radiant_Order_Button".Translate(),
            defaultDesc = "CRO_Choose_Radiant_Order_Desc".Translate(pawn.NameFullColored.Named("PAWN")).Resolve(),
            icon = Icon,
            action = () => { Find.WindowStack.Add(new Dialog.ChooseRadiantOrder(pawn)); },
        };
    }
}