using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar {
    public class Verb_CastRadiant : Verb_CastAbility {
        //public override bool IsApplicableTo(LocalTargetInfo target, bool showMessages = false) {
        //    // Check if ability can be applied
        //    if (!base.IsApplicableTo(target, showMessages)) {
        //        return false;
        //    }

        //    // Check custom radiant conditions (e.g., pawn traits or abilities)
        //    if (CasterPawn.story.traits.HasTrait(TraitDef.Named("Radiant")) == false) {
        //        if (showMessages) {
        //            Messages.Message("Only Radiants can use this ability.", target.ToTargetInfo(ability.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
        //        }
        //        return false;
        //    }

        //    return true;
        //}

        //public override void OrderForceTarget(LocalTargetInfo target) {
        //    // Ensure Radiant ability can target
        //    if (IsApplicableTo(target)) {
        //        base.OrderForceTarget(target);
        //    }
        //}

        //public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true) {
        //    // Validate custom target logic for Radiant abilities
        //    if (!base.ValidateTarget(target, showMessages)) {
        //        return false;
        //    }

        //    if (target.Thing != null && target.Thing.def.category == ThingCategory.Building) {
        //        if (showMessages) {
        //            Messages.Message("Radiant abilities cannot target buildings.", caster, MessageTypeDefOf.RejectInput, historical: false);
        //        }
        //        return false;
        //    }

        //    return true;
        //}

        //public override void OnGUI(LocalTargetInfo target) {
        //    // Handle targeting UI for Radiant abilities
        //    Texture2D texture = UIIcon;

        //    if (!IsApplicableTo(target)) {
        //        texture = TexCommand.CannotShoot;
        //    }
        //    else if (!CanHitTarget(target)) {
        //        texture = TexCommand.CannotShoot;
        //    }

        //    GenUI.DrawMouseAttachment(texture);
        //    DrawAttachmentExtraLabel(target);
        //}

        //private void DrawAttachmentExtraLabel(LocalTargetInfo target) {
        //    // Optionally draw extra labels or indicators in the UI
        //    if (target.IsValid && target.HasThing) {
        //        GenMapUI.DrawText(new Vector2(target.Cell.x, target.Cell.z), "Radiant Target", Color.green);
        //    }
        //}
    }
}
