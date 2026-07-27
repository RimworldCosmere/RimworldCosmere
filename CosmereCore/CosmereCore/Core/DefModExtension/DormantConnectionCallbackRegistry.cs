using System;

namespace Cosmere.Core.DefModExtension;

public static class DormantConnectionCallbackRegistry {
    private static readonly Dictionary<Type, IDormantConnectionCallback> Registered = new();

    public static void Register<T>(T instance)
        where T : IDormantConnectionCallback {
        Registered[typeof(T)] = instance;
    }

    public static IDormantConnectionCallback? Get(Type type) {
        return Registered.TryGetValue(type, out IDormantConnectionCallback? cb) ? cb : null;
    }
}
