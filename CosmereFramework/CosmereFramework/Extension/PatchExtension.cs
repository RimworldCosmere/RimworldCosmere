using System.Reflection;
using System.Xml;
using HarmonyLib;
using Verse;

namespace CosmereFramework.Extension;

public static class PatchExtension {
    public static PatchOperation ReplaceTokens(this PatchOperation operation, string token, string value) {
        FieldInfo? valueProp = AccessTools.Field(operation.GetType(), "value");
        XmlContainer xmlContainer = (XmlContainer)valueProp.GetValue(operation);

        xmlContainer.node.ReplaceInNode(token, value);
        valueProp.SetValue(operation, xmlContainer);

        return operation;
    }

    private static void ReplaceInNode(this XmlNode node, string token, string value) {
        if (node is XmlText or XmlCDataSection) {
            node.Value = node.Value?.Replace($"{{{token}}}", value);
        }

        if (node.Attributes != null) {
            foreach (XmlAttribute attr in node.Attributes) {
                attr.Value = attr.Value?.Replace($"{{{token}}}", value);
            }
        }

        foreach (XmlNode child in node.ChildNodes) {
            ReplaceInNode(child, token, value);
        }
    }
}