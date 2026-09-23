using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.World;

[DefOf]
public static class AshmountDefOf {
    public static WorldObjectDef Cosmere_Scadrial_WorldObject_Ashmount = null!;

    static AshmountDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(AshmountDefOf));
    }
}
