using System.Collections.Generic;
using Cosmere.Lightweave.Feedback;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Steam;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.MainMenu;

public static class ExpansionRow {
    public static LightweaveNode? Create() {
        List<ExpansionDef> all = ModLister.AllExpansions;
        List<ExpansionDef> visible = new List<ExpansionDef>();
        for (int i = 0; i < all.Count; i++) {
            if (!all[i].isCore) {
                visible.Add(all[i]);
            }
        }
        if (visible.Count == 0) {
            return null;
        }

        LightweaveNode rowBox = Box.Create(
            children: c => c.Add(HStack.Create(SpacingScale.Sm, h => {
                h.AddHug(Eyebrow.Create(
                    "CL_MainMenu_ExpansionsActive".Translate(),
                    style: new Style { TextColor = ThemeSlot.SurfaceAccent, TextAlign = TextAlign.Left },
                    letterSpacing: 2.5f
                ));
                h.AddHug(Divider.Vertical());
                for (int i = 0; i < visible.Count; i++) {
                    h.AddHug(BuildItem(visible[i]));
                }
            })),
            style: new Style {
                Padding = new EdgeInsets(SpacingScale.Xs, SpacingScale.Md, SpacingScale.Xs, SpacingScale.Md),
                Background = BackgroundSpec.Blur(new Color(15f / 255f, 12f / 255f, 8f / 255f, 0.78f)),
                Border = BorderSpec.All(new Rem(0.0625f), ThemeSlot.AccentMuted),
                Radius = RadiusSpec.All(RadiusScale.None),
            }
        );

        return HStack.Create(
            SpacingScale.None,
            h => {
                h.AddFlex(Spacer.Flex());
                h.AddHug(rowBox);
                h.AddFlex(Spacer.Flex());
            }
        );
    }

    private static LightweaveNode BuildItem(ExpansionDef expansion) {
        ExpansionStatus status = expansion.Status;
        bool active = status == ExpansionStatus.Active;
        ThemeSlot textSlot = active ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted;

        Texture2D? icon = expansion.IconFromStatus;
        string label = expansion.LabelCap.ToString().ToUpperInvariant();

        return Box.Create(
            children: c => c.Add(HStack.Create(SpacingScale.Xs, h => {
                if (icon != null) {
                    h.AddHug(Icon.Create(icon, size: new Rem(1.25f)));
                }
                h.AddHug(Eyebrow.Create(label, style: new Style { TextColor = textSlot, TextAlign = TextAlign.Left }, letterSpacing: 1.4f));
            })),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.Xs, Right: SpacingScale.Sm, Bottom: SpacingScale.Xs, Left: SpacingScale.Sm),
                Background = BackgroundSpec.Of(new Color(28f / 255f, 22f / 255f, 14f / 255f, 0.85f)),
                Border = BorderSpec.All(new Rem(0.0625f), ThemeSlot.AccentMuted),
                Radius = RadiusSpec.All(RadiusScale.None),
            }
        );
    }

    

    
}
