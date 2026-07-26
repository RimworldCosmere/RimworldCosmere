using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Def;

public class SurgeDef : Verse.Def {
    public List<AbilityDef> abilities = null!;
    public Texture2D icon = null!;
    public Texture2D invertedIcon = null!;
    public RecordDef? timeSpentRecord;

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