using UnityEngine;
using UnityEngine.InputSystem;

namespace StreetMythos.Exploration
{
    // Caméra orbitale libre avec collisions (SPEC § 5.1) : souris avec clic droit, ou stick droit
    public sealed class OrbitCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 6f, Height = 1.5f, MinPitch = -10f, MaxPitch = 60f, Sensitivity = 0.15f, StickSpeed = 120f;
        public LayerMask Obstacles = ~0;

        float _yaw, _pitch = 18f;
        InputAction _look, _stick;

        void Awake()
        {
            _look = new InputAction("Regard", InputActionType.Value, "<Mouse>/delta");
            _stick = new InputAction("Regard manette", InputActionType.Value, "<Gamepad>/rightStick");
            _look.Enable(); _stick.Enable();
        }

        void OnDestroy() { _look.Dispose(); _stick.Dispose(); }

        public void SetYaw(float yaw) => _yaw = yaw;

        void LateUpdate()
        {
            if (Target == null) return;
            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                var d = _look.ReadValue<Vector2>();
                _yaw += d.x * Sensitivity;
                _pitch -= d.y * Sensitivity;
            }
            var s = _stick.ReadValue<Vector2>();
            _yaw += s.x * StickSpeed * Time.unscaledDeltaTime;
            _pitch -= s.y * StickSpeed * 0.6f * Time.unscaledDeltaTime;
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);

            var rot = Quaternion.Euler(_pitch, _yaw, 0);
            var focus = Target.position + Vector3.up * Height;
            float dist = Distance;
            // Collisions : la caméra se rapproche si un mur s'interpose
            if (Physics.SphereCast(focus, 0.25f, rot * Vector3.back, out var hit, Distance, Obstacles, QueryTriggerInteraction.Ignore))
                dist = Mathf.Max(0.8f, hit.distance - 0.1f);
            transform.SetPositionAndRotation(focus + rot * Vector3.back * dist, rot);
        }
    }
}
