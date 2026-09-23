using System;
using Verse;

namespace Cosmere.Core.Framework;

public static class ConnectionStealRegistry {
    private static readonly List<IConnectionStealHandler> handlers = [];

    public static void Register(IConnectionStealHandler handler) {
        handlers.Add(handler);
    }

    public static void NotifyConnectionStolen(Pawn donor) {
        for (int i = 0; i < handlers.Count; i++) {
            try {
                handlers[i].OnConnectionStolen(donor);
            } catch (Exception ex) {
                Log.Warn($"ConnectionStealRegistry: handler {handlers[i].GetType().Name} threw: {ex}");
            }
        }
    }
}
