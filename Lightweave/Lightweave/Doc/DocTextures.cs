using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Doc;

[StaticConstructorOnStartup]
public static class DocTextures {
    public static readonly Texture2D Copy = ContentFinder<Texture2D>.Get("UI/Lightweave/Copy");
}
