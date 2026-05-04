using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Doc;

public static class DocChips {
    public static LightweaveNode Chip(string text, ThemeSlot bg, ThemeSlot fg) {
        return Box.Create(
            EdgeInsets.All(SpacingScale.Xs),
            new BackgroundSpec.Solid(bg),
            null,
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(Text.Create(text, FontRole.Body, new Rem(0.8125f), fg, TextAlign.Center))
        );
    }

    public static LightweaveNode SampleChip(string text) {
        return Chip(text, ThemeSlot.SurfaceSunken, ThemeSlot.TextPrimary);
    }

    public static LightweaveNode AccentChip(string text) {
        return Chip(text, ThemeSlot.SurfaceAccent, ThemeSlot.TextOnAccent);
    }

    public static LightweaveNode MutedChip(string text) {
        return Chip(text, ThemeSlot.SurfaceSunken, ThemeSlot.TextMuted);
    }

    public static LightweaveNode CenterFixed(LightweaveNode child, float width, float height) {
        LightweaveNode node = NodeBuilder.New("CenterFixed", 0, nameof(DocChips));
        node.Children.Add(child);
        node.Paint = (rect, _) => {
            float w = Mathf.Min(width, rect.width);
            float h = Mathf.Min(height, rect.height);
            float x = rect.x + (rect.width - w) * 0.5f;
            float y = rect.y + (rect.height - h) * 0.5f;
            Rect inner = new Rect(x, y, w, h);
            child.MeasuredRect = inner;
            LightweaveRoot.PaintSubtree(child, inner);
        };
        return node;
    }
}
