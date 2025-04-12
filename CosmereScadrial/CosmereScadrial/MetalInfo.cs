using UnityEngine;

namespace CosmereScadrial {
    public class MetalRegistryData {
        public MetalInfo[] Metals;
    }

    public class MetalInfo {
        public MetalAllomancyInfo Allomancy;
        public Color Color;
        public string DefName;
        public MetalFeruchemyInfo Feruchemy;
        public bool GodMetal;
        public float MaxAmount = 100f;
        public string Name;
    }

    public class MetalAllomancyInfo {
        public string Axis;
        public string Description;
        public string Group;
        public string Polarity;
        public string UserName;
    }

    public class MetalFeruchemyInfo {
        public string Description;
        public string Group;
        public string UserName;
    }
}