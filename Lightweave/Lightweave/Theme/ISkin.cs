using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public interface ISkin {
    Color? GetColor(ThemeSlot slot);
    Font? GetFont(FontRole role);
}
