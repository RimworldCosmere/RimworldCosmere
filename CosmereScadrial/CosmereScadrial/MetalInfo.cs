#nullable disable
using UnityEngine;

namespace CosmereScadrial;

public class MetalRegistryData {
    public MetalInfo[] metals;
}

public class MetalInfo {
    public MetalAllomancyInfo allomancy;
    public Color color;
    public string defName;
    public MetalFeruchemyInfo feruchemy;
    public bool godMetal;
    public float maxAmount = 100f;
    public string name;
}

public class MetalAllomancyInfo {
    public string axis;
    public string description;
    public string group;
    public string polarity;
    public string userName;
}

public class MetalFeruchemyInfo {
    public string description;
    public string group;
    public string userName;
}