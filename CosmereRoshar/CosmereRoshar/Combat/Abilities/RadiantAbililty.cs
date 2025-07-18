using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereRoshar;

public class RadiantAbility : Ability {
    private static readonly float MoteCastFadeTime = 0.4f;
    private static readonly float MoteCastScale = 1f;
    private static readonly Vector3 MoteCastOffset = new Vector3(0f, 0f, 0.48f);
    private Mote moteCast;

    public RadiantAbility(Pawn pawn) : base(pawn) { }

    public RadiantAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }


    public override IEnumerable<Command> GetGizmos() {
        if (gizmo == null) {
            gizmo = new Command_RadiantAbility(this, pawn);
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

public class Command_RadiantAbility : Command {
    private readonly Ability ability;
    private readonly Pawn pawn;

    public Command_RadiantAbility(Ability ability, Pawn pawn) {
        this.ability = ability;
        this.pawn = pawn;
        defaultLabel = ability.def.label;
        defaultDesc = ability.def.description;
        icon = ability.def.uiIcon;
    }

    private bool AbriasionCheck() {
        if (ability.def == CosmereRosharDefs.whtwl_SurgeOfAbrasion) {
            CompStormlight? comp = pawn.GetComp<CompStormlight>();
            if (comp != null && comp.AbrasionActive) {
                return true;
            }
        }

        return false;
    }

    private bool BreathStormlightCheck() {
        if (ability.def == CosmereRosharDefs.whtwl_BreathStormlight) {
            CompStormlight? comp = pawn.GetComp<CompStormlight>();
            if (comp != null && comp.m_BreathStormlight) {
                return true;
            }
        }

        return false;
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
        bool hasRadiantTrait = pawn.story.traits.HasTrait(CosmereRosharDefs.whtwl_Radiant_Windrunner) ||
                               pawn.story.traits.HasTrait(CosmereRosharDefs.whtwl_Radiant_Truthwatcher) ||
                               pawn.story.traits.HasTrait(CosmereRosharDefs.whtwl_Radiant_Edgedancer) ||
                               pawn.story.traits.HasTrait(CosmereRosharDefs.whtwl_Radiant_Skybreaker);

        if (pawn.Drafted &&
            (hasRadiantTrait ||
             pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(CosmereRosharDefs.whtwl_SummonShardblade.defName) !=
             null)) {
            if (abilityToggleStormlight()) {
                return;
            }

            if (abilitySummonShardblade()) {
                return;
            }

            if (abilityLashingUpward()) {
                return;
            }

            if (abilityWindRunnerFlight()) {
                return;
            }

            if (abilityHealSurge()) {
                return;
            }

            if (abilityPlantGrowSurge()) {
                return;
            }

            if (abilityAbrasionSurge()) {
                return;
            }

            if (abilityDivisionSurge()) { }
        }
    }

    private bool abilityPlantGrowSurge() {
        if (ability.def == CosmereRosharDefs.whtwl_SurgeOfGrowth) {
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

    private bool abilityAbrasionSurge() {
        if (ability.def == CosmereRosharDefs.whtwl_SurgeOfAbrasion) {
            ability.Activate(pawn, pawn);
            return true;
        }

        return false;
    }

    private bool abilityDivisionSurge() {
        if (ability.def == CosmereRosharDefs.whtwl_SurgeOfDivision) {
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

    private bool abilityHealSurge() {
        if (ability.def == CosmereRosharDefs.whtwl_SurgeOfHealing) {
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

    private bool abilityToggleStormlight() {
        if (ability.def == CosmereRosharDefs.whtwl_BreathStormlight) {
            ability.Activate(pawn, pawn);
            return true;
        }

        return false;
    }

    private bool abilitySummonShardblade() {
        if (ability.def == CosmereRosharDefs.whtwl_SummonShardblade) {
            ability.Activate(pawn, pawn);
            return true;
        }

        return false;
    }

    private bool abilityLashingUpward() {
        if (ability.def == CosmereRosharDefs.whtwl_LashingUpward) {
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

    private bool abilityWindRunnerFlight() {
        if (ability.def == CosmereRosharDefs.whtwl_WindRunnerFlight) {
            float cost = pawn
                .GetAbilityComp<CompAbilityEffect_AbilityWindRunnerFlight>(
                    CosmereRosharDefs.whtwl_WindRunnerFlight.defName
                )
                .Props.stormLightCost;
            float distance = pawn.GetComp<CompStormlight>().Stormlight / cost;
            TargetingParameters tp = new TargetingParameters {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetItems = true,
                canTargetBuildings = true,
                canTargetLocations = true,
                mustBeSelectable = false,
                validator = info => {
                    return info.IsValid &&
                           pawn != null &&
                           pawn.Spawned &&
                           pawn.Position.InHorDistOf(info.Cell, distance);
                },
            };
            StartCustomTargeting(pawn, distance, tp, false);
            return true;
        }

        return false;
    }


    public void StartCustomTargeting(Pawn caster, float maxDistance, TargetingParameters tp, bool triggerAsJob = true) {
        // The main action to do once a valid target is chosen
        Action<LocalTargetInfo> mainAction = chosenTarget => {
            Job job = new Job_CastAbilityOnTarget(CosmereRosharDefs.whtwl_CastAbilityOnTarget, chosenTarget, ability);
            if (triggerAsJob) {
                pawn.jobs.TryTakeOrderedJob(job);
            } else {
                ability.Activate(chosenTarget, chosenTarget);
            }
        };

        // This runs each frame to highlight the hovered cell/thing
        Action<LocalTargetInfo> highlightAction = info => {
            // If it's valid, highlight it in green
            if (info.IsValid) {
                GenDraw.DrawTargetHighlight(info);
            }
        };

        // This function checks if the user is allowed to pick 'info'
        // Return 'true' if valid, 'false' if not
        Func<LocalTargetInfo, bool> targetValidator = info => {
            // Must be valid and within maxDistance
            return info.IsValid &&
                   caster.Spawned;
        };

        // Called each GUI frame after the default crosshair is drawn
        // e.g. you can add a label if out of range
        Action<LocalTargetInfo> onGuiAction = info => {
            if (info.IsValid && !targetValidator(info)) {
                Widgets.MouseAttachedLabel("Out of range");
            }
        };

        // Called each frame. We’ll draw a radius ring to show the cast range
        Action<LocalTargetInfo> onUpdateAction = info => {
            GlowCircleRenderer.DrawCustomCircle(caster, maxDistance, Color.cyan);
        };

        Find.Targeter.BeginTargeting(
            tp,
            mainAction,
            highlightAction,
            targetValidator,
            caster,
            null, // optional cleanup
            null, // or use a custom icon
            true,
            onGuiAction,
            onUpdateAction
        );
    }

    public void StartCustomTargeting(Pawn caster, float maxDistance, TargetingParameters tp, Color color) {
        // The main action to do once a valid target is chosen
        Action<LocalTargetInfo> mainAction = chosenTarget => {
            Vector3 mouseMapPos = UI.MouseMapPosition();
            IntVec3 centerCell = mouseMapPos.ToIntVec3();
            List<Plant> plantsInRadius = new List<Plant>();

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, maxDistance, true)) {
                if (cell.InBounds(pawn.Map)) {
                    foreach (Thing thing in cell.GetThingList(pawn.Map)) {
                        // Check if the Thing is a Plant
                        if (thing is Plant plant) {
                            ability.Activate(plant, plant);
                        }
                    }
                }
            }
        };

        // This runs each frame to highlight the hovered cell/thing
        Action<LocalTargetInfo> highlightAction = info => {
            // If it's valid, highlight it in green
            if (info.IsValid) {
                GenDraw.DrawTargetHighlight(info);
            }
        };

        // This function checks if the user is allowed to pick 'info'
        // Return 'true' if valid, 'false' if not
        Func<LocalTargetInfo, bool> targetValidator = info => {
            // Must be valid and within maxDistance
            return info.IsValid &&
                   caster.Spawned &&
                   caster.Position.InHorDistOf(info.Cell, maxDistance);
        };

        // Called each GUI frame after the default crosshair is drawn
        // e.g. you can add a label if out of range
        Action<LocalTargetInfo> onGuiAction = info => {
            if (info.IsValid && !targetValidator(info)) {
                Widgets.MouseAttachedLabel("Out of range");
            }
        };

        // Called each frame. We’ll draw a radius ring to show the cast range
        Action<LocalTargetInfo> onUpdateAction = info => {
            Vector3 mousePos = UI.MouseMapPosition();
            double distance = Math.Sqrt(
                Math.Pow(mousePos.x - caster.Position.x, 2) + Math.Pow(mousePos.z - caster.Position.z, 2)
            );
            if (distance - 0.5f > maxDistance) {
                color = Color.red;
            } else {
                color = Color.green;
            }

            GlowCircleRenderer.DrawCustomCircle(UI.MouseMapPosition(), maxDistance, color);
        };

        Find.Targeter.BeginTargeting(
            tp,
            mainAction,
            highlightAction,
            targetValidator,
            caster,
            null, // optional cleanup
            null, // or use a custom icon
            true,
            onGuiAction,
            onUpdateAction
        );
    }

    public override bool InheritInteractionsFrom(Gizmo other) {
        return false;
    }
}