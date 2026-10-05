using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Filme l'équipe qui marche, image par image, sans jouer la scène :
    // -executeMethod StreetMythos.Editor.WalkFilm.Run → Builds/walk/frame_000.png…
    public static class WalkFilm
    {
        static readonly (string file, float x, float height)[] Team =
        {
            ("hero_ines_walk", -1.3f, 1.65f), ("hero_yanis_walk", 0f, 1.75f), ("hero_momo_walk", 1.4f, 1.85f),
        };

        sealed class Walker
        {
            public GameObject Root;
            public AnimationClip Clip;
            public (SkinnedMeshRenderer s, GameObject p)[] Proxies;
            public float Scale = -1f, X;
        }

        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/J0_Test.unity");
            foreach (var heroName in new[] { "Ines", "Yanis", "Momo" }) { var g = GameObject.Find(heroName); if (g) Object.DestroyImmediate(g); }

            var walkers = Team.Select(t => Load(t.file, t.x)).Where(w => w != null).ToList();
            if (walkers.Count == 0) { Debug.LogError("[Walk] aucun héros animé"); return; }

            var key = new GameObject("Key", typeof(Light)).GetComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.2f;
            key.transform.rotation = Quaternion.Euler(25f, 20f, 0);

            var cam = Camera.main;
            Directory.CreateDirectory("Builds/walk");
            foreach (var f in Directory.GetFiles("Builds/walk")) File.Delete(f);

            const int fps = 30;
            int frames = Mathf.CeilToInt(walkers.Max(w => w.Clip.length) * fps);
            int n = 0;
            // Un cycle de face, un de trois quarts, un de profil
            foreach (var camPos in new[] { new Vector3(0, 1.2f, -4.4f), new Vector3(-3.0f, 1.3f, -3.4f), new Vector3(-3.1f, 1.0f, 0f) })
                for (int i = 0; i < frames; i++)
                {
                    foreach (var w in walkers) Pose(w, (i / (float)fps) % w.Clip.length, Team.First(t => w.Root.name == t.file).height);
                    cam.transform.position = camPos;
                    cam.transform.LookAt(new Vector3(0, 0.95f, 0));
                    Shoot(cam, $"Builds/walk/frame_{n++:000}.png");
                }
            Debug.Log($"[Walk] {n} images, {walkers.Count} héros");
        }

        static Walker Load(string file, float x)
        {
            string path = $"Assets/_Project/Art/Incoming/{file}.glb";
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().OrderByDescending(c => c.length).FirstOrDefault();
            if (src == null || clip == null) { Debug.LogWarning($"[Walk] {file} absent"); return null; }

            var root = (GameObject)PrefabUtility.InstantiatePrefab(src);
            root.name = file;
            ToonConverter.ConvertInstance(root);
            // Hors mode jeu, la peau ne suit pas les os au rendu : on « cuit » le maillage à chaque image
            var proxies = root.GetComponentsInChildren<SkinnedMeshRenderer>().Select(s =>
            {
                var p = new GameObject(s.name + "_baked", typeof(MeshFilter), typeof(MeshRenderer));
                p.GetComponent<MeshRenderer>().sharedMaterials = s.sharedMaterials;
                p.GetComponent<MeshFilter>().sharedMesh = new Mesh();
                s.enabled = false;
                return (s, p);
            }).ToArray();
            return new Walker { Root = root, Clip = clip, Proxies = proxies, X = x };
        }

        static void Pose(Walker w, float t, float height)
        {
            w.Clip.SampleAnimation(w.Root, t);
            foreach (var (s, p) in w.Proxies)
            {
                var mesh = p.GetComponent<MeshFilter>().sharedMesh;
                s.BakeMesh(mesh, true);
                mesh.RecalculateBounds();
                if (w.Scale < 0) w.Scale = height / mesh.bounds.size.y; // échelle fixée sur la première image
                p.transform.rotation = s.transform.rotation;
                p.transform.localScale = Vector3.one * w.Scale;
                p.transform.position = Vector3.zero;
                var b = p.GetComponent<Renderer>().bounds;
                p.transform.position = new Vector3(w.X - b.center.x, -b.min.y, -b.center.z);
            }
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
