using UnityEngine;
using StreetMythos.Core;

namespace StreetMythos.Exploration
{
    // Lance une scène d'histoire quand le joueur entre dans la zone (une seule fois)
    public sealed class DialogueZone : MonoBehaviour
    {
        public string Knot;
        public string DoneFlag;
        public string RequiredFlag;
        public float Radius = 2.5f;

        ExplorationDirector _director;
        PlayerController _player;

        void Start()
        {
            _director = FindFirstObjectByType<ExplorationDirector>();
            _player = FindFirstObjectByType<PlayerController>();
        }

        void Update()
        {
            var p = GameProgress.Current;
            if (_player == null || _player.Frozen || p.Has(DoneFlag)) return;
            if (!string.IsNullOrEmpty(RequiredFlag) && !p.Has(RequiredFlag) && !p.IsDefeated(RequiredFlag)) return;
            if (Vector3.Distance(_player.transform.position, transform.position) > Radius) return;
            p.Set(DoneFlag);
            StartCoroutine(_director.Talk(Knot));
        }
    }
}
