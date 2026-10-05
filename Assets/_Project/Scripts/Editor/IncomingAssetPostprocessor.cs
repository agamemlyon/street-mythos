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
            if (name.StartsWith("enemy_")) return 15000;
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

        public static int CountTriangles(GameObject root) =>
            root.GetComponentsInChildren<MeshFilter>(true).Where(m => m.sharedMesh != null).Sum(m => (int)TriCount(m.sharedMesh))
          + root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(m => m.sharedMesh != null).Sum(m => (int)TriCount(m.sharedMesh));

        static long TriCount(Mesh mesh)
        {
            long n = 0;
            for (int i = 0; i < mesh.subMeshCount; i++) n += mesh.GetIndexCount(i) / 3;
            return n;
        }

        static void CheckBudget(string path, GameObject root)
        {
            int tris = CountTriangles(root);
            int budget = BudgetFor(path);
            if (tris > budget)
                Debug.LogError($"[Import] HORS BUDGET {path} : {tris} triangles pour {budget}");
            else
                Debug.Log($"[Import] OK {path} : {tris}/{budget} triangles");
        }

        // FBX : les matériaux sont modifiables à l'import, on y pose directement le shader toon
        void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(Incoming)) return;
            CheckBudget(assetPath, root);
            var shader = Shader.Find(ToonShader);
            if (shader == null) return;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials.Where(m => m != null))
                    ToonConverter.CopyToToon(m, m, shader);
        }

        // GLB et VRM passent par les importeurs d'UniVRM : contrôle du budget après import,
        // le shader toon est posé à l'instanciation par ToonConverter
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var path in imported.Where(p => p.StartsWith(Incoming) && (p.EndsWith(".glb") || p.EndsWith(".vrm"))))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root != null) CheckBudget(path, root);
            }
        }
    }
}
