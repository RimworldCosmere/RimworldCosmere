using System;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Defs {
    public class RightClickAbilityDef : Def {
        public Type abilityClass;
        public bool canTargetAnimals = false;

        public bool canTargetHumans = true;
        public bool canTargetSelf = false;
        public int cooldownTicks = 0;
        public string iconPath;
        public MetallicArtsMetalDef metal;

        public float range = 1f;
        public bool requiresLineOfSight = true;

        public virtual Texture2D Icon => ContentFinder<Texture2D>.Get(iconPath);
    }
}