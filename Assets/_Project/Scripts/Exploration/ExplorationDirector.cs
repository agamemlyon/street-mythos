using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using StreetMythos.Core;

namespace StreetMythos.Exploration
{
    // Chef d'orchestre du quartier : menu titre, position du joueur, scènes d'histoire, objectif, sauvegarde
    public sealed class ExplorationDirector : MonoBehaviour
    {
        public UIDocument Ui;
        public PlayerController Player;
        public OrbitCamera OrbitCam;
        public TextAsset StoryInk;                 // Dialogues/q1_croix_rousse.json
        public Transform SpawnPoint;
        public EncounterZone[] Zones;

        DialoguePlayer _dialogue;
        static bool _sessionStarted;               // le menu titre ne s'affiche qu'au lancement

        Label _objective, _status;

        IEnumerator Begin()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            var root = Ui.rootVisualElement;
            _objective = root.Q<Label>("objective");
            _status = root.Q<Label>("status");
            _dialogue = new DialoguePlayer(StoryInk, root);

            if (!_sessionStarted) yield return TitleMenu(root);
            _sessionStarted = true;

            var p = GameProgress.Current;
            if (p.hasPosition && p.scene == SceneManager.GetActiveScene().name)
            {
                Player.GetComponent<CharacterController>().enabled = false;
                Player.transform.SetPositionAndRotation(p.Position, Quaternion.Euler(0, p.yaw, 0));
                Player.GetComponent<CharacterController>().enabled = true;
            }
            else Player.transform.SetPositionAndRotation(SpawnPoint.position, SpawnPoint.rotation);
            OrbitCam.SetYaw(Player.transform.eulerAngles.y + 180f);

            // Scènes d'histoire déclenchées par la progression
            if (!p.Has("intro_vue")) { p.Set("intro_vue"); yield return IntroVideo(); yield return Talk("rencontre_ines_momo"); }
            else if (p.IsDefeated("zone_pigeons") && !p.Has("apres_pigeons")) { p.Set("apres_pigeons"); yield return Talk("apres_pigeons"); }
            else if (p.IsDefeated("zone_gros_caillou") && !p.Has("fin_j1")) { p.Set("fin_j1"); yield return Talk("fin_j1"); yield return Banner("À suivre…"); }
            GameProgress.Save();
        }

        void OnEnable() => StartCoroutine(Begin());
        void OnDestroy() => _dialogue?.Dispose();

        void Update()
        {
            if (_objective == null) return;
            var p = GameProgress.Current;
            _objective.text = Objective(p);
            _status.text = $"Niveau {p.level} · {p.balles} balles · Réput' {p.reput}";
        }

        static string Objective(GameProgress p)
        {
            if (!p.IsDefeated("zone_pigeons")) return "Objectif : chasser les pigeons de la place";
            if (!p.Has("vu_guignol")) return "Objectif : passer par la traboule";
            if (!p.IsDefeated("zone_gros_caillou")) return "Objectif : monter jusqu'au Gros Caillou";
            return "La Croix-Rousse respire. À suivre…";
        }

        public IEnumerator Talk(string knot)
        {
            Player.Frozen = true;
            string combat = null;
            yield return _dialogue.Play(knot, c => combat = c);
            Player.Frozen = false;
            if (combat != null)
            {
                var zone = Zones.FirstOrDefault(z => z != null && z.EncounterId == combat && z.isActiveAndEnabled);
                if (zone != null) zone.Launch(0);
            }
        }

        IEnumerator TitleMenu(VisualElement root)
        {
            Player.Frozen = true;
            var menu = root.Q("title");
            menu.RemoveFromClassList("hidden");
            bool chosen = false;
            var cont = root.Q<Button>("continue");
            cont.SetEnabled(GameProgress.HasSave);
            cont.clicked += () => { GameProgress.Load(); chosen = true; };
            root.Q<Button>("new-game").clicked += () => { GameProgress.NewGame(); chosen = true; };
            while (!chosen) yield return null;
            menu.AddToClassList("hidden");
            Player.Frozen = false;
            // Une partie reprise dans une autre scène y retourne
            var p = GameProgress.Current;
            if (p.hasPosition && p.scene != SceneManager.GetActiveScene().name && Application.CanStreamedLevelBeLoaded(p.scene))
                SceneManager.LoadScene(p.scene);
        }

        // Vidéo d'intro (SPEC § 3.5) hébergée à côté du build et lue par URL ; Espace, E ou clic pour passer
        IEnumerator IntroVideo()
        {
            Player.Frozen = true;
            var cam = UnityEngine.Camera.main;
            var vp = cam.gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
            vp.source = UnityEngine.Video.VideoSource.Url;
            vp.url = System.IO.Path.Combine(Application.streamingAssetsPath, "intro.mp4");
            // Rendu dans une texture affichée par-dessus tout, sur fond noir, sans déformer l'image
            var rt = new RenderTexture(1280, 720, 0);
            vp.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            vp.targetTexture = rt;
            vp.isLooping = false;
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = overlay.style.top = overlay.style.right = overlay.style.bottom = 0;
            overlay.style.backgroundColor = Color.black;
            var screen = new Image { image = rt, scaleMode = ScaleMode.ScaleToFit };
            screen.style.flexGrow = 1;
            overlay.Add(screen);
            Ui.rootVisualElement.Add(overlay);
            vp.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.Direct;
            vp.Prepare();
            float timeout = Time.unscaledTime + 8f;
            while (!vp.isPrepared && Time.unscaledTime < timeout) yield return null;
            if (vp.isPrepared)
            {
                vp.Play();
                var skip = new UnityEngine.InputSystem.InputAction("Passer", UnityEngine.InputSystem.InputActionType.Button);
                skip.AddBinding("<Keyboard>/space"); skip.AddBinding("<Keyboard>/e"); skip.AddBinding("<Keyboard>/escape");
                skip.AddBinding("<Mouse>/leftButton"); skip.AddBinding("<Gamepad>/buttonSouth");
                skip.Enable();
                yield return new WaitForSecondsRealtime(0.5f);
                while (vp.isPlaying && !skip.WasPressedThisFrame()) yield return null;
                skip.Dispose();
            }
            else Debug.LogWarning("[Intro] vidéo introuvable, on passe");
            Destroy(vp);
            overlay.RemoveFromHierarchy();
            rt.Release();
            yield return null;
            Player.Frozen = false;
        }

        IEnumerator Banner(string text)
        {
            var b = Ui.rootVisualElement.Q<Label>("banner");
            b.text = text;
            b.RemoveFromClassList("hidden");
            yield return new WaitForSecondsRealtime(4f);
            b.AddToClassList("hidden");
        }
    }
}
