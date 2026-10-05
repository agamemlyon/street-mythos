using UnityEngine;
using UnityEngine.InputSystem;

namespace StreetMythos.Exploration
{
    // Déplacement en troisième personne : marche, course, pas de saut (SPEC § 4.2)
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float WalkSpeed = 3.2f, RunSpeed = 6f, TurnSpeed = 12f;
        public Transform CameraPivot;
        public bool Frozen;                       // pendant un dialogue ou un menu

        CharacterController _cc;
        InputAction _move, _run, _interact;
        Animation _anim;
        float _vy;

        public InputAction Interact => _interact;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _anim = GetComponentInChildren<Animation>();
            _move = new InputAction("Déplacement", InputActionType.Value);
            _move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/z").With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/q").With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");
            _run = new InputAction("Course", InputActionType.Button);
            _run.AddBinding("<Keyboard>/leftShift");
            _run.AddBinding("<Gamepad>/leftStickPress");
            _interact = new InputAction("Interagir", InputActionType.Button);
            _interact.AddBinding("<Keyboard>/e");
            _interact.AddBinding("<Keyboard>/enter");
            _interact.AddBinding("<Gamepad>/buttonSouth");
            _move.Enable(); _run.Enable(); _interact.Enable();
        }

        void OnDestroy()
        {
            _move.Dispose(); _run.Dispose(); _interact.Dispose();
        }

        void Update()
        {
            Vector2 input = Frozen ? Vector2.zero : _move.ReadValue<Vector2>();
            var cam = CameraPivot != null ? CameraPivot : Camera.main.transform;
            Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, fwd);
            Vector3 dir = Vector3.ClampMagnitude(fwd * input.y + right * input.x, 1f);
            float speed = _run.IsPressed() ? RunSpeed : WalkSpeed;

            _vy = _cc.isGrounded ? -1f : _vy - 20f * Time.deltaTime;
            _cc.Move((dir * speed + Vector3.up * _vy) * Time.deltaTime);

            if (dir.sqrMagnitude > 0.01f)
            {
                // Les modèles regardent vers -Z
                var target = Quaternion.LookRotation(-dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, TurnSpeed * Time.deltaTime);
                if (_anim != null && !_anim.isPlaying) _anim.Play();
                if (_anim != null && _anim.clip != null) _anim[_anim.clip.name].speed = speed / WalkSpeed;
            }
            else if (_anim != null && _anim.isPlaying)
            {
                _anim.Stop();
                _anim.clip.SampleAnimation(_anim.gameObject, 0f);
            }
        }
    }
}
