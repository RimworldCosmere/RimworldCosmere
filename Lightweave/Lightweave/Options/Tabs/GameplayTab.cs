using System;
using System.Globalization;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Options.Tabs;

public static class GameplayTab {
    private static readonly AutomaticPauseMode[] PauseModes = (AutomaticPauseMode[])Enum.GetValues(typeof(AutomaticPauseMode));

    public static LightweaveNode Build() {
        bool inGame = Current.ProgramState == ProgramState.Playing;

        return Stack.Create(SpacingScale.Lg, s => {
            if (inGame) {
                s.Add(SettingRow.Section("CL_Options_Section_Storyteller",
                    SettingRow.Create(
                        "CL_Options_ChangeStoryteller".Translate(),
                        Button.Create(
                            label: "CL_Options_ChangeStoryteller_Action".Translate(),
                            onClick: () => {
                                if (TutorSystem.AllowAction("ChooseStoryteller")) {
                                    Find.WindowStack.Add(new Page_SelectStorytellerInGame());
                                }
                            },
                            variant: ButtonVariant.Secondary
                        )
                    )
                ));
            }
            s.Add(SettingRow.Section("CL_Options_Section_Combat",
                SettingRow.Create(
                    "CL_Options_MaxSettlements".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.MaxNumberOfPlayerSettlements,
                        onChange: v => Prefs.MaxNumberOfPlayerSettlements = Mathf.Clamp(Mathf.RoundToInt(v), 1, 5),
                        min: 1f,
                        max: 5f,
                        step: 1f,
                        format: v => Mathf.RoundToInt(v).ToString(CultureInfo.InvariantCulture)
                    )
                ),
                SettingRow.Create(
                    "CL_Options_PauseOnLoad".Translate(),
                    Switch.Create("", Prefs.PauseOnLoad, v => Prefs.PauseOnLoad = v)
                ),
                SettingRow.Create(
                    "CL_Options_AutoPauseMode".Translate(),
                    Dropdown.Create(
                        value: Prefs.AutomaticPauseMode,
                        options: PauseModes,
                        labelFn: m => m.ToStringHuman(),
                        onChange: m => Prefs.AutomaticPauseMode = m
                    )
                ),
                SettingRow.Create(
                    "CL_Options_AdaptiveTraining".Translate(),
                    Switch.Create("", Prefs.AdaptiveTrainingEnabled, v => Prefs.AdaptiveTrainingEnabled = v)
                ),
                SettingRow.Create(
                    "CL_Options_ResetTutor".Translate(),
                    Button.Create(
                        label: "CL_Options_ResetTutor_Action".Translate(),
                        onClick: () => Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "ConfirmResetLearningHelper".Translate(),
                            () => {
                                Messages.Message("AdaptiveTutorIsReset".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
                                PlayerKnowledgeDatabase.ResetPersistent();
                            },
                            destructive: true
                        )),
                        variant: ButtonVariant.Secondary
                    )
                )
            ));
            s.Add(SettingRow.Section("CL_Options_Section_Names",
                SettingRow.Create(
                    "CL_Options_PreferredNames".Translate(),
                    Button.Create(
                        label: "CL_Options_PreferredNames_Action".Translate(),
                        onClick: () => Find.WindowStack.Add(new Dialog_AddPreferredName()),
                        variant: ButtonVariant.Secondary,
                        disabled: Prefs.PreferredNames.Count >= 6
                    ),
                    caption: PreferredNamesCaption()
                )
            ));
        });
    }


    private static string PreferredNamesCaption() {
        int count = Prefs.PreferredNames.Count;
        if (count == 0) {
            return (string)"NamesYouWantToSeeSubText".Translate();
        }
        return count + " / 6 " + (string)"NamesYouWantToSeeSubText".Translate();
    }
}
