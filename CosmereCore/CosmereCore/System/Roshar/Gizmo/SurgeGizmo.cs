using Cosmere.Core.UI;
using Cosmere.Lightweave.Adapter;
using Cosmere.Lightweave.Runtime;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Gizmo;

[StaticConstructorOnStartup]
public class SurgeGizmo : AsGizmo {
    private static readonly Vector2 Padding = new Vector2(2f, 4f);
    private readonly Surgebinder gene;
    private readonly List<SurgebindingAbilitySubGizmo> subgizmos = [];
    private readonly SurgeDef surgeDef;
    private int cachedIdeal = -1;
    private bool initialized;

    public SurgeGizmo(Surgebinder gene, SurgeDef surgeDef)
        : base(gene.pawn.thingIDNumber, surgeDef.shortHash) {
        this.gene = gene;
        this.surgeDef = surgeDef;
    }

    public override float Order => -100f;

    private float iconSize => Height / 2f;

    private int visibleAbilityCount {
        get {
            int count = 0;
            for (int i = 0; i < surgeDef.abilities.Count; i++) {
                AbilityDef abilityDef = surgeDef.abilities[i];
                if (abilityDef is SurgebindingAbilityDef surgeAbilityDef) {
                    int minIdeal = surgeAbilityDef.GetMinIdealForOrder(gene.radiantOrderDef.defName);
                    if (gene.CurrentIdeal < minIdeal) continue;
                }

                if (!gene.pawn.TryGetAbility(abilityDef, out SurgebindingAbility? ability) || ability == null) continue;
                if (!ability.GizmosVisible()) continue;
                count++;
            }

            return count;
        }
    }

    public override bool Visible => gene.pawn.Faction.IsPlayer && !gene.gizmoShrunk && visibleAbilityCount > 0;

    public override float GetWidth(float maxWidth) {
        int count = visibleAbilityCount;
        if (count == 0) return 0;
        return Height + Padding.x + (iconSize + Padding.x) * count;
    }

    private void Initialize() {
        if (initialized && cachedIdeal == gene.CurrentIdeal) return;
        initialized = true;
        cachedIdeal = gene.CurrentIdeal;

        subgizmos.Clear();
        Pawn pawn = gene.pawn;

        for (int i = 0; i < surgeDef.abilities.Count; i++) {
            AbilityDef abilityDef = surgeDef.abilities[i];
            if (abilityDef is SurgebindingAbilityDef surgeAbilityDef) {
                int minIdeal = surgeAbilityDef.GetMinIdealForOrder(gene.radiantOrderDef.defName);
                if (gene.CurrentIdeal < minIdeal) continue;
            }

            if (!pawn.TryGetAbility(abilityDef, out SurgebindingAbility? ability) || ability == null) continue;
            if (!ability.GizmosVisible()) continue;

            subgizmos.Add(new SurgebindingAbilitySubGizmo(this, gene, ability));
        }
    }

    public void ClearCache() {
        initialized = false;
    }

    protected override LightweaveNode Build() {
        Initialize();
        LightweaveNode node = new LightweaveNode { DebugName = "SurgeGizmo" };
        node.Paint = (outerRect, _) => PaintSurge(outerRect);
        return node;
    }

    private void PaintSurge(Rect outerRect) {
        bool mouseOver = false;

        Rect mainRect = outerRect.ContractedBy(Padding.x * 2, Padding.y);

        Widgets.DrawWindowBackground(outerRect);

        Rect surgeIconRect = new Rect(mainRect.x, mainRect.y, mainRect.height, mainRect.height);
        Texture2D icon = surgeDef.icon ?? BaseContent.BadTex;
        UIHelpers.DrawIcon(surgeIconRect, icon, BGTex, TexUI.GrayscaleGUI, doBorder: false);

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter)) {
            float textHeight = Text.CalcHeight(surgeDef.LabelCap, surgeIconRect.width);
            Rect labelRect = new Rect(
                surgeIconRect.x,
                mainRect.yMax - textHeight,
                surgeIconRect.width,
                textHeight
            );
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            Widgets.Label(labelRect, surgeDef.LabelCap);
        }

        Rect abilityRect = new Rect(
            surgeIconRect.xMax + Padding.x,
            mainRect.y,
            iconSize,
            iconSize
        );

        foreach (SurgebindingAbilitySubGizmo subgizmo in subgizmos) {
            GizmoResult result = subgizmo.OnGUI(abilityRect);
            switch (result.State) {
                case GizmoState.Interacted:
                    subgizmo.ProcessInput(result.InteractEvent);
                    break;
                case GizmoState.Mouseover:
                    subgizmo.OnUpdate(abilityRect);
                    mouseOver = true;
                    break;
            }

            abilityRect.x += iconSize + Padding.x;
        }

        if (Mouse.IsOver(surgeIconRect)) {
            Widgets.DrawHighlight(surgeIconRect);
            TooltipHandler.TipRegion(surgeIconRect, surgeDef.description);
        }
        else if (Mouse.IsOver(mainRect) && !mouseOver) {
            Widgets.DrawHighlight(mainRect);
        }
    }
}