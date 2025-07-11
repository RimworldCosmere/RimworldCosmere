using UnityEngine;
using UnityEngine.Rendering;

namespace CosmereFramework.Extension;

public static class ShaderExtension {
    public static void LogShaderParameters(this Shader shader) {
        int propertyCount = shader.GetPropertyCount();
        Verse.Log.Message($"Shader: {shader.name} has {propertyCount} properties");

        for (int i = 0; i < propertyCount; i++) {
            string name = shader.GetPropertyName(i);
            ShaderPropertyType type = shader.GetPropertyType(i);
            string description = shader.GetPropertyDescription(i);
            string defaultValue = "";

            switch (type) {
                case ShaderPropertyType.Color:
                    defaultValue = shader.GetPropertyDefaultVectorValue(i).ToString("F3");
                    break;

                case ShaderPropertyType.Vector:
                    defaultValue = shader.GetPropertyDefaultVectorValue(i).ToString("F3");
                    break;

                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    defaultValue = shader.GetPropertyDefaultFloatValue(i).ToString("F3");
                    break;

                case ShaderPropertyType.Texture:
                    defaultValue = shader.GetPropertyTextureDefaultName(i) ?? "None";
                    break;
            }

            Verse.Log.Message(
                $"Property {i}: {name} type={type} description={description} default={defaultValue}"
            );
        }
    }
}