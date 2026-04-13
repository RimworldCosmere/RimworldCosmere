using Cosmere.Core.Def;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Graphic;

public class GemStuff : Verse.Graphic {
    protected Verse.Graphic[] subGraphics = null!;

    public override Material MatSingle => subGraphics[GemDefOf.Diamond.index].MatSingle;

    private ThingDef StuffOfThing(Verse.Thing thing) {
        return thing is IConstructible constructible ? constructible.EntityToBuildStuff() : thing.Stuff;
    }

    public override Material MatAt(Rot4 rot, Verse.Thing? thing = null) {
        return SubGraphicFor(thing).MatAt(rot, thing);
    }

    public override void Init(GraphicRequest req) {
        data = req.graphicData;
        path = req.path;
        color = req.color;
        drawSize = req.drawSize;
        List<GemDef> defsListForReading = DefDatabase<GemDef>.AllDefsListForReading;
        subGraphics = new Verse.Graphic[defsListForReading.Count];
        for (int index = 0; index < subGraphics.Length; ++index) {
            GemDef gem = defsListForReading[index];
            string folderPath = req.path;
            Texture2D texture2D = ContentFinder<Texture2D>.GetAllInFolder(folderPath)
                .Where(x => x.name.EndsWith(gem.defName))
                .First();
            subGraphics[index] = GraphicDatabase.Get<Graphic_Single>(
                $"{folderPath}/{texture2D.name}",
                req.shader,
                drawSize,
                color
            );
        }
    }

    public override Verse.Graphic GetColoredVersion(UnityEngine.Shader newShader, Color newColor, Color newColorTwo) {
        if (newColorTwo != Color.white) {
            Logger.Error("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.");
        }

        return GraphicDatabase.Get<GemStuff>(path, newShader, drawSize, newColor, Color.white, data);
    }

    public override Material MatSingleFor(Verse.Thing thing) {
        return SubGraphicFor(thing).MatSingleFor(thing);
    }

    public override void DrawWorker(
        Vector3 loc,
        Rot4 rot,
        ThingDef thingDef,
        Verse.Thing thing,
        float extraRotation
    ) {
        SubGraphicFor(thing).DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }

    public Verse.Graphic SubGraphicFor(Verse.Thing? thing) {
        return thing != null ? SubGraphicFor(StuffOfThing(thing)) : subGraphics[GemDefOf.Diamond.index];
    }

    public Verse.Graphic SubGraphicFor(ThingDef? stuff) {
        GemDef gem = GemDefOf.Diamond;
        if (stuff != null) {
            GemDef? found = DefDatabase<GemDef>.GetNamedSilentFail(stuff.defName.Replace("Raw", ""));
            if (found != null) gem = found;
        }

        return SubGraphicFor(gem);
    }

    public Verse.Graphic SubGraphicFor(GemDef app) {
        return subGraphics[app.index];
    }

    public override string ToString() {
        return $"GemStuff(path={path}, color={color.ToString()}, colorTwo=unsupported)";
    }
}