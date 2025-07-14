using System;
using System.Collections.Generic;
using System.Linq;
using CosmereCore.Extension;
using CosmereFramework;
using CosmereFramework.Extension;
using CosmereResources;
using CosmereResources.Def;
using CosmereResources.DefModExtension;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Feruchemy.Comp.Thing;
using CosmereScadrial.Gene;
using CosmereScadrial.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.FloatMenuOptionProvider;

public class MetalmindMenuProvider : RimWorld.FloatMenuOptionProvider {
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    private static FloatMenuOption CannotEquipReason(string reason) {
        return new FloatMenuOption("CS_CannotEquip".Translate() + ": " + reason, null);
    }
    
    private AcceptanceReport CanEquip(Pawn pawn, Verse.Thing metalmind) {
        var metal = DefDatabase<MetalDef>.GetNamedSilentFail(metalmind.Stuff.defName)?.ToMetallicArts();
        if (metal == null) Logger.Warning("Metalmind doesn't have a metal. This shouldn't happen.");
        
        if (metal?.feruchemy == null) {
            return "CS_NoFeruchemicProperties".Translate();
        }
        
        // Check if the pawn is a Ferring for the metal
        if (!pawn.IsFerring(metal)) {
            return "CS_NotFerring".Translate(metal.feruchemy.userName.Named("FERRING"));
        }

        return true;
    }

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Verse.Thing metalmind, FloatMenuContext context) {
        var comp = metalmind.TryGetComp<Metalmind>();
        if (comp == null) yield break;
        string carryString = "CarryThing".Translate((NamedArgument)metalmind.LabelShort, (NamedArgument)metalmind);
        string equipString = "EquipThing".Translate((NamedArgument)metalmind.LabelShort, (NamedArgument)metalmind);
        string unequipString = "UnequipThing".Translate((NamedArgument)metalmind.LabelShort, (NamedArgument)metalmind);
        
        if (metalmind.holdingOwner?.Owner == context.FirstSelectedPawn) {
            if (comp.equipped) {
                yield return FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(
                        equipString,
                        (Action)(() => {
                            metalmind.SetForbidden(false);
                            Job job = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_EquipMetalmind, (LocalTargetInfo)metalmind);
                            context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
                        })
                    ),
                    context.FirstSelectedPawn,
                    (LocalTargetInfo)metalmind
                );
                yield break;
            }
            
            yield return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption(
                    unequipString,
                    (Action)(() => {
                        metalmind.SetForbidden(false);
                        Job job = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_UnequipMetalmind, (LocalTargetInfo)metalmind);
                        context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
                    })
                ),
                context.FirstSelectedPawn,
                (LocalTargetInfo)metalmind
            );
            yield break;
        }
        
        Pawn? pawn = context.FirstSelectedPawn;
        if (pawn is null) yield break;

        AcceptanceReport report = CanEquip(pawn, metalmind);
        if (!report) {
            yield return CannotEquipReason(report.Reason);
            yield break;
        }

        
        if (!context.FirstSelectedPawn.CanReserveAndReach((LocalTargetInfo)metalmind, PathEndMode.OnCell, Danger.Deadly)) {
            yield return new FloatMenuOption((string)(carryString + ": " + "NoPath".Translate().CapitalizeFirst()), null);
            yield return new FloatMenuOption((string)(equipString + ": " + "NoPath".Translate().CapitalizeFirst()), null);
            yield break;;
        }
        
        // Move to Inventory
        yield return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(
                carryString,
                (Action)(() => {
                    metalmind.SetForbidden(false);
                    Job job = JobMaker.MakeJob(RimWorld.JobDefOf.TakeCountToInventory, (LocalTargetInfo)metalmind);
                    job.count = 1;
                    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
                })
            ),
            context.FirstSelectedPawn,
            (LocalTargetInfo)metalmind
        );
        
        yield return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(
                equipString,
                (Action)(() => {
                    metalmind.SetForbidden(false);
                    Job job = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_EquipMetalmind, (LocalTargetInfo)metalmind);
                    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
                })
            ),
            context.FirstSelectedPawn,
            (LocalTargetInfo)metalmind
        );
    }
}