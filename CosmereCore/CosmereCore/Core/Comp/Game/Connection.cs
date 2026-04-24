using System;
using Cosmere.Core.Entity;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Comp.Game;

public class Connection : IExposable {
    public bool canBondObjectOne = true;
    public bool canBondObjectTwo = true;
    public ILoadReferenceable objectOne = null!;
    public ILoadReferenceable objectTwo = null!;

    private float valueInt;

    public Connection() { }

    public Connection(ILoadReferenceable objectOne, ILoadReferenceable objectTwo) {
        this.objectOne = objectOne;
        this.objectTwo = objectTwo;
    }

    public Connection(
        ILoadReferenceable objectOne,
        ILoadReferenceable objectTwo,
        bool canBondObjectOne = true,
        bool canBondObjectTwo = true
    ) {
        this.objectOne = objectOne;
        this.objectTwo = objectTwo;
        this.canBondObjectOne = canBondObjectOne;
        this.canBondObjectTwo = canBondObjectTwo;
    }

    public float value {
        get => valueInt;
        set => valueInt = Math.Clamp(value, 0f, 1f);
    }

    public bool objectOneIsThing => objectOne is Verse.Thing;
    public bool objectTwoIsThing => objectTwo is Verse.Thing;
    public bool objectOneIsPlanetLayer => objectOne is PlanetLayer;
    public bool objectTwoIsPlanetLayer => objectTwo is PlanetLayer;
    public bool objectOneIsFaction => objectOne is Faction;
    public bool objectTwoIsFaction => objectTwo is Faction;
    public bool objectOneIsShard => objectOne is Shard;
    public bool objectTwoIsShard => objectTwo is Shard;
    public Verse.Thing? thingOne => objectOne as Verse.Thing;
    public Verse.Thing? thingTwo => objectTwo as Verse.Thing;
    public PlanetLayer? planetLayerOne => objectOne as PlanetLayer;
    public PlanetLayer? planetLayerTwo => objectTwo as PlanetLayer;
    public Faction? factionOne => objectOne as Faction;
    public Faction? factionTwo => objectTwo as Faction;
    public Shard? shardOne => objectOne as Shard;
    public Shard? shardTwo => objectTwo as Shard;

    public void ExposeData() {
        Scribe_Values.Look(ref valueInt, "value");
        Scribe_References.Look(ref objectOne, "objectOne");
        Scribe_References.Look(ref objectTwo, "objectTwo");
        Scribe_Values.Look(ref canBondObjectOne, "canBondObjectOne");
        Scribe_Values.Look(ref canBondObjectTwo, "canBondObjectTwo");
    }

    public virtual bool Equals(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        return objectOne.GetUniqueLoadID() == targetOne.GetUniqueLoadID() &&
               objectTwo.GetUniqueLoadID() == targetTwo.GetUniqueLoadID();
    }

    public bool OneObjectMatches(ILoadReferenceable target) {
        return objectOne.GetUniqueLoadID() == target.GetUniqueLoadID() ||
               objectTwo.GetUniqueLoadID() == target.GetUniqueLoadID();
    }

    public bool ShouldSave() {
        if (thingOne is { Destroyed: true }) return false;
        if (thingTwo is { Destroyed: true }) return false;

        return value > 0;
    }
}