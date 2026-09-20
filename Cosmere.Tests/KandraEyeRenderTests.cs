using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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

    private static readonly string[] Genders = ["Male", "Female"];
    private static readonly string[] Sides = ["Left", "Right"];
    private static readonly string[] Directions = ["south", "east", "north", "west"];

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

    private static string KandraSourceRoot => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Kandra"
    );

    /// <summary>Every C# file the kandra render nodes live in, concatenated.</summary>
    private static string EyeNodeSource() {
        string[] files = Directory.EnumerateFiles(KandraSourceRoot, "PawnRenderNode_KandraEye*.cs").ToArray();

        Assert.AreNotEqual(
            0,
            files.Length,
            "Nothing under System/Scadrial/Kandra declares a kandra eye render node, so the def below "
            + "names a class nothing declares."
        );

        return string.Concat(files.Select(File.ReadAllText));
    }

    /// <summary>The body of one class, brace-matched out of whichever file declares it.</summary>
    private static string ClassBody(string className) {
        string source = EyeNodeSource();

        Match declaration = Regex.Match(source, $@"\bclass\s+{className}\b");
        Assert.IsTrue(declaration.Success, $"No class named {className} is declared under System/Scadrial/Kandra.");

        int open = source.IndexOf('{', declaration.Index);
        Assert.AreNotEqual(-1, open, $"{className} has no body.");

        int depth = 0;
        for (int i = open; i < source.Length; i++) {
            if (source[i] == '{') depth++;
            if (source[i] == '}' && --depth == 0) return source[open..(i + 1)];
        }

        Assert.Fail($"{className}'s body never closes.");
        return string.Empty;
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

    private static string EyeTextureRoot => Path.Combine(
        RepoRoot,
        "CosmereScadrial",
        "Assets",
        "Textures",
        "Things",
        "Pawn",
        "Humanlike",
        "HeadAttachments",
        "KandraEyes"
    );

    private static string EyeTextureFile(string gender, string size, string side, string direction) {
        return Path.Combine(EyeTextureRoot, $"Kandra_Eyes_{gender}_{size}_{side}_{direction}.png");
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

        return block.Elements("li")
            .Where(li => li.ToString().Contains("PawnRenderNode_Kandra", StringComparison.Ordinal))
            .ToArray();
    }

    private static XElement[] NodesFor(string nodeClass) {
        XElement[] nodes = EyeRenderNodes()
            .Where(li => li.Element("nodeClass")?.Value.EndsWith(nodeClass, StringComparison.Ordinal) == true)
            .ToArray();

        Assert.AreNotEqual(0, nodes.Length, $"No render node on the true-body gene names {nodeClass}.");
        return nodes;
    }

    /// <summary>
    ///     Whichever way the def says "this one is the right eye" - a rightEye flag, a properties
    ///     class, or the side baked into the fallback texPath - it reads as the word.
    /// </summary>
    private static bool IsRightEye(XElement node) {
        string? flag = node.Element("rightEye")?.Value;
        if (flag != null) return bool.Parse(flag);

        return node.ToString().Contains("right", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void AnEyeWithNoNodeOfItsOwnLeavesTheKandraHalfBlind() {
        XElement[] nodes = EyeRenderNodes();

        Assert.AreEqual(
            4,
            nodes.Length,
            "The true-body gene does not declare a cutout and a bloom for each eye. A missing entry "
            + "means a kandra with one eye and no error anywhere."
        );

        foreach (string nodeClass in new[] { EyeNodeClass, GlowNodeClass }) {
            XElement[] pair = NodesFor(nodeClass);

            Assert.AreEqual(
                1,
                pair.Count(IsRightEye),
                $"{nodeClass} is not declared once per side. One entry has to say it is the right eye "
                + "and the other has to leave it alone, or both nodes draw the same eye."
            );
        }
    }

    [TestMethod]
    public void EyesDrawnWithoutTheFormlessVetoShowThroughAStolenFace() {
        foreach (XElement node in EyeRenderNodes()) {
            StringAssert.Contains(
                node.ToString(),
                FormlessSubWorker,
                $"An eye entry has no {FormlessSubWorker}. Without it a kandra wearing a stolen face "
                + "shows its own eyes through that face, which is the one rule Ka set for this feature."
            );
        }
    }

    [TestMethod]
    public void APlainCutoutShaderDropsTheIrisOutOfTheColourItWasGiven() {
        foreach (XElement node in NodesFor(EyeNodeClass)) {
            Assert.AreNotEqual(
                "Cutout",
                node.Element("shaderTypeDef")?.Value,
                "An eye node is declared with a plain Cutout shader. The eye art is drawn to be tinted, "
                + "and plain Cutout ignores the colour it is handed, with no error anywhere."
            );
        }
    }

    [TestMethod]
    public void AMaskLeftBehindSplitsOneEyeIntoTwoAgain() {
        string[] masks = Directory.EnumerateFiles(EyeTextureRoot, "*.png")
            .Where(file => Path.GetFileName(file).EndsWith("m.png", StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file)!)
            .ToArray();

        Assert.AreEqual(
            0,
            masks.Length,
            "Mask files are still on disk: " + string.Join(", ", masks) + ". A mask split one texture "
            + "into two eyes, and one texture now holds one eye."
        );

        string source = EyeNodeSource();

        Assert.IsFalse(
            source.Contains("maskPath", StringComparison.Ordinal),
            "The eye node still passes a maskPath. One texture now holds one eye, so there is no second "
            + "channel for a mask to select."
        );

        Assert.IsFalse(
            source.Contains("colorTwo", StringComparison.Ordinal),
            "The eye node still passes a colorTwo. That was the right eye's stone riding on the left "
            + "eye's graphic, and each eye now carries its own."
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
        Assert.IsFalse(
            EyeNodeSource().Contains("ContentFinder", StringComparison.Ordinal),
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
    public void AnIrisSizeInThePaletteWithNoArtBehindItRendersAKandraWithNoEyes() {
        List<string> missing = [];

        foreach (string sizeName in IrisSizeNames()) {
            foreach (string gender in Genders) {
                foreach (string side in Sides) {
                    foreach (string direction in Directions) {
                        string file = EyeTextureFile(gender, TitleCase(sizeName), side, direction);
                        if (!File.Exists(file)) missing.Add(Path.GetFileName(file)!);
                    }
                }
            }
        }

        Assert.AreEqual(
            0,
            missing.Count,
            "Missing eye art: " + string.Join(", ", missing) + ". Graphic_Multi reuses _south rotated "
            + "180 degrees when _north is missing, which draws eyes on the back of the head, and a size "
            + "the palette offers with no art behind it renders that kandra with no eyes at all."
        );
    }

    /// <summary>The columns an eye texture actually paints, or null when the file is blank.</summary>
    private static (int min, int max)? IrisColumns(string file) {
        (int width, byte[] pixels, int channels) = PngAlpha(file);

        int min = int.MaxValue, max = -1;
        for (int i = 0; i < pixels.Length; i += channels) {
            if (pixels[i + channels - 1] == 0) continue;

            int x = i / channels % width;
            if (x < min) min = x;
            if (x > max) max = x;
        }

        return max < 0 ? null : (min, max);
    }

    /// <summary>A PNG decoded far enough to see which pixels are painted. 8-bit, non-interlaced.</summary>
    private static (int width, byte[] pixels, int channels) PngAlpha(string file) {
        byte[] bytes = File.ReadAllBytes(file);
        int width = 0, height = 0, channels = 0, pos = 8;
        using MemoryStream idat = new MemoryStream();

        while (pos < bytes.Length) {
            int length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(pos));
            string type = Encoding.ASCII.GetString(bytes, pos + 4, 4);

            if (type == "IHDR") {
                width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(pos + 8));
                height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(pos + 12));
                Assert.AreEqual(8, bytes[pos + 16], $"{Path.GetFileName(file)} is not 8 bits per channel.");
                channels = bytes[pos + 17] switch { 0 => 1, 2 => 3, 4 => 2, 6 => 4, _ => 0 };
                Assert.AreNotEqual(0, channels, $"{Path.GetFileName(file)} is palettised, which this reader does not decode.");
                Assert.AreEqual(0, bytes[pos + 20], $"{Path.GetFileName(file)} is interlaced.");
            } else if (type == "IDAT") {
                idat.Write(bytes, pos + 8, length);
            }

            pos += 12 + length;
        }

        idat.Position = 0;
        using ZLibStream inflate = new ZLibStream(idat, CompressionMode.Decompress);
        using MemoryStream raw = new MemoryStream();
        inflate.CopyTo(raw);

        return (width, Unfilter(raw.ToArray(), width, height, channels), channels);
    }

    /// <summary>Undoes the per-row PNG filters, which is the whole of PNG decoding that matters here.</summary>
    private static byte[] Unfilter(byte[] raw, int width, int height, int channels) {
        int stride = width * channels;
        byte[] output = new byte[stride * height];

        for (int y = 0; y < height; y++) {
            int filter = raw[y * (stride + 1)];
            int src = y * (stride + 1) + 1;
            int dst = y * stride;

            for (int x = 0; x < stride; x++) {
                int a = x >= channels ? output[dst + x - channels] : 0;
                int b = y > 0 ? output[dst - stride + x] : 0;
                int c = y > 0 && x >= channels ? output[dst - stride + x - channels] : 0;

                int add = filter switch {
                    1 => a,
                    2 => b,
                    3 => (a + b) / 2,
                    4 => Paeth(a, b, c),
                    _ => 0,
                };

                output[dst + x] = (byte)(raw[src + x] + add);
            }
        }

        return output;
    }

    private static int Paeth(int a, int b, int c) {
        int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);

        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    [TestMethod]
    public void AWestIrisLeftOnTheFlippedWestQuadDrawsOnTheBackOfTheHead() {
        string source = EyeNodeSource();

        StringAssert.Contains(
            source,
            "GetMesh",
            "The eye node takes the head set's west quad as it comes. That quad is UV-flipped, because "
            + "vanilla heads ship no _west and rely on the flip to mirror their _east art. These eyes "
            + "do ship _west, already drawn mirrored, so the flip mirrors the iris a second time and "
            + "lands it on the far side of the face."
        );

        StringAssert.Contains(
            source,
            "Rot4.East",
            "Nothing in the eye node redirects the west facing onto the unflipped east quad, which is "
            + "the one mesh that leaves the pre-mirrored west art where it was drawn."
        );

        foreach (string sizeName in IrisSizeNames()) {
            foreach (string gender in Genders) {
                foreach (string side in Sides) {
                    string size = TitleCase(sizeName);
                    string facingAway = side == "Left" ? "Right" : "Left";

                    (int width, _, _) = PngAlpha(EyeTextureFile(gender, size, side, "east"));
                    (int min, int max)? east = IrisColumns(EyeTextureFile(gender, size, side, "east"));
                    (int min, int max)? west = IrisColumns(EyeTextureFile(gender, size, facingAway, "west"));

                    (int min, int max)? mirrored = east == null
                        ? null
                        : (width - 1 - east.Value.max, width - 1 - east.Value.min);

                    Assert.AreEqual(
                        mirrored,
                        west,
                        $"Kandra_Eyes_{gender}_{size}_{facingAway}_west.png paints columns "
                        + $"{west?.ToString() ?? "none"}, but the west head is the east head mirrored, so the "
                        + $"iris belongs at {mirrored?.ToString() ?? "none"}. The art and the mesh choice have "
                        + "to agree: pre-mirrored west art drawn on the unflipped east quad."
                    );
                }
            }
        }
    }

    [TestMethod]
    public void ABloomPinnedToTheLeftEyesColourPutsTheWrongLightOverTheRightEye() {
        string glow = ClassBody(GlowNodeClass);

        Assert.IsFalse(
            glow.Contains("eyeColourName", StringComparison.Ordinal),
            "The bloom reads the form's eye colour itself instead of taking whichever eye its own node "
            + "draws. The bloom used to be one colour over both eyes because the mask could not reach "
            + "it; that compromise is what this phase removes."
        );

        Assert.IsFalse(
            glow.Contains("eyeColourTwoName", StringComparison.Ordinal),
            "The bloom picks between the two eye colours itself. Its node already knows which eye it "
            + "is, so a second copy of that choice is one more place for the two to disagree."
        );
    }
}
