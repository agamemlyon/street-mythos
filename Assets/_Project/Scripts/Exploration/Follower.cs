using System.Collections.Generic;
using UnityEngine;

namespace StreetMythos.Exploration
{
    // Coéquipier qui suit le meneur à la trace (Inès et Momo derrière Yanis) : il repasse là où le meneur
    // est passé, ce qui évite de couper à travers les murs. Pas de physique, la trace suit déjà le sol.
    public sealed class Follower : MonoBehaviour
    {
        public Transform Leader;
        public float Distance = 1.6f;            // longueur de trace entre le meneur et ce coéquipier
        public float Side;                       // décalage latéral (formation en V : la caméra garde le meneur en vue)
        public float WalkSpeed = 3.2f;

        readonly List<Vector3> _trail = new List<Vector3>();   // du plus récent au plus ancien
        Animation _anim;
        const float Spacing = 0.15f, TeleportDistance = 8f;

        void Awake() => _anim = GetComponentInChildren<Animation>();

        void Start() => Snap();

        // Replace le coéquipier derrière le meneur (début de partie, téléportation du joueur)
        void Snap()
        {
            _trail.Clear();
            var back = Leader.forward; // les modèles regardent vers -Z : « derrière » est +forward
            back.y = 0;
            for (float d = 0; d <= Distance + Spacing; d += Spacing) _trail.Add(Leader.position + back.normalized * d);
            transform.SetPositionAndRotation(_trail[_trail.Count - 1] + Leader.right * Side, Leader.rotation);
        }

        void LateUpdate()
        {
            if (Leader == null) return;
            if ((Leader.position - transform.position).sqrMagnitude > TeleportDistance * TeleportDistance) { Snap(); return; }

            if (_trail.Count == 0 || (Leader.position - _trail[0]).sqrMagnitude > Spacing * Spacing)
                _trail.Insert(0, Leader.position);

            // Point de la trace situé à « Distance » du meneur
            Vector3 goal = _trail[_trail.Count - 1];
            float run = 0;
            for (int i = 1; i < _trail.Count; i++)
            {
                run += Vector3.Distance(_trail[i - 1], _trail[i]);
                if (run >= Distance) { goal = _trail[i]; _trail.RemoveRange(i + 1, _trail.Count - i - 1); break; }
            }

            goal += Leader.right * Side;
            var step = goal - transform.position;
            float dist = step.magnitude;
            bool moving = dist > 0.05f;
            if (moving)
            {
                // Rattrape plus vite s'il est distancé (course du meneur)
                float speed = Mathf.Max(WalkSpeed, dist * 4f);
                transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.deltaTime);
                var flat = new Vector3(step.x, 0, step.z);
                if (flat.sqrMagnitude > 0.0004f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-flat), 10f * Time.deltaTime);
                if (_anim != null && _anim.clip != null)
                {
                    if (!_anim.IsPlaying(_anim.clip.name)) _anim.Play(_anim.clip.name);
                    _anim[_anim.clip.name].speed = speed / WalkSpeed;
                }
            }
            else if (_anim != null && _anim.isPlaying && _anim.clip != null)
            {
                _anim.Stop();
                _anim.clip.SampleAnimation(_anim.gameObject, 0f);
            }
        }
    }
}
