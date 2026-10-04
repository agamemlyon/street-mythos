using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace StreetMythos.Editor
{
    // Réglages projet imposés par TECH_DESIGN : sérialisation texte, WebGL 2, Brotli, gabarit maison
    public static class ProjectConfigurator
    {
        [MenuItem("Street Mythos/Appliquer les réglages projet")]
        public static void ConfigureAll()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.companyName = "Street Mythos";
            PlayerSettings.productName = "Street Mythos : Légendes du 69";
            ConfigureWeb();
            AssetDatabase.SaveAssets();
            Debug.Log("[Config] Réglages Street Mythos appliqués");
        }

        public static void ConfigureWeb()
        {
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true; // itch.io ne sert pas les .br avec Content-Encoding : le loader décompresse lui-même
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            if (AssetDatabase.IsValidFolder("Assets/WebGLTemplates/StreetMythos"))
                PlayerSettings.WebGL.template = "PROJECT:StreetMythos";
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
        }
    }
}
