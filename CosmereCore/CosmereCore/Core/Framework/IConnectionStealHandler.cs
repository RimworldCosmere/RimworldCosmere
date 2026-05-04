
using Verse;
namespace Cosmere.Core.Framework;

public interface IConnectionStealHandler {
    void OnConnectionStolen(Pawn donor);
}
