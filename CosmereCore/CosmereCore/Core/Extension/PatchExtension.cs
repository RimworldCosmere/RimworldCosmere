using System;
using System.Reflection;
using System.Xml;
using Verse;

namespace Cosmere.Core.Extension;

public static class PatchExtension {
    public static PatchOperation ReplaceTokens(this PatchOperation operation, string token, string value) {
        FieldInfo? valueProp = FindField(operation.GetType(), "value");
        XmlContainer xmlContainer = (XmlContainer)valueProp!.GetValue(operation);

        xmlContainer.node.ReplaceInNode(token, value);
        valueProp.SetValue(operation, xmlContainer);

        return operation;
    }

    private static FieldInfo? FindField(Type type, string name) {
        for (Type? current = type; current != null; current = current.BaseType) {
            FieldInfo? field = current.GetField(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
            );
            if (field != null) return field;
        }

        return null;
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
            child.ReplaceInNode(token, value);
        }
    }
}
