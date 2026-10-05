using UnityEngine;

namespace StreetMythos.BattleView
{
    // Animation par code des ennemis sans squelette (SPEC D4) : chacun bouge selon sa nature
    public sealed class EnemyMotion : MonoBehaviour
    {
        public enum Style { Pigeon, Stone, Ghost, Golem, Mist }

        public Style Kind;
        public Transform Body;     // partie animée (l'enfant qui porte le modèle)
        Vector3 _basePos;
        Quaternion _baseRot;
        float _seed;

        void Start()
        {
            if (Body == null) Body = transform.childCount > 0 ? transform.GetChild(0) : transform;
            _basePos = Body.localPosition;
            _baseRot = Body.localRotation;
            _seed = Random.value * 10f;
        }

        void Update()
        {
            float t = Time.time + _seed;
            switch (Kind)
            {
                case Style.Pigeon:
                    // Petits sauts nerveux et coups de tête
                    float hop = Mathf.Max(0, Mathf.Sin(t * 7f)) * 0.08f;
                    Body.localPosition = _basePos + Vector3.up * hop;
                    Body.localRotation = _baseRot * Quaternion.Euler(Mathf.Sin(t * 9f) * 8f, Mathf.Sin(t * 1.3f) * 15f, 0);
                    break;
                case Style.Stone:
                    // Statue : immobile, puis de brusques à-coups de quelques degrés
                    float step = Mathf.Floor(t * 1.5f);
                    float jitter = Mathf.Sin(step * 12.9898f) * 4f;
                    Body.localRotation = _baseRot * Quaternion.Euler(0, jitter, 0);
                    Body.localPosition = _basePos + Vector3.up * (Mathf.Repeat(t * 1.5f, 1f) < 0.08f ? 0.03f : 0f);
                    break;
                case Style.Ghost:
                    // Flotte et se balance
                    Body.localPosition = _basePos + Vector3.up * (0.25f + Mathf.Sin(t * 1.6f) * 0.12f);
                    Body.localRotation = _baseRot * Quaternion.Euler(0, 0, Mathf.Sin(t * 0.9f) * 4f);
                    break;
                case Style.Golem:
                    // Masse qui respire lourdement et oscille d'un pied sur l'autre
                    Body.localPosition = _basePos + Vector3.up * Mathf.Abs(Mathf.Sin(t * 0.8f)) * 0.06f;
                    Body.localRotation = _baseRot * Quaternion.Euler(0, 0, Mathf.Sin(t * 0.8f) * 3f);
                    Body.localScale = Vector3.one * (1f + Mathf.Sin(t * 1.6f) * 0.015f);
                    break;
                case Style.Mist:
                    Body.localPosition = _basePos + Vector3.up * (0.15f + Mathf.Sin(t * 1.2f) * 0.1f);
                    Body.localRotation = _baseRot * Quaternion.Euler(0, t * 20f, 0);
                    break;
            }
        }
    }
}
