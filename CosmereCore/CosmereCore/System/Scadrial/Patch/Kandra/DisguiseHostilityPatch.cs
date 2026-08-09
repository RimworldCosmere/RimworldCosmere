using Concord;
using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Kandra;

/// <summary>
///     Stops a raid attacking one of its own, when one of its own is a kandra.
/// </summary>
/// <remarks>
///     <c>HostileTo</c> is the single question everything asks before it decides to shoot: target
///     finding, turrets, mental states, threat counts. Answering it once here means the disguise
///     works everywhere rather than in whichever places happened to get patched.
///     <para>
///         Only true is flipped to false. A disguise can talk somebody out of a fight; it cannot
///         start one.
///     </para>
/// </remarks>
[Patch(typeof(GenHostility))]
public static class DisguiseHostilityPatch {
    [Inject(At.Return, nameof(GenHostility.HostileTo), 0u, new[] { typeof(Verse.Thing), typeof(Verse.Thing) })]
    private static void AfterHostileTo(Verse.Thing a, Verse.Thing b, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (a == null || b == null) return;
        if (!KandraDisguise.Fools(a, b)) return;

        ch.ReturnValue = false;
    }
}
