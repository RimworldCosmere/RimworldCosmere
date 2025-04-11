using Verse;

namespace Cosmere.Scadrial.Startup.DefGenerator {
    [StaticConstructorOnStartup]
    public static class DefGenerators {
        static DefGenerators() {
            TraitDefGenerator.Generate();
            // Genes depend on Traits, need to be created after
            GeneDefGenerator.Generate();
        }
    }
}