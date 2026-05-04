using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public interface IGemstoneHandler {
    void RemoveGemstone();
    void AddGemstone(ThingWithComps gemstone);
}
