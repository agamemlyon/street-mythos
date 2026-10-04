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

        static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s.Replace(' ', '_').Replace("(Instance)", "");
        }
    }
}
