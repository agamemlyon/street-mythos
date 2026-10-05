using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using StreetMythos.Core;
using StreetMythos.Exploration;

namespace StreetMythos.Editor
{
    // Maquette jouable de la Croix-Rousse (lot 6) : la place, la montée de la Grande Côte, la traboule,
    // le plateau et le Gros Caillou. Formes simples en attendant le kit de façades (lot 7).
    public static class CroixRousseBuilder
    {
        const string Root = "Assets/_Project";
        const string ScenePath = Root + "/Scenes/Q1_CroixRousse.unity";
        static Shader _toon;

        [MenuItem("Street Mythos/Générer la Croix-Rousse (maquette)")]
        public static void Build()
        {
            InkBuild.CompileAll();
            _toon = Shader.Find("StreetMythos/Toon");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.19f, 0.36f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.56f, 0.64f, 0.78f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 55f;

            var moon = new GameObject("Lune", typeof(Light)).GetComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.6f, 0.65f, 1f);
            moon.intensity = 0.8f;
            moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(50f, -40f, 0);

            var level = new GameObject("Quartier").transform;
            // La place (départ), la montée (rampe), le plateau du haut, pavés texturés (lot 7)
            Block(level, "Place", new Vector3(0, -0.25f, 10), new Vector3(16, 0.5f, 22), Tex("M_CR_Paves_Place", "tex_paves", new Vector2(8, 11)));
            var ramp = Block(level, "Montee_Grande_Cote", new Vector3(0, 2.75f, 35.5f), new Vector3(10, 0.5f, 30.6f), Tex("M_CR_Paves_Montee", "tex_paves", new Vector2(5, 15)));
            ramp.transform.rotation = Quaternion.Euler(-11.3f, 0, 0);
            Block(level, "Plateau", new Vector3(0, 5.75f, 72), new Vector3(18, 0.5f, 44), Tex("M_CR_Paves_Plateau", "tex_paves", new Vector2(9, 22)));

            // Façades des deux côtés, avec collisions
            var colors = new[] { new Color(0.62f, 0.45f, 0.42f), new Color(0.66f, 0.55f, 0.4f), new Color(0.5f, 0.47f, 0.6f), new Color(0.58f, 0.5f, 0.5f) };
            int n = 0;
            for (float z = -1; z < 94; z += 6.2f)
            {
                float ground = z < 20 ? 0 : z < 51 ? (z - 20) * 0.2f : 6f;
                float halfWidth = z < 20 ? 8.5f : z < 51 ? 6f : 9.5f;
                foreach (float side in new[] { -1f, 1f })
                {
                    float h = 7f + (n % 3) * 2f;
                    Block(level, $"Facade_{n++}", new Vector3(side * (halfWidth + 1.5f), ground + h / 2f, z + 3f), new Vector3(3f, h, 6f), Tex($"M_CR_Facade_{n % 3}", FacadeTextures[n % 3], Vector2.one));
                }
            }
            Block(level, "Mur_fond", new Vector3(0, 10, 96), new Vector3(24, 20, 2), M("M_CR_Facade_0", colors[0]));
            Block(level, "Mur_depart", new Vector3(0, 5, -2), new Vector3(18, 10, 1), M("M_CR_Facade_1", colors[1]));

            // La traboule : passage couvert en haut de la montée
            Block(level, "Traboule_toit", new Vector3(0, 9.5f, 55), new Vector3(8, 1, 6), M("M_CR_Traboule", new Color(0.35f, 0.3f, 0.38f)));
            Block(level, "Traboule_mur_g", new Vector3(-3.6f, 7.5f, 55), new Vector3(0.8f, 3.5f, 6), M("M_CR_Traboule", new Color(0.35f, 0.3f, 0.38f)));
            Block(level, "Traboule_mur_d", new Vector3(3.6f, 7.5f, 55), new Vector3(0.8f, 3.5f, 6), M("M_CR_Traboule", new Color(0.35f, 0.3f, 0.38f)));

            // Lampadaires et lumières sodium
            var lampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Art/Incoming/prop_lampadaire_lyon.glb");
            foreach (var (x, z, y) in new[] { (-6f, 6f, 0f), (6f, 14f, 0f), (-4.5f, 30f, 2f), (4.5f, 42f, 4.4f), (-7f, 64f, 6f), (7f, 78f, 6f), (-7f, 90f, 6f) })
            {
                if (lampPrefab != null)
                {
                    var lamp = (GameObject)PrefabUtility.InstantiatePrefab(lampPrefab);
                    lamp.transform.SetParent(level);
                    lamp.transform.position = new Vector3(x, y, z);
                    ToonConverter.ConvertInstance(lamp);
                    Fit(lamp, 4f);
                }
                var l = new GameObject("Sodium", typeof(Light)).GetComponent<Light>();
                l.transform.SetParent(level);
                l.type = LightType.Point;
                l.color = new Color(1f, 0.70f, 0.36f);
                l.range = 9f;
                l.intensity = 3f;
                l.transform.position = new Vector3(x * 0.85f, y + 3.6f, z);
            }

            // Le joueur (Yanis) et la caméra
            var player = new GameObject("Joueur");
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.75f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.9f, 0);
            var heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Art/Characters/Hero_Yanis.prefab");
            if (heroPrefab != null) ((GameObject)PrefabUtility.InstantiatePrefab(heroPrefab)).transform.SetParent(player.transform, false);
            var pc = player.AddComponent<PlayerController>();

            var cam = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.239f, 0.173f, 0.42f);
            cam.fieldOfView = 55f;
            cam.farClipPlane = 120f;
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cam.gameObject.AddComponent<PerfOverlay>();
            var orbit = cam.gameObject.AddComponent<OrbitCamera>();
            orbit.Target = player.transform;
            pc.CameraPivot = cam.transform;

            var spawn = new GameObject("Depart").transform;
            spawn.position = new Vector3(0, 0.05f, 3f);
            spawn.rotation = Quaternion.Euler(0, 180f, 0); // regarde vers +Z (les modèles regardent vers -Z)

            // Ennemis visibles sur la carte, dans l'ordre du déroulé (SPEC § 3.5)
            var zones = new List<EncounterZone>
            {
                Zone("zone_pigeons", "q1_tuto_pigeons", new Vector3(0, 0, 13), "Ennemi_Pigeon", 3, 0.7f),
                Zone("zone_brume", "q1_brume_montee", new Vector3(2f, 6f, 62), null, 2, 1.4f, required: "vu_guignol_scene"),
                Zone("zone_lion", "q1_lion_place", new Vector3(-3, 6, 70), "Ennemi_Lion", 1, 1f, required: "vu_guignol_scene"),
                Zone("zone_ficelle", "q1_elite_ficelle", new Vector3(3, 6, 79), "Ennemi_Controleur", 2, 1f, required: "vu_guignol_scene"),
                Zone("zone_gros_caillou", "q1_boss_gros_caillou", new Vector3(0, 6, 90), "Boss_Gros_Caillou", 1, 1f, required: "vu_guignol_scene", still: true),
            };

            // Scènes d'histoire
            Talk("Guignol", "traboule_guignol", "vu_guignol_scene", null, new Vector3(0, 6, 55), guignol: true);
            Talk("Avant_Gros_Caillou", "avant_gros_caillou", "vu_gros_caillou_scene", "vu_guignol_scene", new Vector3(0, 6, 84));

            // Interface et chef d'orchestre
            var uiGo = new GameObject("Interface", typeof(UIDocument));
            var ui = uiGo.GetComponent<UIDocument>();
            ui.panelSettings = ArenaBuilder.MakePanelSettings();
            ui.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Root + "/UI/Explore.uxml");

            var director = new GameObject("Quartier_Directeur").AddComponent<ExplorationDirector>();
            director.Ui = ui;
            director.Player = pc;
            director.OrbitCam = orbit;
            director.StoryInk = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/Dialogues/q1_croix_rousse.json");
            director.SpawnPoint = spawn;
            director.Zones = zones.ToArray();
            player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

            EditorSceneManager.SaveScene(scene, ScenePath);

            // La scène de test du J0 sort du build : elle ne sert plus au joueur et pèse sur le démarrage
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath && !s.path.EndsWith("J0_Test.unity")).ToList();
            scenes.Insert(Mathf.Min(1, scenes.Count), new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            ArenaBuilder.SetBootTarget("Q1_CroixRousse");
            AssetDatabase.SaveAssets();
            Debug.Log("[Quartier] Q1_CroixRousse générée");
        }

        static EncounterZone Zone(string id, string encounter, Vector3 pos, string prefab, int count, float spacing, string required = null, bool still = false)
        {
            var go = new GameObject(id);
            go.transform.position = pos;
            var z = go.AddComponent<EncounterZone>();
            z.ZoneId = id;
            z.EncounterId = encounter;
            z.RequiredFlag = required;
            if (still) { z.PatrolRadius = 0; z.ContactRadius = 0; z.StrikeRadius = 0; z.PatrolSpeed = 0; }

            for (int i = 0; i < count; i++)
            {
                GameObject v;
                var p = prefab != null ? AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Art/Characters/{prefab}.prefab") : null;
                if (p != null) v = (GameObject)PrefabUtility.InstantiatePrefab(p);
                else
                {
                    v = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Object.DestroyImmediate(v.GetComponent<Collider>());
                    v.transform.localScale = Vector3.one * 1.3f;
                    var brume = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Settings/M_Brume.mat");
                    if (brume != null) v.GetComponent<Renderer>().sharedMaterial = brume;
                }
                v.transform.SetParent(go.transform, false);
                v.transform.localPosition = new Vector3((i - (count - 1) / 2f) * spacing, p == null ? 1f : 0f, 0);
            }
            return z;
        }

        static void Talk(string name, string knot, string doneFlag, string required, Vector3 pos, bool guignol = false)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var t = go.AddComponent<DialogueZone>();
            t.Knot = knot;
            t.DoneFlag = doneFlag;
            t.RequiredFlag = required;
            t.Radius = 3f;
            if (guignol)
            {
                // Guignol provisoire : marionnette simple (veste marron, nœud rouge), en attendant son modèle
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.transform.SetParent(go.transform, false);
                body.transform.localPosition = new Vector3(1.6f, 1.1f, 0);
                body.transform.localScale = new Vector3(0.6f, 0.55f, 0.6f);
                body.GetComponent<Renderer>().sharedMaterial = M("M_Guignol_Veste", new Color(0.45f, 0.28f, 0.16f));
                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.transform.SetParent(go.transform, false);
                head.transform.localPosition = new Vector3(1.6f, 1.85f, 0);
                head.transform.localScale = Vector3.one * 0.5f;
                head.GetComponent<Renderer>().sharedMaterial = M("M_Guignol_Visage", new Color(0.95f, 0.78f, 0.65f));
                var bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bow.transform.SetParent(go.transform, false);
                bow.transform.localPosition = new Vector3(1.6f, 1.55f, -0.28f);
                bow.transform.localScale = new Vector3(0.3f, 0.12f, 0.08f);
                bow.GetComponent<Renderer>().sharedMaterial = M("M_Guignol_Noeud", new Color(0.85f, 0.12f, 0.15f));
                foreach (var c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            }
        }

        static GameObject Block(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = name;
            b.transform.SetParent(parent);
            b.transform.position = pos;
            b.transform.localScale = size;
            b.GetComponent<Renderer>().sharedMaterial = mat;
            b.isStatic = true;
            return b;
        }

        static Material M(string name, Color c) => ArenaBuilder.Mat(_toon, name, c);

        static readonly string[] FacadeTextures = { "tex_facade_ocre", "tex_facade_rose", "tex_facade_jaune" };

        // Matériau toon texturé avec une texture Higgsfield du quartier (1024, DXT Crunch)
        static Material Tex(string name, string texture, Vector2 tiling)
        {
            string path = $"{Root}/Art/Environments/Q1_CroixRousse/{texture}.png";
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && (ti.maxTextureSize != 1024 || !ti.crunchedCompression))
            {
                ti.maxTextureSize = 1024;
                ti.crunchedCompression = true;
                ti.compressionQuality = 50;
                ti.SaveAndReimport();
            }
            var m = M(name, Color.white);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                m.SetTexture("_BaseMap", tex);
                m.SetTextureScale("_BaseMap", tiling);
            }
            return m;
        }

        static void Fit(GameObject go, float height)
        {
            var b = go.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            if (b.size.y < 1e-4f) return;
            var target = go.transform.position;
            go.transform.localScale *= height / b.size.y;
            b = go.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            go.transform.position += new Vector3(target.x - b.center.x, target.y - b.min.y, target.z - b.center.z);
        }
    }
}
