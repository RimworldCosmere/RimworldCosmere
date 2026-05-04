using Cosmere.Core.DefModExtension;
using Verse;

namespace Cosmere.System.Scadrial.DormantConnection.Callback;

public class Snapped : IDormantConnectionCallback {
    public bool Callback(Pawn pawn, GeneDef gene) {
        return pawn.IsSnapped();
    }
}