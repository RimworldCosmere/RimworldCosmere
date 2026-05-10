using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using UnityEngine;
using Verse;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Options.Tabs;

internal static class SettingRow {
    public static LightweaveNode Create(string label, LightweaveNode control, string? caption = null, Rem? controlWidth = null) {
        Rem cw = controlWidth ?? new Rem(24f);
        return Box.Create(
            padding: new EdgeInsets(Top: SpacingScale.Md, Bottom: SpacingScale.Md, Left: SpacingScale.None, Right: SpacingScale.None),
            children: c => c.Add(HStack.Create(SpacingScale.Lg, h => {
                h.AddFlex(Stack.Create(SpacingScale.Xxs, s => {
                    s.Add(Text.Create(label, weight: FontStyle.Bold, size: new Rem(0.95f)));
                    if (!string.IsNullOrEmpty(caption)) {
                        s.Add(Text.Create(caption!, color: ThemeSlot.TextMuted, size: new Rem(0.78f)));
                    }
                }));
                h.Add(control, cw.ToPixels());
            }))
        );
    }

    public static LightweaveNode Section(string headerKey, params LightweaveNode[] rows) {
        return Stack.Create(SpacingScale.None, s => {
            s.Add(Box.Create(
                padding: new EdgeInsets(Top: SpacingScale.None, Bottom: SpacingScale.Sm, Left: SpacingScale.None, Right: SpacingScale.None),
                children: c => c.Add(Eyebrow.Create(headerKey.Translate(), letterSpacing: 2.5f))
            ));
            s.Add(Divider.Horizontal());
            for (int i = 0; i < rows.Length; i++) {
                s.Add(rows[i]);
                if (i < rows.Length - 1) {
                    s.Add(Divider.Horizontal());
                }
            }
        });
    }
}
