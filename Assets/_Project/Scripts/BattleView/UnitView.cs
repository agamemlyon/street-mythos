using System.Collections;
using UnityEngine;
using StreetMythos.Battle;

namespace StreetMythos.BattleView
{
    // Représentation 3D d'un combattant : position de repos, élan vers la cible, recul, chute
    public sealed class UnitView : MonoBehaviour
    {
        public BattleUnit Unit { get; private set; }
        Vector3 _home;
        Quaternion _homeRot;
        Animation _anim;
        Renderer[] _renderers;

        public void Bind(BattleUnit unit)
        {
            Unit = unit;
            _home = transform.position;
            _homeRot = transform.rotation;
            _anim = GetComponentInChildren<Animation>();
            _renderers = GetComponentsInChildren<Renderer>();
            Idle();
        }

        public Vector3 Head => transform.position + Vector3.up * Height();

        float Height()
        {
            if (_renderers == null || _renderers.Length == 0) return 1.6f;
            float top = float.MinValue;
            foreach (var r in _renderers) top = Mathf.Max(top, r.bounds.max.y);
            return top - transform.position.y + 0.2f;
        }

        void Idle()
        {
            if (_anim == null || _anim.clip == null) return;
            _anim.Stop();
            _anim.clip.SampleAnimation(gameObject, 0f);
        }

        // Course vers la cible, coup, retour
        public IEnumerator Lunge(Vector3 target, float duration = 0.35f)
        {
            if (_anim != null && _anim.clip != null) _anim.Play();
            var dir = (target - _home);
            dir.y = 0;
            var strike = _home + dir * 0.7f;
            transform.rotation = Quaternion.LookRotation(-dir.normalized); // les modèles regardent vers -Z
            yield return Move(_home, strike, duration);
            yield return new WaitForSecondsRealtime(0.08f);
            yield return Move(strike, _home, duration * 0.8f);
            transform.rotation = _homeRot;
            Idle();
        }

        public IEnumerator Recoil()
        {
            var back = _home + (transform.position - Camera.main.transform.position).normalized * 0.05f - transform.forward * 0.25f;
            yield return Move(_home, back, 0.06f);
            yield return Move(back, _home, 0.12f);
        }

        public IEnumerator Dodge()
        {
            var side = _home + transform.right * 0.6f;
            yield return Move(_home, side, 0.08f);
            yield return new WaitForSecondsRealtime(0.15f);
            yield return Move(side, _home, 0.15f);
        }

        public IEnumerator Fall()
        {
            var start = transform.rotation;
            var end = start * Quaternion.Euler(0, 0, 80f);
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 3f)
            {
                transform.rotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        IEnumerator Move(Vector3 from, Vector3 to, float duration)
        {
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t / duration));
                yield return null;
            }
            transform.position = to;
        }
    }
}
