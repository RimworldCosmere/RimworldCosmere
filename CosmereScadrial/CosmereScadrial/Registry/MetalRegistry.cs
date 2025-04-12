using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Registry {
    [StaticConstructorOnStartup]
    public static class MetalRegistry {
        public static readonly Dictionary<string, MetalInfo> Metals = new Dictionary<string, MetalInfo>();

        static MetalRegistry() {
            LoadMetalRegistry();
        }

        private static void LoadMetalRegistry() {
            var mod = LoadedModManager.RunningMods.FirstOrDefault(m =>
                m.PackageId.Contains("cosmere.scadrial")); // use your actual packageId
            if (mod == null) {
                Log.Error("[CosmereScadrial] Could not find CosmereScadrial mod to load MetalRegistry.xml");
                return;
            }

            var path = Path.Combine(mod.RootDir, "Resources", "MetalRegistry.xml");
            if (!File.Exists(path)) {
                Log.Error("[CosmereScadrial] MetalRegistry.xml not found at: " + path);
                return;
            }

            try {
                var doc = new XmlDocument();
                doc.Load(path);

                foreach (XmlNode metalNode in doc.SelectNodes("//metals/li")) {
                    var parsedColor = ParseColor(metalNode["color"]);
                    var metal = new MetalInfo {
                        Name = metalNode["name"]?.InnerText ?? "",
                        DefName = metalNode["defName"]?.InnerText,
                        GodMetal = bool.TryParse(metalNode["godMetal"]?.InnerText, out var gm) && gm,
                        Color = new Color(parsedColor[0] / 255f, parsedColor[1] / 255f, parsedColor[2] / 255f),
                        Allomancy = ParseAllomancyInfo(metalNode["allomancy"]),
                        Feruchemy = ParseFeruchemyInfo(metalNode["feruchemy"]),
                    };

                    if (!string.IsNullOrEmpty(metal.Name)) Metals[metal.Name] = metal;
                }

                Log.Message($"[CosmereScadrial] Loaded {Metals.Count} metals from MetalRegistry.xml.");
            }
            catch (Exception ex) {
                Log.Error("[CosmereScadrial] Error loading MetalRegistry.xml: " + ex);
            }
        }

        private static List<int> ParseColor(XmlNode node) => node?.InnerText.Replace(")", "").Replace("(", "").Split(',').Select(int.Parse).ToList();

        private static MetalAllomancyInfo ParseAllomancyInfo(XmlNode node) {
            if (node == null) return null;
            return new MetalAllomancyInfo {
                UserName = node["userName"]?.InnerText,
                Description = node["description"]?.InnerText,
                Group = node["group"]?.InnerText,
                Axis = node["axis"]?.InnerText,
                Polarity = node["polarity"]?.InnerText,
            };
        }

        private static MetalFeruchemyInfo ParseFeruchemyInfo(XmlNode node) {
            if (node == null) return null;
            return new MetalFeruchemyInfo {
                UserName = node["userName"]?.InnerText,
                Description = node["description"]?.InnerText,
                Group = node["group"]?.InnerText,
            };
        }
    }
}