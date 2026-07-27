using System;
using Cosmere.Core.Entity;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Comp.Game;

public class Connection : IExposable {
    public bool CanBondObjectOne = true;
    public bool CanBondObjectTwo = true;
    public ILoadReferenceable ObjectOne = null!;
    public ILoadReferenceable ObjectTwo = null!;

    private float valueInt;

    public Connection() { }

    public Connection(ILoadReferenceable objectOne, ILoadReferenceable objectTwo) {
        ObjectOne = objectOne;
        ObjectTwo = objectTwo;
    }

    public Connection(
        ILoadReferenceable objectOne,
        ILoadReferenceable objectTwo,
        bool canBondObjectOne = true,
        bool canBondObjectTwo = true
    ) {
        ObjectOne = objectOne;
        ObjectTwo = objectTwo;
        CanBondObjectOne = canBondObjectOne;
        CanBondObjectTwo = canBondObjectTwo;
    }

    public float Value {
        get => valueInt;
        set => valueInt = Math.Clamp(value, 0f, 1f);
    }

    public Verse.Thing? ThingOne => ObjectOne as Verse.Thing;

    public Verse.Thing? ThingTwo => ObjectTwo as Verse.Thing;

    public void ExposeData() {
        Scribe_Values.Look(ref valueInt, "value");
        Scribe_References.Look(ref ObjectOne, "objectOne");
        Scribe_References.Look(ref ObjectTwo, "objectTwo");
        Scribe_Values.Look(ref CanBondObjectOne, "CanBondObjectOne");
        Scribe_Values.Look(ref CanBondObjectTwo, "CanBondObjectTwo");
    }

    public bool Matches(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        string oneId = ObjectOne.GetUniqueLoadID();
        string twoId = ObjectTwo.GetUniqueLoadID();
        string targetOneId = targetOne.GetUniqueLoadID();
        string targetTwoId = targetTwo.GetUniqueLoadID();
        return (oneId == targetOneId && twoId == targetTwoId) ||
               (oneId == targetTwoId && twoId == targetOneId);
    }

    public bool OneObjectMatches(ILoadReferenceable target) {
        return ObjectOne.GetUniqueLoadID() == target.GetUniqueLoadID() ||
               ObjectTwo.GetUniqueLoadID() == target.GetUniqueLoadID();
    }

    public bool ShouldSave() {
        if (ThingOne is { Destroyed: true }) return false;
        if (ThingTwo is { Destroyed: true }) return false;

        return Value > 0;
    }

    public void NotifyChange(float newValue, float oldValue) {
        if (ThingOne != null) {
            ThingOne.Notify_SignalReceived(
                new Signal(SpiritWeb.ChangedSignal, ToSignalPayload(ObjectTwo), newValue, oldValue)
            );
        }

        if (ThingTwo != null) {
            ThingTwo.Notify_SignalReceived(
                new Signal(SpiritWeb.ChangedSignal, ToSignalPayload(ObjectOne), newValue, oldValue)
            );
        }
    }

    private static NamedArgument ToSignalPayload(ILoadReferenceable obj) {
        return obj switch {
            Verse.Thing t => (NamedArgument)t,
            Faction f => (NamedArgument)f,
            _ => (NamedArgument)obj.GetUniqueLoadID(),
        };
    }
}
