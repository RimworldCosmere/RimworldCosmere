using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace Cosmere.Lightweave.ModsConfig;

internal static class ModsConfigState {
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    public static bool HasUnsavedChanges(Page_ModsConfig page) {
        try {
            FieldInfo? hashField = typeof(Page_ModsConfig).GetField(
                "activeModsWhenOpenedHash",
                PrivateInstance
            );
            if (hashField == null) {
                return true;
            }
            int opened = (int)(hashField.GetValue(page) ?? 0);
            int current = ModLister.InstalledModsListHash(activeOnly: true);
            return opened != current;
        }
        catch (Exception ex) {
            LightweaveLog.Error("HasUnsavedChanges reflection failed: " + ex);
            return true;
        }
    }

    public static bool GetSaveChanges(Page_ModsConfig page) {
        return GetPrivateBool(page, "saveChanges");
    }

    public static bool GetDiscardChanges(Page_ModsConfig page) {
        return GetPrivateBool(page, "discardChanges");
    }

    public static void SetSaveChanges(Page_ModsConfig page, bool value) {
        SetPrivateBool(page, "saveChanges", value);
    }

    public static void SetDiscardChanges(Page_ModsConfig page, bool value) {
        SetPrivateBool(page, "discardChanges", value);
    }

    private static bool GetPrivateBool(Page_ModsConfig page, string fieldName) {
        try {
            FieldInfo? field = typeof(Page_ModsConfig).GetField(fieldName, PrivateInstance);
            if (field == null) {
                LightweaveLog.Error("Page_ModsConfig." + fieldName + " field not found via reflection.");
                return false;
            }
            return (bool)(field.GetValue(page) ?? false);
        }
        catch (Exception ex) {
            LightweaveLog.Error("GetPrivateBool(" + fieldName + ") failed: " + ex);
            return false;
        }
    }

    private static void SetPrivateBool(Page_ModsConfig page, string fieldName, bool value) {
        try {
            FieldInfo? field = typeof(Page_ModsConfig).GetField(fieldName, PrivateInstance);
            if (field == null) {
                LightweaveLog.Error("Page_ModsConfig." + fieldName + " field not found via reflection.");
                return;
            }
            field.SetValue(page, value);
        }
        catch (Exception ex) {
            LightweaveLog.Error("SetPrivateBool(" + fieldName + ") failed: " + ex);
        }
    }
}
