using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using StreetMythos.Battle;
using StreetMythos.Data;

namespace StreetMythos.BattleView
{
    // Fait jouer un combat : relie le modèle (règles) à l'arène 3D et à l'interface
    public sealed class BattleController : MonoBehaviour
    {
        [Header("Données")]
        public TextAsset SkillsJson, HeroesJson, EnemiesJson, EncountersJson;
        public string EncounterId = "q1_tuto_pigeons";
        public int HeroLevel = 1;
        public uint Seed = 1;
        public Assist Assist = Assist.None;

        [Header("Scène")]
        public UIDocument Ui;
        public Transform[] HeroSlots, EnemySlots;
        public GameObject[] HeroPrefabs;     // dans l'ordre de HeroIds
        public string[] HeroIds = { "yanis", "ines", "momo" };
        public Material ToonTemplate;

        [Header("Debug")]
        public bool AutoPlay;                // l'IA joue les deux camps (captures, tests)
        public float TelegraphTime = 0.7f;   // délai entre l'annonce d'un coup et l'impact

        BattleModel _model;
        BattleHud _hud;
        GameData _data;
        ReactionInput _input;
        readonly Dictionary<BattleUnit, UnitView> _views = new Dictionary<BattleUnit, UnitView>();
        Action _pendingCommand;

        public BattleModel Model => _model;
        public bool Finished { get; private set; }

        void Start()
        {
            if (Application.absoluteURL.Contains("auto=1") || Environment.GetCommandLineArgs().Contains("-autoplay")) AutoPlay = true;
            _data = new GameData(SkillsJson.text, HeroesJson.text, EnemiesJson.text, EncountersJson.text);
            _input = new ReactionInput();
            EnsureEventSystem();
            StartCoroutine(Run());
        }

        void OnDestroy() => _input?.Dispose();

        // Sans le gestionnaire d'entrées historique, l'interface reçoit les clics par ce module
        static void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null) return;
            var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            DontDestroyOnLoad(go);
        }

        void Setup()
        {
            foreach (var v in _views.Values) if (v) Destroy(v.gameObject);
            _views.Clear();

            var heroes = HeroIds.Select(id => _data.BuildHero(id, HeroLevel)).ToList();
            _model = new BattleModel(heroes, _data.BuildEncounter(EncounterId), Seed);
            _model.Emitted += OnEvent;

            int h = 0, e = 0;
            foreach (var u in _model.Units)
            {
                bool hero = u.Team == Team.Heroes;
                var slot = hero ? HeroSlots[h++] : EnemySlots[e++];
                var go = hero ? SpawnHero(u.Id) : SpawnEnemy(u);
                go.transform.SetPositionAndRotation(slot.position, slot.rotation);
                var view = go.AddComponent<UnitView>();
                view.Bind(u);
                _views[u] = view;
            }

            Ui.rootVisualElement.Q("party").Clear();
            Ui.rootVisualElement.Q("foes").Clear();
            _hud = new BattleHud(Ui.rootVisualElement);
            _hud.Build(_model);
            _hud.Refresh(_model);
            _hud.HideBanner();
        }

        GameObject SpawnHero(string id)
        {
            int i = Array.IndexOf(HeroIds, id);
            if (i >= 0 && i < HeroPrefabs.Length && HeroPrefabs[i] != null) return Instantiate(HeroPrefabs[i]);
            return Placeholder(PrimitiveType.Capsule, new Color(1f, 0.6f, 0.2f), 1f);
        }

        // Ennemis provisoires (formes simples) en attendant leurs modèles du lot 5
        GameObject SpawnEnemy(BattleUnit u)
        {
            string id = u.Def.Id;
            if (id.StartsWith("pigeon")) return Placeholder(PrimitiveType.Sphere, new Color(0.55f, 0.6f, 0.75f), 0.6f);
            if (id.StartsWith("brumeux")) return Placeholder(PrimitiveType.Capsule, new Color(0.56f, 0.64f, 0.78f), 1.1f);
            if (id.StartsWith("lion")) return Placeholder(PrimitiveType.Cube, new Color(0.8f, 0.75f, 0.62f), 1.2f);
            if (id.StartsWith("controleur")) return Placeholder(PrimitiveType.Capsule, new Color(0.2f, 0.22f, 0.4f), 1.2f);
            if (id.StartsWith("gros_caillou")) return Placeholder(PrimitiveType.Cube, new Color(0.5f, 0.48f, 0.45f), 2.6f);
            return Placeholder(PrimitiveType.Cube, Color.magenta, 1f);
        }

        GameObject Placeholder(PrimitiveType type, Color color, float size)
        {
            var root = new GameObject("Ennemi");
            var shape = GameObject.CreatePrimitive(type);
            Destroy(shape.GetComponent<Collider>());
            shape.transform.SetParent(root.transform, false);
            shape.transform.localScale = Vector3.one * size;
            shape.transform.localPosition = Vector3.up * size * (type == PrimitiveType.Capsule ? 1f : 0.5f);
            var mat = ToonTemplate != null ? new Material(ToonTemplate) : new Material(Shader.Find("StreetMythos/Toon"));
            mat.SetColor("_BaseColor", color);
            shape.GetComponent<Renderer>().sharedMaterial = mat;
            return root;
        }

        // ---------- Boucle de combat ----------

        IEnumerator Run()
        {
            while (true)
            {
                Setup();
                yield return new WaitForSecondsRealtime(0.5f);
                while (_model.Outcome == BattleOutcome.Ongoing)
                {
                    var actor = _model.NextActor();
                    if (actor == null) break;
                    _hud.Refresh(_model);
                    if (actor.Team == Team.Heroes) yield return HeroTurn(actor);
                    else yield return EnemyTurn();
                    _hud.Refresh(_model);
                    yield return new WaitForSecondsRealtime(0.25f);
                }

                switch (_model.Outcome)
                {
                    case BattleOutcome.Victory: _hud.ShowBanner("Victoire !"); break;
                    case BattleOutcome.Fled: _hud.ShowBanner("Fuite réussie"); break;
                    default: _hud.ShowBanner("K.-O. … on remet ça"); break;
                }
                yield return new WaitForSecondsRealtime(2.5f);
                // Défaite : le combat recommence à l'identique, sans pénalité (SPEC § 4.3)
                if (_model.Outcome != BattleOutcome.Defeat) break;
                Seed++;
            }
            Finished = true;
        }

        IEnumerator HeroTurn(BattleUnit actor)
        {
            _pendingCommand = null;
            if (AutoPlay) _pendingCommand = AutoCommand(actor);
            else ShowRootMenu(actor);
            while (_pendingCommand == null) yield return null;
            _hud.HideMenu();

            // L'élan est joué avant d'appliquer l'action, pour que les chiffres tombent à l'impact
            var cmd = _pendingCommand;
            if (_lungeTarget != null && _views.TryGetValue(_lungeTarget, out var tv))
                yield return _views[actor].Lunge(tv.transform.position);
            cmd();
            _lungeTarget = null;
        }

        BattleUnit _lungeTarget;

        void Pick(Action command, BattleUnit lungeAt = null)
        {
            _lungeTarget = lungeAt;
            _pendingCommand = command;
        }

        void ShowRootMenu(BattleUnit actor)
        {
            _hud.ShowMenu($"{actor.Def.Name} · Flow {actor.Flow}", new[]
            {
                new BattleHud.Option { Label = "Attaque", OnPick = () => ShowTargets(actor, t => Pick(() => _model.Attack(t), t)) },
                new BattleHud.Option { Label = "Compétence", Enabled = actor.Def.Skills.Count > 0, OnPick = () => ShowSkills(actor) },
                new BattleHud.Option { Label = "Tchatche", OnPick = () => ShowTargets(actor, t => ShowVannes(actor, t)) },
                new BattleHud.Option { Label = "Défense", OnPick = () => Pick(() => _model.Defend()) },
                new BattleHud.Option { Label = "Fuite", Enabled = _model.CanFlee, OnPick = () => Pick(() => _model.Flee()) },
            });
        }

        void ShowTargets(BattleUnit actor, Action<BattleUnit> then, bool allies = false)
        {
            var options = (allies ? _model.Heroes : _model.Enemies).Where(u => u.IsAlive)
                .Select(u => new BattleHud.Option { Label = $"{u.Def.Name} ({u.Hp} PV)", OnPick = () => then(u) }).ToList();
            options.Add(new BattleHud.Option { Label = "Retour", IsBack = true, OnPick = () => ShowRootMenu(actor) });
            _hud.ShowMenu("Cible", options);
        }

        void ShowSkills(BattleUnit actor)
        {
            var options = actor.Def.Skills.Select(s => new BattleHud.Option
            {
                Label = $"{s.Name} · {s.FlowCost} Flow",
                Enabled = _model.CanUse(s),
                OnPick = () =>
                {
                    switch (s.Target)
                    {
                        case TargetKind.SingleEnemy: ShowTargets(actor, t => Pick(() => _model.UseSkill(s, t), s.Power > 0 ? t : null)); break;
                        case TargetKind.SingleAlly: ShowTargets(actor, t => Pick(() => _model.UseSkill(s, t)), allies: true); break;
                        default: Pick(() => _model.UseSkill(s, null)); break;
                    }
                }
            }).ToList();
            options.Add(new BattleHud.Option { Label = "Retour", IsBack = true, OnPick = () => ShowRootMenu(actor) });
            _hud.ShowMenu("Compétences", options);
        }

        void ShowVannes(BattleUnit actor, BattleUnit target)
        {
            // Les textes des vannes viendront des fichiers Ink (lot 8)
            _hud.ShowMenu("Tchatche", new[]
            {
                new BattleHud.Option { Label = "Clash", OnPick = () => Pick(() => _model.Tchatche(target, VanneType.Clash)) },
                new BattleHud.Option { Label = "Chambrage", OnPick = () => Pick(() => _model.Tchatche(target, VanneType.Chambrage)) },
                new BattleHud.Option { Label = "Mytho", OnPick = () => Pick(() => _model.Tchatche(target, VanneType.Mytho)) },
                new BattleHud.Option { Label = "Retour", IsBack = true, OnPick = () => ShowRootMenu(actor) },
            });
        }

        // IA du pilote automatique : la même logique que la simulation d'équilibrage
        Action AutoCommand(BattleUnit actor)
        {
            var target = _model.Enemies.Where(e => e.IsAlive).OrderBy(e => e.Hp).First();
            var skill = actor.Def.Skills.Where(s => _model.CanUse(s) && s.Power > 0).OrderByDescending(s => s.FlowCost).FirstOrDefault();
            if (target.Def.Weakness.HasValue && target.Moral > 0 && !target.IsDestabilized && UnityEngine.Random.value < 0.3f)
                return () => _model.Tchatche(target, target.Def.Weakness.Value);
            if (skill != null && skill.Target == TargetKind.SingleEnemy) { _lungeTarget = target; return () => _model.UseSkill(skill, target); }
            _lungeTarget = target;
            return () => _model.Attack(target);
        }

        IEnumerator EnemyTurn()
        {
            var turn = _model.StartEnemyTurn();
            var attacker = _views[turn.Actor];
            while (turn.NextHit(out var hit))
            {
                // Annonce du coup, puis impact à heure fixe : la fenêtre est jugée sur l'horodatage des entrées
                _hud.ShowHint($"{turn.Move.Name} sur {hit.Target.Def.Name} ! Espace : esquive · F : parade");
                _input.Arm();
                double impact = Time.realtimeSinceStartupAsDouble + TelegraphTime;
                if (hit.HitIndex == 0) StartCoroutine(attacker.Lunge(_views[hit.Target].transform.position, TelegraphTime * 0.9f));
                if (AutoPlay)
                {
                    float r = UnityEngine.Random.value;
                    _input.Simulate(r < 0.4f ? impact : (double?)null, r > 0.8f ? impact : (double?)null);
                }
                while (Time.realtimeSinceStartupAsDouble < impact + ReactionJudge.DodgeWindow / 2) yield return null;
                _input.Disarm();
                _hud.HideHint();
                turn.Resolve(ReactionJudge.Judge(impact, _input.DodgeAt, _input.ParryAt, Assist));
                _hud.Refresh(_model);
                yield return new WaitForSecondsRealtime(0.2f);
            }
            turn.Finish();
            yield return new WaitForSecondsRealtime(0.3f);
        }

        // ---------- Réaction visuelle aux événements du modèle ----------

        void OnEvent(BattleEvent e)
        {
            switch (e)
            {
                case Damaged d when _views.TryGetValue(d.Target, out var v):
                    if (d.Reaction == Reaction.Dodge) { _hud.Float(v.Head, "Esquive !", "good", this); StartCoroutine(v.Dodge()); }
                    else if (d.Reaction == Reaction.Parry) _hud.Float(v.Head, "Parade !", "good", this);
                    else { _hud.Float(v.Head, d.Amount.ToString(), d.Critical ? "crit" : null, this); StartCoroutine(v.Recoil()); }
                    break;
                case MoralChanged m when _views.TryGetValue(m.Target, out var v):
                    _hud.Float(v.Head + Vector3.up * 0.3f, $"Moral {m.Delta} · {m.Result}", "moral", this);
                    break;
                case Destabilized s when _views.TryGetValue(s.Target, out var v):
                    _hud.Float(v.Head + Vector3.up * 0.6f, "Déstabilisé !", "crit", this);
                    break;
                case Healed h when _views.TryGetValue(h.Target, out var v) && h.Amount > 0:
                    _hud.Float(v.Head, $"+{h.Amount}", "heal", this);
                    break;
                case Knocked k when _views.TryGetValue(k.Target, out var v):
                    StartCoroutine(v.Fall());
                    break;
                case TurnSkipped t when _views.TryGetValue(t.Unit, out var v):
                    _hud.Float(v.Head, "Tour perdu", "moral", this);
                    break;
            }
        }
    }
}
