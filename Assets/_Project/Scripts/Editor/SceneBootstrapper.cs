using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using StreetMythos.Core;

namespace StreetMythos.Editor
{
    // Assemble par script le pipeline URP et les scènes du J0 (Boot, MainMenu, J0_Test),
    // pour que personne n'ait à les monter à la main.
    public static class SceneBootstrapper
    {
        const string SettingsDir = "Assets/_Project/Settings";
        const string ScenesDir = "Assets/_Project/Scenes";
        const string LampPath = "Assets/_Project/Art/Incoming/prop_lampadaire_lyon.glb";

        // Capture de la caméra de la scène test, sans build : -executeMethod StreetMythos.Editor.SceneBootstrapper.Capture
        public static void Capture()
        {
            EditorSceneManager.OpenScene($"{ScenesDir}/J0_Test.unity");
            var cam = Camera.main;
            Shoot(cam, "Builds/capture_j0.png");

            // Lumière de face pour juger les modèles, puis photo de groupe et gros plans
            var key = new GameObject("Key", typeof(Light)).GetComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.2f;
            key.transform.rotation = Quaternion.Euler(25f, 20f, 0);

            var heroes = new[] { "Ines", "Yanis", "Momo" }.Select(GameObject.Find).Where(h => h != null).ToList();
            if (heroes.Count == 0) return;
            Bounds BoundsOf(GameObject g) => g.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            var all = heroes.Select(BoundsOf).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            cam.transform.position = all.center + new Vector3(0, 0.2f, -4.6f);
            cam.transform.LookAt(all.center);
            Shoot(cam, "Builds/capture_equipe.png");

            foreach (var hero in heroes)
            {
                var b = BoundsOf(hero);
                cam.transform.position = b.center + new Vector3(0, 0.1f, -2.6f);
                cam.transform.LookAt(b.center);
                Shoot(cam, $"Builds/capture_{hero.name.ToLowerInvariant()}.png");
                cam.transform.position = b.center + new Vector3(0, 0.45f, -0.9f);
                cam.transform.LookAt(b.center + new Vector3(0, b.extents.y * 0.8f, 0));
                Shoot(cam, $"Builds/capture_{hero.name.ToLowerInvariant()}_visage.png");
            }
        }

        static void Shoot(Camera cam, string path)
        {
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            System.IO.Directory.CreateDirectory("Builds");
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            Debug.Log($"[J0] Capture écrite dans {path}");
        }

        [MenuItem("Street Mythos/Générer les scènes du J0")]
        public static void BuildAll()
        {
            ProjectConfigurator.ConfigureAll();
            AssetDatabase.ImportAsset("Assets/_Project/Art/Incoming", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            SetupUrp();
            var boot = CreateBootScene();
            var test = CreateTestScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(boot, true), new EditorBuildSettingsScene(test, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[J0] Pipeline URP et scènes générés");
        }

        static void SetupUrp()
        {
            if (!AssetDatabase.IsValidFolder(SettingsDir)) AssetDatabase.CreateFolder("Assets/_Project", "Settings");
            string rendererPath = $"{SettingsDir}/StreetMythos_Renderer.asset";
            string pipelinePath = $"{SettingsDir}/StreetMythos_URP.asset";

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            // Budgets web (TECH_DESIGN 3) : 4 lumières temps réel, ombres modestes, MSAA 2x
            pipeline.maxAdditionalLightsCount = 4;
            pipeline.shadowDistance = 40f;
            pipeline.msaaSampleCount = 2;
            pipeline.supportsHDR = true;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
        }

        static string CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("Bootstrap");
            var boot = go.AddComponent<Bootstrap>();
            var so = new SerializedObject(boot);
            so.FindProperty("firstScene").stringValue = "J0_Test";
            so.ApplyModifiedPropertiesWithoutUndo();
            string path = $"{ScenesDir}/Boot.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        // Scène de mesure : rue de nuit simplifiée, lampadaires toon, lumière sodium
        static string CreateTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var toon = Shader.Find("StreetMythos/Toon");

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.106f, 0.106f, 0.227f); // indigo #1B1B3A
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.56f, 0.64f, 0.78f);       // brume #8FA3C7
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 15f;
            RenderSettings.fogEndDistance = 60f;

            var cam = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.239f, 0.173f, 0.42f);         // violet #3D2C6B
            cam.transform.SetPositionAndRotation(new Vector3(0, 3.2f, -9f), Quaternion.Euler(12f, 0, 0));
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cam.gameObject.AddComponent<PerfOverlay>();

            var moon = new GameObject("Lune", typeof(Light)).GetComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.55f, 0.6f, 0.95f);
            moon.intensity = 0.6f;
            moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(45f, -30f, 0);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Chaussee";
            ground.transform.localScale = new Vector3(4, 1, 4);
            ground.GetComponent<Renderer>().sharedMaterial = MakeMaterial(toon, "M_Chaussee", new Color(0.3f, 0.28f, 0.38f));

            // L'équipe, héros par héros s'ils sont importés, sinon une capsule témoin
            var team = new[] { ("Ines", "hero_ines", -1.3f, 1.65f), ("Yanis", "hero_yanis", 0f, 1.75f), ("Momo", "hero_momo", 1.4f, 1.85f) };
            bool any = false;
            foreach (var (name, file, x, height) in team)
            {
                var src = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Art/Incoming/{file}.glb");
                if (src == null) continue;
                any = true;
                var hero = (GameObject)PrefabUtility.InstantiatePrefab(src);
                hero.name = name;
                hero.transform.position = new Vector3(x, 0, 0);
                ToonConverter.ConvertInstance(hero);
                FitHeight(hero, height);
            }
            if (!any)
            {
                var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                hero.name = "Temoin_Heros";
                hero.transform.position = new Vector3(0, 1, 0);
                hero.GetComponent<Renderer>().sharedMaterial = MakeMaterial(toon, "M_Temoin", new Color(1f, 0.5f, 0.2f));
            }

            var lamp = AssetDatabase.LoadAssetAtPath<GameObject>(LampPath);
            for (int i = 0; i < 6; i++)
            {
                float x = (i % 2 == 0 ? -3.5f : 3.5f);
                float z = (i / 2) * 8f;
                var pos = new Vector3(x, 0, z);
                if (lamp != null)
                {
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(lamp);
                    inst.transform.position = pos;
                    ToonConverter.ConvertInstance(inst);
                    FitHeight(inst, 4f);
                }
                var light = new GameObject($"Sodium_{i}", typeof(Light)).GetComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.70f, 0.36f);                    // sodium #FFB35C
                light.range = 7f;
                light.intensity = 3f;
                light.transform.position = pos + new Vector3(0, 3.6f, 0);
            }

            string path = $"{ScenesDir}/J0_Test.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        static void FitHeight(GameObject go, float height)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var b = renderers.Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            if (b.size.y <= 0.0001f) return;
            go.transform.localScale *= height / b.size.y;
            b = go.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            // Pieds au sol, et centre du modèle sur le point visé (le pivot des GLB générés est décalé)
            var target = go.transform.position;
            go.transform.position += new Vector3(target.x - b.center.x, -b.min.y, target.z - b.center.z);
        }

        static Material MakeMaterial(Shader shader, string name, Color color)
        {
            string path = $"{SettingsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            return mat;
        }
    }
}
