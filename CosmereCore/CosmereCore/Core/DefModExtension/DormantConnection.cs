using System;
using Verse;

namespace Cosmere.Core.DefModExtension;

public interface IDormantConnectionCallback {
    public bool callback(Pawn pawn, GeneDef gene);
}

public class DormantConnection : Verse.DefModExtension {
    public Type callbackHandler = null!;

    private IDormantConnectionCallback? cachedHandler;

    public IDormantConnectionCallback? Handler =>
        cachedHandler ??= Activator.CreateInstance(callbackHandler) as IDormantConnectionCallback;

    public override IEnumerable<string> ConfigErrors() {
        if (callbackHandler == null) {
            yield return "callbackHandler is null";
        }

        if (!typeof(IDormantConnectionCallback).IsAssignableFrom(callbackHandler)) {
            yield return "callbackHandler is not assignable to IDormantConnectionCallback";
        }
    }
}