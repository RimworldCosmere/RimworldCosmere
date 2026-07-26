using Verse;

namespace Cosmere.Core.Nightwatcher;

public interface INightwatcherChoiceProvider {
    IEnumerable<NightwatcherChoice> GetChoices(Verse.Def def);
}
