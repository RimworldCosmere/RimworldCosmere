using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CosmereCore.Tabs
{
    public static class InvestitureTabUI
    {
        private static readonly Dictionary<string, bool> collapsedStates = new();

        public static bool DrawCollapsibleHeader(string label, string key, Listing_Standard listing)
        {
            if (!collapsedStates.TryGetValue(key, out var expanded))
                expanded = true;

            var headerRect = listing.GetRect(Text.LineHeight);
            Text.Anchor = TextAnchor.MiddleLeft;

            var prefix = expanded ? "▼ " : "▶ ";
            Widgets.Label(headerRect, prefix + label);

            if (Widgets.ButtonInvisible(headerRect))
            {
                expanded = !expanded;
                collapsedStates[key] = expanded;
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }

            listing.Gap(2f);
            Text.Anchor = TextAnchor.UpperLeft;

            return expanded;
        }
    }
}