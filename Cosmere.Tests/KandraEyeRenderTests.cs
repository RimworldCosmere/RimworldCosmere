using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A miswired eye node draws nothing, draws through a stolen face, or quietly collapses both
///     irises onto one colour, and none of those three log anything.
/// </summary>
[TestClass]
public class KandraEyeRenderTests {
    private const string EyeNodeClass = "PawnRenderNode_KandraEyes";
    private const string GlowNodeClass = "PawnRenderNode_KandraEyeGlow";
    private const string FormlessSubWorker = "PawnRenderSubWorker_ShowOnlyWhileFormless";

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir.FullName;
        }
    }

    private static string EyeNodeSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Kandra",
        EyeNodeClass + ".cs"
    );

    private static string EyeNodeSource() {
        Assert.IsTrue(
            File.Exists(EyeNodeSourcePath),
            $"{EyeNodeClass}.cs does not exist, so the def below names a class nothing declares."
        );
        return File.ReadAllText(EyeNodeSourcePath);
    }

    private static string AppearanceSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Util",
        "KandraAppearance.cs"
    );

    /// <summary>The iris size names the palette offers, read straight out of the table that defines them.</summary>
    private static string[] IrisSizeNames() {
        string source = File.ReadAllText(AppearanceSourcePath);

        Match table = Regex.Match(source, @"IrisSizes\s*=\s*\[(?<rows>.*?)\];", RegexOptions.Singleline);
        Assert.IsTrue(table.Success, "KandraAppearance no longer declares an IrisSizes table, so nothing says what sizes exist.");

        string[] names = Regex.Matches(table.Groups["rows"].Value, @"\(\s*""(?<name>[^""]+)""")
            .Select(m => m.Groups["name"].Value)
            .ToArray();

        Assert.AreNotEqual(0, names.Length, "The IrisSizes table parsed to no names at all.");
        return names;
    }

    private static string TitleCase(string name) {
        return char.ToUpperInvariant(name[0]) + name.Substring(1).ToLowerInvariant();
    }

    private static string EyeTextureFile(string gender, string size, string suffix) {
        return Path.Combine(
            RepoRoot,
            "CosmereScadrial",
            "Assets",
            "Textures",
            "Things",
            "Pawn",
            "Humanlike",
            "HeadAttachments",
            "KandraEyes",
            $"Kandra_Eyes_{gender}_{size}_{suffix}.png"
        );
    }

    private static XElement[] EyeRenderNodes() {
        XDocument doc = XDocument.Load(Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Races", "Genes", "Kandra.xml"));

        XElement? gene = doc.Descendants("GeneDef")
            .FirstOrDefault(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_TrueBody");
        Assert.IsNotNull(gene, "Cosmere_Scadrial_Gene_TrueBody is gone, so nothing hangs the eyes off the true body.");

        XElement? block = gene.Element("renderNodeProperties");
        Assert.IsNotNull(
            block,
            "Cosmere_Scadrial_Gene_TrueBody declares no renderNodeProperties, so a kandra in its own "
            + "shape has a blank face."
        );

        return block.Elements("li").ToArray();
    }

    private static XElement NodeFor(string nodeClass) {
        XElement? node = EyeRenderNodes()
            .FirstOrDefault(li => li.ToString().Contains(nodeClass, StringComparison.Ordinal));
        Assert.IsNotNull(node, $"No render node on the true-body gene names {nodeClass}.");
        return node;
    }

    [TestMethod]
    public void AMissingEyeOrGlowNodeLeavesTheTrueBodyFaceBlank() {
        string block = string.Concat(EyeRenderNodes().Select(li => li.ToString()));

        foreach (string nodeClass in new[] { EyeNodeClass, GlowNodeClass }) {
            StringAssert.Contains(
                block,
                nodeClass,
                $"The true-body gene never names {nodeClass}, so that half of the eye never draws."
            );
        }
    }

    [TestMethod]
    public void EyesDrawnWithoutTheFormlessVetoShowThroughAStolenFace() {
        foreach (string nodeClass in new[] { EyeNodeClass, GlowNodeClass }) {
            StringAssert.Contains(
                NodeFor(nodeClass).ToString(),
                FormlessSubWorker,
                $"{nodeClass} has no {FormlessSubWorker}. Without it a kandra wearing a stolen face "
                + "shows its own eyes through that face, which is the one rule Ka set for this feature."
            );
        }
    }

    [TestMethod]
    public void APlainCutoutShaderCollapsesBothIrisesOntoOneColour() {
        XElement node = NodeFor(EyeNodeClass);
        string? shader = node.Element("shaderTypeDef")?.Value;

        Assert.AreNotEqual(
            "Cutout",
            shader,
            "The eye node is declared with a plain Cutout shader. Cutout reports no _MaskTex, so the "
            + "mask is ignored and both eyes silently take one colour - odd eyes stops working with "
            + "no error anywhere."
        );

        string evidence = node + EyeNodeSource();
        StringAssert.Contains(
            evidence,
            "CutoutComplex",
            "Nothing in the eye node or its def names a mask-capable shader, so Graphic_Multi never "
            + "looks for the _m files and the right eye takes the left eye's colour."
        );
    }

    [TestMethod]
    public void ATexPathWithNoFileBehindItDrawsNothingAndLogsNothing() {
        string[] paths = EyeRenderNodes()
            .SelectMany(li => li.Descendants("texPath"))
            .Select(e => e.Value)
            .ToArray();

        Assert.AreNotEqual(0, paths.Length, "No render node on the true-body gene names a texture.");

        foreach (string texPath in paths) {
            string stem = Path.Combine(
                RepoRoot,
                "CosmereScadrial",
                "Assets",
                "Textures",
                texPath.Replace('/', Path.DirectorySeparatorChar)
            );

            Assert.IsTrue(
                File.Exists(stem + "_south.png"),
                $"The def asks for {texPath}, but {texPath}_south.png is not on disk."
            );
        }
    }

    [TestMethod]
    public void LoadingTheEyeTextureInTheDrawPathCostsOneLookupPerKandraPerFrame() {
        string source = EyeNodeSource();

        Assert.IsFalse(
            source.Contains("ContentFinder", StringComparison.Ordinal),
            "The eye node calls ContentFinder. This runs per node per frame for every kandra on the "
            + "map; GraphicDatabase already caches, ContentFinder does not."
        );
    }

    [TestMethod]
    public void AnEyeQuadThatDoesNotMatchTheHeadSlidesTheIrisesOffTheirSockets() {
        string source = EyeNodeSource();

        Assert.IsTrue(
            source.Contains("GetHumanlikeHeadSetForPawn", StringComparison.Ordinal),
            "The eye node does not take the head's mesh set. PawnRenderNode_AttachmentHead hands back "
            + "the hair mesh, which narrows to 1.3 on the six vanilla Narrow head types, while the "
            + "kandra head is swapped onto PawnRenderNode_Head and stays 1.5. The irises then sit on "
            + "a quad 13 percent narrower than the face and slide off their painted sockets."
        );

        Assert.IsFalse(
            source.Contains("MeshPool.GetMeshSetForSize", StringComparison.Ordinal),
            "The eye node builds a mesh at its own size. Scaling the quad scales the iris offsets "
            + "with it, which walks the irises out of their sockets. The sizes are drawn in place."
        );

        Assert.IsFalse(
            source.Contains("drawSize", StringComparison.Ordinal),
            "The eye node does arithmetic on drawSize. Nothing on the node's side of the render tree "
            + "reads a graphic's own drawSize, so that maths is dead and the irises come out whatever "
            + "size the def says."
        );
    }

    [TestMethod]
    public void ASecondCopyOfTheMaleFemaleRuleDriftsFromTheOneKandraAppearanceKeeps() {
        string source = EyeNodeSource();

        StringAssert.Contains(
            source,
            "EyeGraphicPathFor",
            "The eye node does not ask KandraAppearance for its texture path, so the body, the head "
            + "and the eyes each decide male or female on their own."
        );

        Assert.IsFalse(
            source.Contains("Gender.", StringComparison.Ordinal),
            "The eye node tests gender itself on top of calling EyeGraphicPathFor. Two copies of that "
            + "rule drift, and the face ends up wearing one set of art and the eyes the other."
        );
    }

    [TestMethod]
    public void AMissingDirectionFileDrawsEyesOnTheBackOfTheHead() {
        foreach (string gender in new[] { "Male", "Female" }) {
            foreach (string size in new[] { "Small", "Standard" }) {
                foreach (string suffix in new[] { "south", "east", "north", "southm", "eastm", "northm", "westm" }) {
                    string file = EyeTextureFile(gender, size, suffix);

                    Assert.IsTrue(
                        File.Exists(file),
                        $"Kandra_Eyes_{gender}_{size}_{suffix}.png is missing. Graphic_Multi reuses _south "
                        + "rotated 180 degrees when _north is missing, which draws eyes on the back of the "
                        + "head, and a missing mask drops the right eye onto the left eye's colour."
                    );
                }
            }
        }
    }

    [TestMethod]
    public void AnIrisSizeInThePaletteWithNoArtBehindItRendersAKandraWithNoEyes() {
        foreach (string name in IrisSizeNames()) {
            foreach (string gender in new[] { "Male", "Female" }) {
                string file = EyeTextureFile(gender, TitleCase(name), "south");

                Assert.IsTrue(
                    File.Exists(file),
                    $"The palette offers the iris size '{name}', but {Path.GetFileName(file)} is not on "
                    + "disk. Adding a size to the table without drawing it renders that kandra with no "
                    + "eyes and logs nothing anywhere."
                );
            }
        }
    }
}
