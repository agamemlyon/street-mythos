using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using StreetMythos.BattleView;
using StreetMythos.Core;

namespace StreetMythos.Editor
{
    // Génère l'arène de combat du quartier 1 et les prefabs des héros (toon, à la bonne taille)
    public static class ArenaBuilder
    {
        const string Root = "Assets/_Project";
        const string ArenaPath = Root + "/Scenes/Arena_Q1.unity";

        [MenuItem("Street Mythos/Générer l'arène du quartier 1")]
        public static void Build()
        {
            InkBuild.CompileAll();
            AnimationExtractor.ExtractAll();
            var heroes = new[]
            {
                MakeHeroPrefab("hero_yanis_walk", "Hero_Yanis", 1.75f),
                MakeHeroPrefab("hero_ines_walk", "Hero_Ines", 1.65f),
                MakeHeroPrefab("hero_momo_walk", "Hero_Momo", 1.85f),
            };
            var enemies = new[]
            {
                ("pigeon", MakeEnemyPrefab("enemy_pigeon", "Ennemi_Pigeon", 0.7f), EnemyMotion.Style.Pigeon),
                ("lion", MakeEnemyPrefab("enemy_lion", "Ennemi_Lion", 1.5f), EnemyMotion.Style.Stone),
                ("controleur", MakeEnemyPrefab("enemy_controleur", "Ennemi_Controleur", 1.8f), EnemyMotion.Style.Ghost),
                ("gros_caillou", MakeEnemyPrefab("boss_gros_caillou", "Boss_Gros_Caillou", 3.2f), EnemyMotion.Style.Golem),
            }.Where(e => e.Item2 != null).ToArray();
            var panel = MakePanelSettings();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var toon = Shader.Find("StreetMythos/Toon");

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.15f, 0.3f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.56f, 0.64f, 0.78f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 12f;
            RenderSettings.fogEndDistance = 40f;

            var cam = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.239f, 0.173f, 0.42f);
            cam.fieldOfView = 40f;
            cam.transform.position = new Vector3(-0.6f, 3.8f, -12f);
            cam.transform.LookAt(new Vector3(0f, 0.8f, 0.4f));
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cam.gameObject.AddComponent<PerfOverlay>();

            var key = new GameObject("Lune", typeof(Light)).GetComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(0.65f, 0.7f, 1f);
            key.intensity = 0.9f;
            key.shadows = LightShadows.Soft;
            key.transform.rotation = Quaternion.Euler(45f, -60f, 0); // vient de l'avant-droite : éclaire le visage des héros, ombres vers le fond

            foreach (var x in new[] { -6f, 6f })
            {
                var l = new GameObject("Sodium", typeof(Light)).GetComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1f, 0.70f, 0.36f);
                l.range = 9f;
                l.intensity = 2.5f;
                l.transform.position = new Vector3(x, 3.5f, 2f);
            }

            // Sol de pavés (disque) et fond de façades simplifié
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Place";
            ground.transform.localScale = new Vector3(18f, 0.05f, 12f);
            ground.transform.position = new Vector3(0, -0.05f, 0);
            Object.DestroyImmediate(ground.GetComponent<Collider>());
            ground.GetComponent<Renderer>().sharedMaterial = Mat(toon, "M_Arene_Sol", new Color(0.32f, 0.29f, 0.4f));
            for (int i = 0; i < 7; i++)
            {
                var f = GameObject.CreatePrimitive(PrimitiveType.Cube);
                f.name = $"Facade_{i}";
                float h = 5f + (i % 3) * 1.5f;
                f.transform.localScale = new Vector3(3.2f, h, 1f);
                f.transform.position = new Vector3(-10f + i * 3.3f, h / 2f, 6.5f);
                Object.DestroyImmediate(f.GetComponent<Collider>());
                f.GetComponent<Renderer>().sharedMaterial = Mat(toon, $"M_Facade_{i % 3}",
                    new[] { new Color(0.55f, 0.4f, 0.45f), new Color(0.6f, 0.5f, 0.38f), new Color(0.45f, 0.42f, 0.55f) }[i % 3]);
            }

            // Emplacements : héros à gauche tournés vers la droite, ennemis en face
            var heroSlots = Slots("Heros", -3.2f, -90f, 3, 1.6f);
            var enemySlots = Slots("Ennemis", 3.2f, 90f, 4, 1.4f);

            var uiGo = new GameObject("Interface", typeof(UIDocument));
            var ui = uiGo.GetComponent<UIDocument>();
            ui.panelSettings = panel;
            ui.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Root + "/UI/Battle.uxml");

            var ctrl = new GameObject("Combat").AddComponent<BattleController>();
            ctrl.SkillsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/Data/Json/skills.json");
            ctrl.HeroesJson = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/Data/Json/heroes.json");
            ctrl.EnemiesJson = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/Data/Json/enemies.json");
            ctrl.EncountersJson = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/Data/Json/encounters.json");
            ctrl.Ui = ui;
            ctrl.HeroSlots = heroSlots;
            ctrl.EnemySlots = enemySlots;
            ctrl.HeroPrefabs = heroes;
            ctrl.ToonTemplate = Mat(toon, "M_Ennemi_Provisoire", Color.white);
            ctrl.EncounterId = "q1_tuto_pigeons";
            ctrl.EnemyPrefabIds = enemies.Select(e => e.Item1).ToArray();
            ctrl.EnemyPrefabs = enemies.Select(e => e.Item2).ToArray();
            ctrl.EnemyStyles = enemies.Select(e => e.Item3).ToArray();
            ctrl.BrumeMaterial = BrumeMat();
            ctrl.VannesInk = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/Dialogues/vannes.json");

            EditorSceneManager.SaveScene(scene, ArenaPath);

            // Le build de test du J1 démarre directement sur l'arène
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ArenaPath).ToList();
            scenes.Add(new EditorBuildSettingsScene(ArenaPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            SetBootTarget("Arena_Q1");
            AssetDatabase.SaveAssets();
            Debug.Log("[Arène] Arena_Q1 générée");
        }

        // Vérifie l'orientation des ennemis : chacun posé comme dans l'arène (tourné vers -X),
        // photographié depuis le camp des héros. On doit voir leur face.
        public static void CaptureEnemies()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.24f, 0.17f, 0.42f);
            float z = 0;
            foreach (var name in new[] { "Ennemi_Pigeon", "Ennemi_Lion", "Ennemi_Controleur", "Boss_Gros_Caillou" })
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Art/Characters/{name}.prefab");
                if (p == null) continue;
                var g = (GameObject)PrefabUtility.InstantiatePrefab(p);
                g.transform.SetPositionAndRotation(new Vector3(0, 0, z), Quaternion.Euler(0, 90f, 0));
                z += 4f;
            }
            cam.transform.position = new Vector3(-9f, 2.5f, 6f);
            cam.transform.LookAt(new Vector3(0, 1f, 6f));
            var rt = new RenderTexture(1600, 600, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1600, 600, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1600, 600), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes("Builds/capture_ennemis.png", tex.EncodeToPNG());
        }

        static Transform[] Slots(string name, float x, float yaw, int count, float spacing)
        {
            var parent = new GameObject(name).transform;
            return Enumerable.Range(0, count).Select(i =>
            {
                var t = new GameObject($"{name}_{i}").transform;
                t.SetParent(parent);
                // Rangée en diagonale : chacun reste visible depuis la caméra
                float k = i - (count - 1) / 2f;
                t.position = new Vector3(x + k * spacing * 0.9f * Mathf.Sign(x), 0, k * spacing * 0.8f);
                t.rotation = Quaternion.Euler(0, yaw, 0);
                return t;
            }).ToArray();
        }

        internal static void SetBootTarget(string sceneName)
        {
            var boot = EditorSceneManager.OpenScene(Root + "/Scenes/Boot.unity", OpenSceneMode.Single);
            var b = Object.FindFirstObjectByType<Bootstrap>();
            var so = new SerializedObject(b);
            so.FindProperty("firstScene").stringValue = sceneName;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(boot);
        }

        static GameObject MakeHeroPrefab(string glb, string prefabName, float height, float yaw = 0f)
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Art/Incoming/{glb}.glb");
            if (src == null) { Debug.LogWarning($"[Arène] {glb} absent"); return null; }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
            inst.name = prefabName;
            inst.transform.rotation = Quaternion.Euler(0, yaw, 0);
            ToonConverter.ConvertInstance(inst);
            AnimationExtractor.AddClips(inst, glb.Replace("hero_", "").Replace("enemy_", "").Replace("_walk", ""));
            var b = inst.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            inst.transform.localScale *= height / b.size.y;
            // Racine neutre : la taille et le pivot au sol vivent sur l'enfant
            var root = new GameObject(prefabName);
            inst.transform.SetParent(root.transform, true);
            b = inst.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            inst.transform.position -= new Vector3(b.center.x, b.min.y, b.center.z);
            string path = $"{Root}/Art/Characters/{prefabName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // yaw : correction d'orientation des modèles générés qui ne regardent pas vers -Z
        static GameObject MakeEnemyPrefab(string glb, string prefabName, float height, float yaw = 0f) => MakeHeroPrefab(glb, prefabName, height, yaw);

        static Material BrumeMat()
        {
            string path = Root + "/Settings/M_Brume.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("StreetMythos/Brume")); AssetDatabase.CreateAsset(m, path); }
            return m;
        }

        internal static PanelSettings MakePanelSettings()
        {
            string themePath = Root + "/UI/StreetMythosTheme.tss";
            if (!System.IO.File.Exists(themePath))
            {
                System.IO.File.WriteAllText(themePath, "@import url(\"unity-theme://default\");\n");
                AssetDatabase.ImportAsset(themePath);
            }
            string path = Root + "/UI/BattlePanel.asset";
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps == null)
            {
                ps = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(ps, path);
            }
            ps.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(themePath);
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1600, 900);
            ps.match = 0.5f;
            EditorUtility.SetDirty(ps);
            return ps;
        }

        internal static Material Mat(Shader shader, string name, Color color)
        {
            string path = $"{Root}/Settings/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", color);
            return m;
        }
    }
}
