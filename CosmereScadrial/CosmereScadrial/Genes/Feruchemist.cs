using System.Linq;
using CosmereCore.Utils;
using CosmereResources.ModExtensions;
using CosmereScadrial.Defs;
using CosmereScadrial.Extensions;
using CosmereScadrial.Utils;
using Verse;

namespace CosmereScadrial.Genes;

public class Feruchemist : Gene {
    public MetallicArtsMetalDef metal => def.GetModExtension<MetalsLinked>().Metals.First()!.ToMetallicArts();

    public override void PostAdd() {
        base.PostAdd();

        MetalbornUtility.HandleMistbornAndFullFeruchemistTraits(pawn);
        MetalbornUtility.HandleMetalbornTrait(pawn);
        InvestitureUtility.AssignHeighteningFromBEUs(pawn);
    }

    public override void PostRemove() {
        base.PostRemove();

        MetalbornUtility.HandleMistbornAndFullFeruchemistTraits(pawn);
        MetalbornUtility.HandleMetalbornTrait(pawn);
    }
}