using System.Text;
using UnityEditor;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Écrit la hiérarchie des os d'un personnage importé, pour brancher poses et animations
    public static class RigInspector
    {
        public static void DumpYanis()
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Incoming/hero_yanis.glb");
            var sb = new StringBuilder();
            Walk(go.transform, 0, sb);
            foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                sb.AppendLine($"SMR {smr.name} : {smr.bones.Length} os, racine {smr.rootBone?.name}");
            foreach (var a in go.GetComponentsInChildren<Animation>(true)) sb.AppendLine($"Animation {a.name}");
            foreach (var clip in AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Incoming/hero_yanis.glb"))
                if (clip is AnimationClip c) sb.AppendLine($"Clip {c.name} {c.length:0.00}s");
            System.IO.File.WriteAllText("Builds/rig_yanis.txt", sb.ToString());
        }

        static void Walk(Transform t, int depth, StringBuilder sb)
        {
            sb.Append(' ', depth * 2).AppendLine(t.name);
            foreach (Transform c in t) Walk(c, depth + 1, sb);
        }
    }
}
