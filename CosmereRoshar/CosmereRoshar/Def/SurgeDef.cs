using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Def;

public class SurgeDef : Verse.Def {
    public List<AbilityDef> abilities;
    public Texture2D icon;
    public Texture2D invertedIcon;

    public override void PostLoad() {
        base.PostLoad();

        LongEventHandler.ExecuteWhenFinished(() => {
                icon = ContentFinder<Texture2D>.Get($"UI/Surgebinding/Surge/{defName}");
                if (icon != null) {
                    invertedIcon = icon.CloneTexture().InvertColors();
                }
            }
        );
    }
}