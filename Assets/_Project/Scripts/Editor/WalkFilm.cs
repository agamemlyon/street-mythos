using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Filme le cycle de marche de Yanis image par image (sans jouer la scène) :
    // -executeMethod StreetMythos.Editor.WalkFilm.Run  → Builds/walk/frame_000.png…
    public static class WalkFilm
    {
        const string WalkPath = "Assets/_Project/Art/Incoming/hero_yanis_walk.glb";

        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/J0_Test.unity");
            var old = GameObject.Find("Yanis");
            if (old != null) Object.DestroyImmediate(old);

            var src = AssetDatabase.LoadAssetAtPath<GameObject>(WalkPath);
            var clip = AssetDatabase.LoadAllAssetsAtPath(WalkPath).OfType<AnimationClip>()
                                    .OrderByDescending(c => c.length).FirstOrDefault();
            if (src == null || clip == null) { Debug.LogError("[Walk] modèle ou clip absent"); return; }
            Debug.Log($"[Walk] clip {clip.name} {clip.length:0.00}s legacy={clip.legacy}");

            var hero = (GameObject)PrefabUtility.InstantiatePrefab(src);
            hero.name = "Yanis";
            ToonConverter.ConvertInstance(hero);

            // Diagnostic : chemins animés et mouvement réel d'un os
            var bindings = AnimationUtility.GetCurveBindings(clip);
            Debug.Log($"[Walk] {bindings.Length} courbes, ex. : {string.Join(" | ", bindings.Take(4).Select(x => x.path + "." + x.propertyName))}");
            var leg = hero.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "LeftUpLeg");
            var anim = hero.GetComponentInChildren<Animation>();
            Debug.Log($"[Walk] Animation sur {(anim ? anim.gameObject.name : "rien")}, racine {hero.name}");
            var host = anim ? anim.gameObject : hero;
            clip.SampleAnimation(host, 0f); var r0 = leg.localRotation;
            clip.SampleAnimation(host, 1f); var r1 = leg.localRotation;
            Debug.Log($"[Walk] LeftUpLeg t0 {r0.eulerAngles} t1 {r1.eulerAngles}");
            hero = host;

            var key = new GameObject("Key", typeof(Light)).GetComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.2f;
            key.transform.rotation = Quaternion.Euler(25f, 20f, 0);

            var cam = Camera.main;
            Directory.CreateDirectory("Builds/walk");
            foreach (var f in Directory.GetFiles("Builds/walk")) File.Delete(f);

            // Deux cycles : un de trois quarts, un de profil
            // Hors mode jeu, la peau ne suit pas les os au rendu : on « cuit » le maillage à chaque image
            var smrs = hero.GetComponentsInChildren<SkinnedMeshRenderer>();
            var proxies = smrs.Select(s =>
            {
                var p = new GameObject(s.name + "_baked", typeof(MeshFilter), typeof(MeshRenderer));
                p.GetComponent<MeshRenderer>().sharedMaterials = s.sharedMaterials;
                p.GetComponent<MeshFilter>().sharedMesh = new Mesh();
                s.enabled = false;
                return (s, p);
            }).ToArray();

            float scale = -1f;
            const int fps = 30;
            int frames = Mathf.CeilToInt(clip.length * fps);
            int n = 0;
            foreach (var camOffset in new[] { new Vector3(-2.2f, 1.2f, -2.6f), new Vector3(-3.2f, 1.0f, 0f) })
                for (int i = 0; i < frames; i++)
                {
                    clip.SampleAnimation(hero, i / (float)fps);
                    foreach (var (s, p) in proxies)
                    {
                        var mesh = p.GetComponent<MeshFilter>().sharedMesh;
                        s.BakeMesh(mesh, true);
                        mesh.RecalculateBounds();
                        // Échelle fixée une fois pour toutes sur la première image : 1,75 m de haut
                        if (scale < 0) scale = 1.75f / mesh.bounds.size.y;
                        p.transform.rotation = s.transform.rotation;
                        p.transform.localScale = Vector3.one * scale;
                        p.transform.position = Vector3.zero;
                        p.transform.position = new Vector3(0, -p.GetComponent<Renderer>().bounds.min.y, 0); // pieds au sol
                    }
                    var b = proxies.Select(x => x.p.GetComponent<Renderer>().bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
                    cam.transform.position = new Vector3(b.center.x, 0, b.center.z) + camOffset;
                    cam.transform.LookAt(new Vector3(b.center.x, 0.9f, b.center.z));
                    Shoot(cam, $"Builds/walk/frame_{n++:000}.png");
                }
            Debug.Log($"[Walk] {n} images");
        }

        static void Shoot(Camera cam, string path)
        {
            var rt = new RenderTexture(960, 540, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(tex);
            rt.Release();
        }
    }
}
