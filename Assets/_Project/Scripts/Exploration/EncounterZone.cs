using UnityEngine;
using UnityEngine.SceneManagement;
using StreetMythos.Core;

namespace StreetMythos.Exploration
{
    // Ennemi visible sur la carte (SPEC § 4.2) : il patrouille ; le toucher lance le combat.
    // Frapper le premier (E à portée, de dos ou de côté) donne le premier tour à l'équipe ;
    // se faire surprendre le donne aux ennemis.
    public sealed class EncounterZone : MonoBehaviour
    {
        public string ZoneId;
        public string EncounterId;
        public string ArenaScene = "Arena_Q1";
        public float PatrolRadius = 2.5f, PatrolSpeed = 1.2f, ContactRadius = 1.3f, StrikeRadius = 2.4f;
        public string RequiredFlag;                 // la zone n'apparaît qu'une fois ce drapeau posé

        Vector3 _home;
        float _t;
        PlayerController _player;
        bool _triggered;

        void Start()
        {
            var p = GameProgress.Current;
            if (p.IsDefeated(ZoneId))
            {
                gameObject.SetActive(false);
                return;
            }
            _home = transform.position;
            _player = FindFirstObjectByType<PlayerController>();
            _t = Random.value * 10f;
        }

        void Update()
        {
            // Zone en attente d'un événement d'histoire : invisible et inactive jusque-là
            bool ready = string.IsNullOrEmpty(RequiredFlag) || GameProgress.Current.Has(RequiredFlag);
            foreach (Transform c in transform) c.gameObject.SetActive(ready);
            if (!ready || _triggered || _player == null || _player.Frozen) return;
            _t += Time.deltaTime * PatrolSpeed;
            var next = _home + new Vector3(Mathf.Cos(_t), 0, Mathf.Sin(_t * 0.7f)) * PatrolRadius;
            var step = next - transform.position;
            if (step.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(-step.normalized);
            transform.position = next;

            Vector3 toPlayer = _player.transform.position - transform.position;
            toPlayer.y = 0;
            // Distance au membre du groupe le plus proche (un groupe de pigeons s'étale sur plusieurs mètres)
            float d = toPlayer.magnitude;
            foreach (Transform c in transform)
            {
                var v = _player.transform.position - c.position;
                v.y = 0;
                d = Mathf.Min(d, v.magnitude);
            }
            if (d < StrikeRadius && _player.Interact.WasPressedThisFrame())
            {
                // L'ennemi regarde vers -Z : il est surpris si le joueur n'est pas devant lui
                bool behind = Vector3.Dot(-transform.forward, toPlayer.normalized) < 0.3f;
                Launch(behind ? 1 : 0);
            }
            else if (d < ContactRadius)
            {
                bool facing = Vector3.Dot(-transform.forward, toPlayer.normalized) > 0.5f;
                Launch(facing ? 2 : 0);
            }
        }

        public void Launch(int opening)
        {
            _triggered = true;
            var p = GameProgress.Current;
            p.SetPosition(_player.transform.position, _player.transform.eulerAngles.y);
            p.scene = SceneManager.GetActiveScene().name;
            BattleRequest.EncounterId = EncounterId;
            BattleRequest.ZoneId = ZoneId;
            BattleRequest.Opening = opening;
            BattleRequest.ReturnScene = p.scene;
            GameProgress.Save();
            SceneManager.LoadScene(ArenaScene);
        }
    }
}
