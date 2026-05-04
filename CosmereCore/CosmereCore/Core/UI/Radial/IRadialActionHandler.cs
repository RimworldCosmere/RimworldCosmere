
using Verse;
namespace Cosmere.Core.UI.Radial;

public interface IRadialActionHandler {
    bool CanHandle(RadialActionKind kind);
    void Dispatch(Pawn pawn, RadialLeaf leaf, string subsystemId, bool flareShift);
}
