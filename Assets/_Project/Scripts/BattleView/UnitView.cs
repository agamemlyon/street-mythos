using System;
using System.Collections;
using UnityEngine;
using StreetMythos.Battle;

namespace StreetMythos.BattleView
{
    // Représentation 3D d'un combattant : garde, course vers la cible, coup, esquive, parade, chute.
    // Les clips (« stance », « attaque », « esquive », « parade ») sont ajoutés aux prefabs par ArenaBuilder ;
    // un combattant sans clip se contente des mouvements par code.
    public sealed class UnitView : MonoBehaviour
    {
        public BattleUnit Unit { get; private set; }
        Vector3 _home;
        Quaternion _homeRot;
        Animation _anim;
        Renderer[] _renderers;
        string _walk;

        public void Bind(BattleUnit unit)
        {
            Unit = unit;
            _home = transform.position;
            _homeRot = transform.rotation;
            _anim = GetComponentInChildren<Animation>();
            _renderers = GetComponentsInChildren<Renderer>();
            if (_anim != null)
                foreach (AnimationState s in _anim)
                    if (s.name.Contains("Walk")) _walk = s.name;
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

        bool Has(string clip) => _anim != null && _anim[clip] != null;

        // Joue un clip, accéléré pour tenir dans maxDuration (les clips de la bibliothèque durent jusqu'à 7 s) ;
        // renvoie la durée réelle (0 s'il n'existe pas)
        float PlayClip(string clip, float fade = 0.1f, bool loop = false, float maxDuration = 0f)
        {
            if (!Has(clip)) return 0f;
            var state = _anim[clip];
            state.wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
            state.speed = maxDuration > 0 && state.length > maxDuration ? state.length / maxDuration : 1f;
            state.time = 0;
            _anim.CrossFade(clip, fade);
            return state.length / state.speed;
        }

        void Idle()
        {
            if (PlayClip("stance", 0.2f, loop: true) > 0) return;
            if (_anim == null || _anim.clip == null) return;
            _anim.Stop();
            _anim.clip.SampleAnimation(_anim.gameObject, 0f);
        }

        // Course vers la cible, coup (onImpact appelé au moment du contact), retour en garde
        public IEnumerator Lunge(Vector3 target, float duration = 0.35f, Action onImpact = null)
        {
            var dir = target - _home;
            dir.y = 0;
            var strike = _home + dir * 0.7f;
            transform.rotation = Quaternion.LookRotation(-dir.normalized); // les modèles regardent vers -Z
            if (_walk != null) PlayClip(_walk, 0.1f, loop: true);
            yield return Move(_home, strike, duration);

            float length = PlayClip("attaque", 0.08f, maxDuration: 1.1f);
            // Impact vers le milieu du clip (le poing ou le pied arrive à ce moment-là)
            yield return new WaitForSecondsRealtime(length > 0 ? length * 0.45f : 0.08f);
            onImpact?.Invoke();
            if (length > 0) yield return new WaitForSecondsRealtime(length * 0.4f);

            if (_walk != null) PlayClip(_walk, 0.1f, loop: true);
            yield return Move(strike, _home, duration * 0.8f);
            transform.rotation = _homeRot;
            Idle();
        }

        public IEnumerator Recoil()
        {
            float length = PlayClip("parade", 0.05f, maxDuration: 0.6f);
            var back = _home - transform.forward * -0.25f;
            yield return Move(_home, back, 0.06f);
            yield return Move(back, _home, 0.12f);
            if (length > 0) { yield return new WaitForSecondsRealtime(Mathf.Max(0, length * 0.6f - 0.18f)); Idle(); }
        }

        public IEnumerator Parry()
        {
            float length = PlayClip("parade", 0.05f, maxDuration: 0.6f);
            yield return new WaitForSecondsRealtime(length > 0 ? length * 0.9f : 0.2f);
            Idle();
        }

        public IEnumerator Dodge()
        {
            float length = PlayClip("esquive", 0.05f, maxDuration: 0.8f);
            var side = _home + transform.right * 0.6f;
            yield return Move(_home, side, 0.08f);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.15f, length * 0.5f));
            yield return Move(side, _home, 0.15f);
            Idle();
        }

        public IEnumerator Fall()
        {
            if (_anim != null) _anim.Stop();
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
