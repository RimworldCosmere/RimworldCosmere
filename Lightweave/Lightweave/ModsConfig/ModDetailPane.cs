using System;
using System.Collections.Generic;
using System.Linq;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Steam;
using Display = Cosmere.Lightweave.Typography.Display;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.ModsConfig;

public static class ModDetailPane {
    public static LightweaveNode Create(ModMetaData? mod, RimWorld.Page_ModsConfig page, Action onClose) {
        if (mod == null) {
            return BuildEmpty();
        }

        return Box.Create(
            children: c => c.Add(HStack.Create(SpacingScale.Lg, h => {
                h.AddFlex(ScrollArea.Create(
                    content: BuildBody(mod),
                    showScrollbar: true
                ));
                h.Add(BuildActionsColumn(mod, page, onClose), new Rem(14f).ToPixels());
            })),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Lg),
                Background = BackgroundSpec.Of(ThemeSlot.SurfacePrimary),
            }
        );
    }

    private static LightweaveNode BuildBody(ModMetaData mod) {
        return Stack.Create(SpacingScale.Md, s => {
            s.Add(BuildHeader(mod));
            s.Add(KeyValueTable.Create(BuildStatRows(mod), KeyValueOrientation.Horizontal));
            string description = mod.Description ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(description)) {
                s.Add(Text.Create(description, color: ThemeSlot.TextSecondary, wrap: true));
            }
            if (mod.Dependencies != null && mod.Dependencies.Count > 0) {
                s.Add(BuildDependencies(mod));
            }
        });
    }

    private static LightweaveNode BuildHeader(ModMetaData mod) {
        return Stack.Create(SpacingScale.Xs, s => {
            s.Add(Eyebrow.Create(BuildSourceLabel(mod), color: SourceTone(mod)));
            s.Add(Display.Create(mod.Name ?? mod.PackageId, level: 2, align: TextAlign.Start));
            string author = mod.AuthorsString ?? string.Empty;
            if (!string.IsNullOrEmpty(author)) {
                s.Add(Text.Create("CL_ModsConfig_AuthorByline".Translate(author.Named("AUTHOR")).Resolve(),
                    color: ThemeSlot.TextMuted));
            }
        });
    }

    private static List<KeyValueRow> BuildStatRows(ModMetaData mod) {
        return new List<KeyValueRow> {
            new KeyValueRow(
                "CL_ModsConfig_Stat_Status".Translate(),
                mod.Active
                    ? "CL_ModsConfig_Status_Active".Translate()
                    : "CL_ModsConfig_Status_Inactive".Translate()
            ),
            new KeyValueRow(
                "CL_ModsConfig_Stat_Version".Translate(),
                string.IsNullOrEmpty(mod.ModVersion) ? "—" : mod.ModVersion
            ),
            new KeyValueRow(
                "CL_ModsConfig_Stat_PackageId".Translate(),
                mod.PackageIdPlayerFacing ?? mod.PackageId ?? "—"
            ),
            new KeyValueRow(
                "CL_ModsConfig_Stat_Source".Translate(),
                BuildSourceLabel(mod)
            ),
        };
    }

    private static LightweaveNode BuildDependencies(ModMetaData mod) {
        return Stack.Create(SpacingScale.Xs, s => {
            s.Add(Eyebrow.Create("CL_ModsConfig_Dependencies".Translate()));
            foreach (ModRequirement req in mod.Dependencies) {
                bool met = req.IsSatisfied;
                s.Add(Text.Create(
                    "  •  " + (req.displayName ?? req.packageId ?? string.Empty),
                    color: met ? ThemeSlot.TextSecondary : ThemeSlot.StatusWarning
                ));
            }
        });
    }

    private static LightweaveNode BuildActionsColumn(ModMetaData mod, RimWorld.Page_ModsConfig page, Action onClose) {
        bool active = mod.Active;
        bool locked = mod.IsCoreMod;
        return Stack.Create(SpacingScale.Sm, s => {
            s.Add(Eyebrow.Create("CL_ModsConfig_Actions".Translate()));
            s.Add(Button.Create(
                label: active
                    ? "CL_ModsConfig_Action_Disable".Translate()
                    : "CL_ModsConfig_Action_Enable".Translate(),
                onClick: () => Verse.ModsConfig.SetActive(mod, !active),
                variant: active ? ButtonVariant.Secondary : ButtonVariant.Primary,
                fullWidth: true,
                disabled: locked
            ));
            if (active) {
                s.Add(Button.Create(
                    label: "CL_ModsConfig_Action_MoveUp".Translate(),
                    onClick: () => MoveMod(mod, -1),
                    variant: ButtonVariant.Ghost,
                    fullWidth: true,
                    disabled: locked
                ));
                s.Add(Button.Create(
                    label: "CL_ModsConfig_Action_MoveDown".Translate(),
                    onClick: () => MoveMod(mod, 1),
                    variant: ButtonVariant.Ghost,
                    fullWidth: true,
                    disabled: locked
                ));
            }
            if (mod.OnSteamWorkshop && SteamManager.Initialized) {
                s.Add(Button.Create(
                    label: "CL_ModsConfig_Action_Workshop".Translate(),
                    onClick: () => SteamUtility.OpenWorkshopPage(new Steamworks.PublishedFileId_t(mod.GetPublishedFileId().m_PublishedFileId)),
                    variant: ButtonVariant.Ghost,
                    fullWidth: true
                ));
            }
            else if (!string.IsNullOrEmpty(mod.Url)) {
                s.Add(Button.Create(
                    label: "CL_ModsConfig_Action_Url".Translate(),
                    onClick: () => Application.OpenURL(mod.Url),
                    variant: ButtonVariant.Ghost,
                    fullWidth: true
                ));
            }
            s.Add(Button.Create(
                label: "CL_ModsConfig_Action_Close".Translate(),
                onClick: () => onClose?.Invoke(),
                variant: ButtonVariant.Ghost,
                fullWidth: true
            ));
        });
    }

    private static void MoveMod(ModMetaData mod, int delta) {
        List<ModMetaData> active = Verse.ModsConfig.ActiveModsInLoadOrder.ToList();
        int idx = active.FindIndex(m => string.Equals(m.PackageId, mod.PackageId, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) {
            return;
        }
        int target = Mathf.Clamp(idx + delta, 0, active.Count - 1);
        if (target == idx) {
            return;
        }
        Verse.ModsConfig.TryReorder(idx, target, out _);
    }

    private static string BuildSourceLabel(ModMetaData mod) {
        if (mod.IsCoreMod) {
            return "CL_ModsConfig_Source_Core".Translate();
        }
        if (mod.Official) {
            return "CL_ModsConfig_Source_Expansion".Translate();
        }
        if (mod.OnSteamWorkshop) {
            return "CL_ModsConfig_Source_Workshop".Translate();
        }
        return "CL_ModsConfig_Source_Local".Translate();
    }

    private static ThemeSlot SourceTone(ModMetaData mod) {
        if (mod.IsCoreMod || mod.Official) {
            return ThemeSlot.StatusSuccess;
        }
        return ThemeSlot.TextMuted;
    }

    private static LightweaveNode BuildEmpty() {
        return Container.Create(
            child: Stack.Create(SpacingScale.Sm, s => {
                s.Add(Eyebrow.Create("CL_ModsConfig_Empty_Eyebrow".Translate()));
                s.Add(Text.Create("CL_ModsConfig_Empty_Body".Translate(), color: ThemeSlot.TextSecondary, wrap: true));
            }),
            padding: EdgeInsets.All(SpacingScale.Lg)
        );
    }
}
