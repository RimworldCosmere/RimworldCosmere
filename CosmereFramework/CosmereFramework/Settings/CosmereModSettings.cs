using CosmereFramework.Listing;
using UnityEngine;
using Verse;

namespace CosmereFramework.Settings;

public abstract class CosmereModSettings : IExposable {
    public abstract string Name { get; }

    public abstract void ExposeData();

    public abstract void DoTabContents(Rect inRect, ListingForm listing);
}