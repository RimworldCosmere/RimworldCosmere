using Cosmere.Core.DefModExtension;
using Verse;

namespace Cosmere.Scadrial.DormantConnection.Callback;

public class Snapped : IDormantConnectionCallback {
    public bool callback(Pawn pawn, GeneDef gene) {
        return pawn.IsSnapped();
    }
}