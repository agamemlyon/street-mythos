using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Contrôle à l'import (TECH_DESIGN 5.2) : tout modèle entré par Art/Incoming reçoit le shader toon
    // et est signalé s'il dépasse le budget de triangles de sa catégorie (préfixe du nom de fichier).
    public sealed class IncomingAssetPostprocessor : AssetPostprocessor
    {
        const string Incoming = "Assets/_Project/Art/Incoming/";
        const string ToonShader = "StreetMythos/Toon";

        public static int BudgetFor(string path)
        {
            string name = System.IO.Path.GetFileName(path).ToLowerInvariant();
            if (name.StartsWith("hero_")) return 30000;
            if (name.StartsWith("npc_")) return 15000;
            if (name.StartsWith("prop_")) return 3000;
            if (name.StartsWith("boss_")) return 40000;
            return 10000;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Incoming)) return;
            var ti = (TextureImporter)assetImporter;
            ti.maxTextureSize = 2048;
            ti.crunchedCompression = true;
            ti.compressionQuality = 50;
        }

        void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(Incoming)) return;

            int tris = root.GetComponentsInChildren<MeshFilter>(true).Where(m => m.sharedMesh != null)
                           .Sum(m => m.sharedMesh.triangles.Length / 3)
                     + root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(m => m.sharedMesh != null)
                           .Sum(m => m.sharedMesh.triangles.Length / 3);
            int budget = BudgetFor(assetPath);
            if (tris > budget)
                Debug.LogError($"[Import] HORS BUDGET {assetPath} : {tris} triangles pour {budget}");
            else
                Debug.Log($"[Import] OK {assetPath} : {tris}/{budget} triangles");

            var shader = Shader.Find(ToonShader);
            if (shader == null) return;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials.Where(m => m != null))
                {
                    var tex = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : m.mainTexture;
                    var color = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white;
                    m.shader = shader;
                    if (tex != null) m.SetTexture("_BaseMap", tex);
                    m.SetColor("_BaseColor", color);
                }
        }
    }
}
