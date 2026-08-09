using System;
using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Framework;

/// <summary>
///     Who a pawn actually is, when something on the map is wearing somebody else.
/// </summary>
/// <remarks>
///     A kandra in an animal shape is a different <see cref="Pawn" /> object with a different
///     load ID, so everything keyed by the pawn - connections, the log, the social tab - looks at
///     a stranger. Copying that state onto the shape and merging it back would mean two copies
///     that drift, and the log cannot be copied at all: its entries live in one global list and
///     hold pawn references.
///     <para>
///         So nothing is copied. Systems that care ask here instead, and get the person rather
///         than the body. Shards register their own unwrapping because Core must not know what a
///         kandra is.
///     </para>
/// </remarks>
public static class PawnIdentityRegistry {
    private static readonly List<Func<Pawn, Pawn?>> resolvers = [];

    public static void Register(Func<Pawn, Pawn?> resolver) {
        resolvers.Add(resolver);
    }

    /// <summary>The pawn behind this one, or the pawn itself when it is nobody but itself.</summary>
    public static Pawn Real(Pawn pawn) {
        for (int i = 0; i < resolvers.Count; i++) {
            try {
                Pawn? behind = resolvers[i](pawn);
                if (behind != null && behind != pawn) return behind;
            } catch (Exception ex) {
                Logger.Warning($"PawnIdentityRegistry: resolver {i} threw: {ex}");
            }
        }

        return pawn;
    }

    /// <summary>Whether this pawn is a body somebody else is walking around in.</summary>
    public static bool IsWorn(Pawn pawn) {
        return Real(pawn) != pawn;
    }

    /// <summary>Resolves anything a connection can be anchored to, leaving non-pawns alone.</summary>
    public static ILoadReferenceable Real(ILoadReferenceable target) {
        return target is Pawn pawn ? Real(pawn) : target;
    }
}
