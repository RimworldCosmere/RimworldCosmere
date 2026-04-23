using System;
using System.Collections.Generic;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Playground;

internal static class PlaygroundDemos
{
    private static readonly IReadOnlyList<PlaygroundVariant> EmptyVariants = Array.Empty<PlaygroundVariant>();
    private static readonly IReadOnlyList<PlaygroundState> EmptyStates = Array.Empty<PlaygroundState>();

    internal static (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) Build(
        string id,
        bool forceDisabled)
    {
        switch (id)
        {
            default:
                return (EmptyVariants, EmptyStates);
        }
    }
}
