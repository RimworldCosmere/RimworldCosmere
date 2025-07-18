using System;
using System.Collections.Generic;
using CosmereRoshar.Combat.Abilities.Implementations;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Comps.WeaponsAndArmor;
using CosmereRoshar.Jobs;
using CosmereRoshar.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Combat.Abilities;

public class RadiantAbility : Ability {
    private static readonly float MoteCastFadeTime = 0.4f;
    private static readonly float MoteCastScale = 1f;
    private static readonly Vector3 MoteCastOffset = new Vector3(0f, 0f, 0.48f);
    private Mote moteCast;

    public RadiantAbility(Pawn pawn) : base(pawn) { }

    public RadiantAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }


    public override IEnumerable<Command> GetGizmos() {
        if (gizmo == null) {
            gizmo = new CommandRadiantAbility(this, pawn);
        }

        yield return gizmo;
    }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        if (!CanApplyRadiantEffect(target)) {
            MoteMaker.ThrowText(target.CenterVector3, pawn.Map, "Target not valid for Radiant ability.");
            return false;
        }

        // Optionally add special effects
        if (def.HasAreaOfEffect) {
            FleckMaker.Static(target.Cell, pawn.Map, FleckDefOf.PsycastAreaEffect, def.EffectRadius);
        }

        return base.Activate(target, dest);
    }

    protected override void ApplyEffects(
        IEnumerable<CompAbilityEffect> effects,
        LocalTargetInfo target,
        LocalTargetInfo dest
    ) {
        foreach (CompAbilityEffect effect in effects) {
            effect.Apply(target, dest);
        }
    }

    private bool CanApplyRadiantEffect(LocalTargetInfo target) {
        // Example logic to ensure the ability can be applied

        if (target.Pawn != null && !target.Pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness)) {
            return false;
        }

        return true;
    }

    public override void AbilityTick() {
        base.AbilityTick();

        if (pawn.Spawned && Casting) {
            if (moteCast == null || moteCast.Destroyed) {
                moteCast = MoteMaker.MakeAttachedOverlay(
                    pawn,
                    RimWorld.ThingDefOf.Mote_CastPsycast,
                    MoteCastOffset,
                    MoteCastScale,
                    verb.verbProps.warmupTime - MoteCastFadeTime
                );
            } else {
                moteCast.Maintain();
            }
        }
    }
}

public class CommandRadiantAbility : Command {
    private readonly Ability ability;
    private readonly Pawn pawn;

    public CommandRadiantAbility(Ability ability, Pawn pawn) {
        this.ability = ability;
        this.pawn = pawn;
        defaultLabel = ability.def.label;
        defaultDesc = ability.def.description;
        icon = ability.def.uiIcon;
    }

    private bool AbriasionCheck() {
        if (ability.def == CosmereRosharDefs.WhtwlSurgeOfAbrasion) {
            Stormlight? comp = pawn.GetComp<Stormlight>();
            if (comp is { abrasionActive: true }) {
                return true;
            }
        }

        return false;
    }

    private bool BreathStormlightCheck() {
        if (ability.def != CosmereRosharDefs.WhtwlBreathStormlight) return false;
        
        return pawn.TryGetComp<Stormlight>() is { breathStormlight: true };
    }

    protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms) {
        Text.Font = GameFont.Tiny;

        bool mouseOver = Mouse.IsOver(butRect);
        bool interacting = false;

        Color bgColor = Color.white;

        if (AbriasionCheck() || BreathStormlightCheck()) {
            bgColor = new Color(0.2f, 0.9f, 0.4f); // Green when active
        }

        if (mouseOver && !disabled) {
            bgColor = GenUI.MouseoverColor;
        }

        if (parms.highLight) {
            Widgets.DrawStrongHighlight(butRect.ExpandedBy(4f));
        }

        Material mat = disabled || parms.lowLight ? TexUI.GrayscaleGUI : null;

        GUI.color = parms.lowLight ? LowLightBgColor : bgColor;
        GenUI.DrawTextureWithMaterial(butRect, parms.shrunk ? BGTextureShrunk : BGTexture, mat);

        GUI.color = Color.white;
        DrawIcon(butRect, mat, parms);

        // Hotkey / label logic
        Vector2 keyLabelOffset = parms.shrunk ? new Vector2(3f, 0f) : new Vector2(5f, 3f);
        Rect keyRect = new Rect(
            butRect.x + keyLabelOffset.x,
            butRect.y + keyLabelOffset.y,
            butRect.width - 10f,
            Text.LineHeight
        );

        if (hotKey?.KeyDownEvent ?? false) {
            interacting = true;
            Event.current.Use();
        }

        if (Widgets.ButtonInvisible(butRect)) {
            interacting = true;
        }

        // Label
        if (!parms.shrunk && !LabelCap.NullOrEmpty()) {
            float labelHeight = Text.CalcHeight(LabelCap, butRect.width);
            Rect labelRect = new Rect(butRect.x, butRect.yMax - labelHeight + 12f, butRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        // Tooltip
        if (Mouse.IsOver(butRect)) {
            TipSignal tip = Desc;
            if (disabled && !disabledReason.NullOrEmpty()) {
                tip.text += $"\n\n{"DisabledCommand".Translate()}: {disabledReason}".Colorize(ColorLibrary.RedReadable);
            }

            tip.text += DescPostfix;
            TooltipHandler.TipRegion(butRect, tip);
        }

        // Return result
        if (interacting) {
            if (disabled) {
                if (!disabledReason.NullOrEmpty()) {
                    Messages.Message(disabledReason, MessageTypeDefOf.RejectInput, false);
                }

                return new GizmoResult(GizmoState.Mouseover, null);
            }

            return new GizmoResult(
                Event.current.button == 1 ? GizmoState.OpenedFloatMenu : GizmoState.Interacted,
                Event.current
            );
        }

        return mouseOver ? new GizmoResult(GizmoState.Mouseover) : new GizmoResult(GizmoState.Clear);
    }


    public override void ProcessInput(Event ev) {
        base.ProcessInput(ev);
        bool hasRadiantTrait = pawn.story.traits.HasTrait(CosmereRosharDefs.WhtwlRadiantWindrunner) ||
                               pawn.story.traits.HasTrait(CosmereRosharDefs.WhtwlRadiantTruthwatcher) ||
                               pawn.story.traits.HasTrait(CosmereRosharDefs.WhtwlRadiantEdgedancer) ||
                               pawn.story.traits.HasTrait(CosmereRosharDefs.WhtwlRadiantSkybreaker);

        if (pawn.Drafted &&
            (hasRadiantTrait ||
             pawn.GetAbilityComp<CompAbilityEffectSpawnEquipment>(CosmereRosharDefs.WhtwlSummonShardblade.defName) !=
             null)) {
            if (AbilityToggleStormlight()) {
                return;
            }

            if (AbilitySummonShardblade()) {
                return;
            }

            if (AbilityLashingUpward()) {
                return;
            }

            if (AbilityWindRunnerFlight()) {
                return;
            }

            if (AbilityHealSurge()) {
                return;
            }

            if (AbilityPlantGrowSurge()) {
                return;
            }

            if (AbilityAbrasionSurge()) {
                return;
            }

            if (AbilityDivisionSurge()) { }
        }
    }

    private bool AbilityPlantGrowSurge() {
        if (ability.def == CosmereRosharDefs.WhtwlSurgeOfGrowth) {
            TargetingParameters tp = new TargetingParameters {
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetItems = false,
                canTargetBuildings = false,
                canTargetLocations = true,
                canTargetPlants = true,
                validator = info => {
                    return info.IsValid &&
                           pawn != null &&
                           pawn.Spawned &&
                           pawn.Position.InHorDistOf(info.Cell, ability.verb.EffectiveRange);
                },
            };

            StartCustomTargeting(pawn, ability.verb.EffectiveRange, tp, Color.green);
            return true;
        }

        return false;
    }

    private bool AbilityAbrasionSurge() {
        if (ability.def == CosmereRosharDefs.WhtwlSurgeOfAbrasion) {
            ability.Activate(pawn, pawn);
            return true;
        }

        return false;
    }

    private bool AbilityDivisionSurge() {
        if (ability.def == CosmereRosharDefs.WhtwlSurgeOfDivision) {
            TargetingParameters tp = new TargetingParameters {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetItems = false,
                canTargetBuildings = false,
                canTargetLocations = false,
                validator = info => {
                    return info.IsValid &&
                           pawn != null &&
                           pawn.Spawned;
                },
            };
            StartCustomTargeting(pawn, ability.verb.EffectiveRange, tp);
            return true;
        }

        return false;
    }

    private bool AbilityHealSurge() {
        if (ability.def == CosmereRosharDefs.WhtwlSurgeOfHealing) {
            TargetingParameters tp = new TargetingParameters {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetItems = false,
                canTargetBuildings = false,
                canTargetLocations = false,
                validator = info => {
                    return info.IsValid &&
                           pawn != null &&
                           pawn.Spawned;
                },
            };
            StartCustomTargeting(pawn, ability.verb.EffectiveRange, tp);
            return true;
        }

        return false;
    }

    private bool AbilityToggleStormlight() {
        if (ability.def == CosmereRosharDefs.WhtwlBreathStormlight) {
            ability.Activate(pawn, pawn);
            return true;
        }

        return false;
    }

    private bool AbilitySummonShardblade() {
        if (ability.def == CosmereRosharDefs.WhtwlSummonShardblade) {
            ability.Activate(pawn, pawn);
            return true;
        }

        return false;
    }

    private bool AbilityLashingUpward() {
        if (ability.def == CosmereRosharDefs.WhtwlLashingUpward) {
            TargetingParameters tp = new TargetingParameters {
                canTargetPawns = true,
                canTargetAnimals = false,
                canTargetItems = false,
                canTargetBuildings = false,
                canTargetLocations = false,
                validator = info => {
                    return info.IsValid &&
                           pawn != null &&
                           pawn.Spawned;
                },
            };
            StartCustomTargeting(pawn, ability.verb.EffectiveRange, tp);
            return true;
        }

        return false;
    }

    private bool AbilityWindRunnerFlight() {
        if (ability.def != CosmereRosharDefs.WhtwlWindRunnerFlight) return false;
        
        float cost = pawn
            .GetAbilityComp<CompAbilityEffectAbilityWindRunnerFlight>(
                CosmereRosharDefs.WhtwlWindRunnerFlight.defName
            )
            .props.stormLightCost;
        float distance = pawn.GetComp<Stormlight>().currentStormlight / cost;
        TargetingParameters tp = new TargetingParameters {
            canTargetPawns = true,
            canTargetAnimals = true,
            canTargetItems = true,
            canTargetBuildings = true,
            canTargetLocations = true,
            mustBeSelectable = false,
            validator = info => info.IsValid &&
                                pawn.Spawned &&
                                pawn.Position.InHorDistOf(info.Cell, distance),
        };
        StartCustomTargeting(pawn, distance, tp, false);
        return true;
    }

    private void StartCustomTargeting(Pawn caster, float maxDistance, TargetingParameters tp, bool triggerAsJob = true) {
        Find.Targeter.BeginTargeting(
            tp,
            MainAction,
            HighlightAction,
            TargetValidator,
            caster,
            null, // optional cleanup
            null, // or use a custom icon
            true,
            OnGuiAction,
            OnUpdateAction
        );
        return;

        // This function checks if the user is allowed to pick 'info'
        // Return 'true' if valid, 'false' if not
        bool TargetValidator(LocalTargetInfo info) {
            // Must be valid and within maxDistance
            return info.IsValid && caster.Spawned;
        }

        // This runs each frame to highlight the hovered cell/thing
        void HighlightAction(LocalTargetInfo info) {
            // If it's valid, highlight it in green
            if (info.IsValid) {
                GenDraw.DrawTargetHighlight(info);
            }
        }

        // Called each GUI frame after the default crosshair is drawn
        // e.g. you can add a label if out of range
        void OnGuiAction(LocalTargetInfo info) {
            if (info.IsValid && !TargetValidator(info)) {
                Widgets.MouseAttachedLabel("Out of range");
            }
        }

        // Called each frame. We’ll draw a radius ring to show the cast range
        void OnUpdateAction(LocalTargetInfo info) {
            GlowCircleRenderer.DrawCustomCircle(caster, maxDistance, Color.cyan);
        }

        // The main action to do once a valid target is chosen
        void MainAction(LocalTargetInfo chosenTarget) {
            Job job = new JobCastAbilityOnTarget(CosmereRosharDefs.WhtwlCastAbilityOnTarget, chosenTarget, ability);
            if (triggerAsJob) {
                pawn.jobs.TryTakeOrderedJob(job);
            } else {
                ability.Activate(chosenTarget, chosenTarget);
            }
        }
    }

    public void StartCustomTargeting(Pawn caster, float maxDistance, TargetingParameters tp, Color color) {
        Find.Targeter.BeginTargeting(
            tp,
            MainAction,
            HighlightAction,
            TargetValidator,
            caster,
            null, // optional cleanup
            null, // or use a custom icon
            true,
            OnGuiAction,
            OnUpdateAction
        );
        return;

        // This runs each frame to highlight the hovered cell/thing
        void HighlightAction(LocalTargetInfo info) {
            // If it's valid, highlight it in green
            if (info.IsValid) {
                GenDraw.DrawTargetHighlight(info);
            }
        }

        // This function checks if the user is allowed to pick 'info'
        // Return 'true' if valid, 'false' if not
        bool TargetValidator(LocalTargetInfo info) {
            // Must be valid and within maxDistance
            return info.IsValid && caster.Spawned && caster.Position.InHorDistOf(info.Cell, maxDistance);
        }

        // Called each GUI frame after the default crosshair is drawn
        // e.g. you can add a label if out of range
        void OnGuiAction(LocalTargetInfo info) {
            if (info.IsValid && !TargetValidator(info)) {
                Widgets.MouseAttachedLabel("Out of range");
            }
        }

        // Called each frame. We’ll draw a radius ring to show the cast range
        void OnUpdateAction(LocalTargetInfo info) {
            Vector3 mousePos = UI.MouseMapPosition();
            double distance = Math.Sqrt(Math.Pow(mousePos.x - caster.Position.x, 2) + Math.Pow(mousePos.z - caster.Position.z, 2));
            if (distance - 0.5f > maxDistance) {
                color = Color.red;
            } else {
                color = Color.green;
            }

            GlowCircleRenderer.DrawCustomCircle(UI.MouseMapPosition(), maxDistance, color);
        }

        // The main action to do once a valid target is chosen
        void MainAction(LocalTargetInfo chosenTarget) {
            Vector3 mouseMapPos = UI.MouseMapPosition();
            IntVec3 centerCell = mouseMapPos.ToIntVec3();
            List<Plant> plantsInRadius = [];

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, maxDistance, true)) {
                if (!cell.InBounds(pawn.Map)) continue;
                foreach (Thing thing in cell.GetThingList(pawn.Map)) {
                    // Check if the Thing is a Plant
                    if (thing is Plant plant) {
                        ability.Activate(plant, plant);
                    }
                }
            }
        }
    }

    public override bool InheritInteractionsFrom(Gizmo other) {
        return false;
    }
}