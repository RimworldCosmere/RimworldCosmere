using CosmereCore.Defs;
using JetBrains.Annotations;

namespace CosmereScadrial.Defs {
    public enum AllomancyAxis {
        Internal,
        External,
    }

    public enum AllomancyGroup {
        Physical,
        Mental,
        Enhancement,
        Temporal,
    }

    public enum AllomancyPolarity {
        Pushing,
        Pulling,
    }

    public enum FeruchemyGroup {
        Physical,
        Cognitive,
        Spiritual,
        Hybrid,
    }

    public class MetallicArtsMetalDef : MetalDef {
        public MetalAllomancyDef allomancy;
        public MetalFeruchemyDef feruchemy;
    }

    public class MetalAllomancyDef {
        public AllomancyAxis? axis;
        public string description;
        public AllomancyGroup? group;
        public AllomancyPolarity? polarity;
        [CanBeNull] public string userName;
    }

    public class MetalFeruchemyDef {
        public string description;
        public FeruchemyGroup? group;
        [CanBeNull] public string userName;
    }
}