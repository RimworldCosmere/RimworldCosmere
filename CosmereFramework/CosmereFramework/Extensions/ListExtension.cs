using System.Collections.Generic;
using Verse;

namespace CosmereFramework.Extensions;

public static class ListExtension {
    public static bool TryPopFront<T>(this List<T> list, out T element) {
        if (list.Count == 0) {
            element = default!;
            return false;
        }

        element = list.PopFront();
        return true;
    }
}