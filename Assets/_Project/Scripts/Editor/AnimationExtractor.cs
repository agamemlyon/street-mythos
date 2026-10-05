using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Les clips de combat arrivent dans des GLB complets (maillage compris, ~6 Mo chacun).
    // On n'en garde que l'animation, en .anim, puis les GLB peuvent quitter le projet.
    public static class AnimationExtractor
    {
        const string Dir = "Assets/_Project/Art/Animations";

        [MenuItem("Street Mythos/Extraire les clips de combat")]
        public static void ExtractAll()
        {
            foreach (var glb in Directory.GetFiles(Dir, "anim_*.glb"))
            {
                string path = glb.Replace('\\', '/');
                var clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                                        .OrderByDescending(c => c.length).FirstOrDefault();
                if (clip == null) { Debug.LogWarning($"[Anim] aucun clip dans {path}"); continue; }
                var copy = Object.Instantiate(clip);
                copy.legacy = true;
                string name = Path.GetFileNameWithoutExtension(path).Substring("anim_".Length); // yanis_stance
                copy.name = name;
                string target = $"{Dir}/{name}.anim";
                AssetDatabase.DeleteAsset(target);
                AssetDatabase.CreateAsset(copy, target);
                Debug.Log($"[Anim] {name} : {copy.length:0.00} s, {AnimationUtility.GetCurveBindings(copy).Length} courbes");
            }
            AssetDatabase.SaveAssets();
        }

        // Allège les clips (une clé sur deux, soit 15 images par seconde au lieu de 30) pour tenir
        // le budget de démarrage ; marqué dans userData pour ne le faire qu'une fois
        [MenuItem("Street Mythos/Alléger les clips de combat")]
        public static void ReduceAll()
        {
            foreach (var file in Directory.GetFiles(Dir, "*.anim"))
            {
                string path = file.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(path);
                if (importer != null && importer.userData == "reduit") continue;
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                foreach (var b in AnimationUtility.GetCurveBindings(clip))
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, b);
                    if (curve == null || curve.length <= 4) continue;
                    var keys = curve.keys.Where((k, i) => i % 2 == 0 || i == curve.length - 1).ToArray();
                    AnimationUtility.SetEditorCurve(clip, b, new AnimationCurve(keys));
                }
                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                importer = AssetImporter.GetAtPath(path);
                if (importer != null) { importer.userData = "reduit"; importer.SaveAndReimport(); }
                Debug.Log($"[Anim] {Path.GetFileName(path)} allégé");
            }
        }

        // Ajoute à l'Animation d'un personnage les clips extraits « <qui>_<action> »
        public static void AddClips(GameObject instance, string who)
        {
            var anim = instance.GetComponentInChildren<Animation>();
            if (anim == null) return;
            foreach (var action in new[] { "stance", "attaque", "esquive", "parade" })
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{Dir}/{who}_{action}.anim");
                if (clip != null) anim.AddClip(clip, action);
            }
        }
    }
}
