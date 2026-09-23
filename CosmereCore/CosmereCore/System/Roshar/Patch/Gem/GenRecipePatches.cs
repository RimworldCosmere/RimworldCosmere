using Concord;
using Verse;

namespace Cosmere.System.Roshar.Patch.Gem;

[Patch(typeof(GenRecipe))]
public static class GenRecipePatch {
    [Inject(At.Return, nameof(GenRecipe.MakeRecipeProducts))]
    private static void AfterMakeRecipeProducts(
        Verse.Thing dominantIngredient,
        ControlHandle<IEnumerable<Verse.Thing>> ch
    ) {
        ch.ReturnValue = RestuffGemProducts(ch.ReturnValue, dominantIngredient);
    }

    private static IEnumerable<Verse.Thing> RestuffGemProducts(
        IEnumerable<Verse.Thing> result,
        Verse.Thing dominantIngredient
    ) {
        List<Verse.Thing> products = result.ToList();
        if (products.Count != 1) {
            foreach (Verse.Thing product in products) {
                yield return product;
            }

            yield break;
        }

        Verse.Thing firstResult = products[0];
        if (!firstResult.def.IsOneOf(
                ThingDefOf.Cosmere_Roshar_Thing_Chip,
                ThingDefOf.Cosmere_Roshar_Thing_Mark,
                ThingDefOf.Cosmere_Roshar_Thing_Broam
            )) {
            yield return firstResult;
            yield break;
        }

        firstResult.SetStuffDirect(dominantIngredient.Stuff);

        yield return firstResult;
    }
}
