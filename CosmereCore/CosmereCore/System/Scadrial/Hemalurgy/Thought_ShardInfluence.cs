using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

/// <summary>
///     The voice in a spiked pawn's head, named for whichever Shard is actually behind it.
/// </summary>
/// <remarks>
///     Thought stage labels are static XML, so the def writes {SHARD} and this fills it in when
///     the player looks. Post-Catacendre a spiked colonist should be hearing Harmony rather than
///     a Shard that stopped existing three hundred years ago.
/// </remarks>
public class Thought_ShardInfluence : Thought_Situational {
    private const string Placeholder = "{SHARD}";

    public override string LabelCap => Fill(base.LabelCap);

    public override string Description => Fill(base.Description);

    private static string Fill(string text) {
        return string.IsNullOrEmpty(text) || !text.Contains(Placeholder)
            ? text
            : text.Replace(Placeholder, HemalurgicShard.Name);
    }
}
