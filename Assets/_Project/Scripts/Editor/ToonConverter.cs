using System.IO;
using UnityEditor;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Remplace les matériaux d'un objet par des matériaux toon (un .mat par matériau source),
    // en gardant texture et couleur de base
    public static class ToonConverter
    {
        const string MaterialsDir = "Assets/_Project/Art/Props/Materials";

        public static void ConvertInstance(GameObject instance)
        {
            var shader = Shader.Find("StreetMythos/Toon");
            if (shader == null) return;
            if (!AssetDatabase.IsValidFolder(MaterialsDir)) AssetDatabase.CreateFolder("Assets/_Project/Art/Props", "Materials");

            foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null || mats[i].shader == shader) continue;
                    string path = $"{MaterialsDir}/{Sanitize(instance.name)}_{Sanitize(mats[i].name)}_toon.mat";
                    var toon = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (toon == null)
                    {
                        toon = new Material(shader);
                        AssetDatabase.CreateAsset(toon, path);
                    }
                    CopyToToon(mats[i], toon, shader);
                    // Les textures embarquées dans les GLB échappent au contrôle d'import : on les réduit ici
                    var tex = toon.GetTexture("_BaseMap") as Texture2D;
                    if (tex != null && !AssetDatabase.GetAssetPath(tex).EndsWith(".png"))
                        toon.SetTexture("_BaseMap", ExtractTexture(tex, path.Replace("_toon.mat", "_base.png"), MaxSizeFor(instance.name)));
                    EditorUtility.SetDirty(toon);
                    mats[i] = toon;
                }
                r.sharedMaterials = mats;
            }
        }

        public static void CopyToToon(Material source, Material target, Shader shader)
        {
            var tex = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap")
                    : source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
            var color = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor")
                      : source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white;
            target.shader = shader;
            if (tex != null) target.SetTexture("_BaseMap", tex);
            target.SetColor("_BaseColor", color);
        }

        // Budgets de TECH_DESIGN 3 : héros et PNJ en 1024, accessoires en 512
        static int MaxSizeFor(string name)
        {
            name = name.ToLowerInvariant();
            if (name.StartsWith("prop_")) return 512;
            return 1024;
        }

        // Copie la texture en PNG à la taille du budget, importée en DXT Crunch
        static Texture2D ExtractTexture(Texture2D source, string pngPath, int maxSize)
        {
            if (!System.IO.File.Exists(pngPath))
            {
                int w = Mathf.Min(source.width, maxSize), h = Mathf.Min(source.height, maxSize);
                var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                Graphics.Blit(source, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var copy = new Texture2D(w, h, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                copy.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                System.IO.File.WriteAllBytes(pngPath, copy.EncodeToPNG());
                Object.DestroyImmediate(copy);
                AssetDatabase.ImportAsset(pngPath);
            }
            var ti = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            if (ti.maxTextureSize != maxSize || !ti.crunchedCompression)
            {
                ti.maxTextureSize = maxSize;
                ti.crunchedCompression = true;
                ti.compressionQuality = 50;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
        }

        static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s.Replace(' ', '_').Replace("(Instance)", "");
        }
    }
}
