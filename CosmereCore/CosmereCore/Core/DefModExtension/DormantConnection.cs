using System;
using Verse;

namespace Cosmere.Core.DefModExtension;

public interface IDormantConnectionCallback {
    public bool Callback(Pawn pawn, GeneDef gene);
}

public class DormantConnection : Verse.DefModExtension {
    public Type callbackHandler = null!;

    public IDormantConnectionCallback? Handler => DormantConnectionCallbackRegistry.Get(callbackHandler);

    public override IEnumerable<string> ConfigErrors() {
        if (callbackHandler == null) {
            yield return "callbackHandler is null";
            yield break;
        }

        if (!typeof(IDormantConnectionCallback).IsAssignableFrom(callbackHandler)) {
            yield return $"callbackHandler '{callbackHandler.FullName}' is not assignable to IDormantConnectionCallback";
        }

        if (DormantConnectionCallbackRegistry.Get(callbackHandler) == null) {
            yield return $"callbackHandler '{callbackHandler.FullName}' is not registered in DormantConnectionCallbackRegistry";
        }
    }
}
