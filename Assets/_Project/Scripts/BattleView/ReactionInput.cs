using UnityEngine.InputSystem;

namespace StreetMythos.BattleView
{
    // Capte le premier appui d'esquive et de parade avec l'horodatage de l'événement d'entrée
    // (et non celui de l'image), pour que les fenêtres ne dépendent pas des images par seconde
    public sealed class ReactionInput : System.IDisposable
    {
        readonly InputAction _dodge = new InputAction("Esquive", InputActionType.Button);
        readonly InputAction _parry = new InputAction("Parade", InputActionType.Button);
        bool _listening;

        public double? DodgeAt { get; private set; }
        public double? ParryAt { get; private set; }

        public ReactionInput()
        {
            _dodge.AddBinding("<Keyboard>/space");
            _dodge.AddBinding("<Gamepad>/buttonSouth");
            _parry.AddBinding("<Keyboard>/f");
            _parry.AddBinding("<Gamepad>/buttonWest");
            _dodge.performed += c => { if (_listening && DodgeAt == null) DodgeAt = c.time; };
            _parry.performed += c => { if (_listening && ParryAt == null) ParryAt = c.time; };
            _dodge.Enable();
            _parry.Enable();
        }

        // Ouvre l'écoute pour un coup : les appuis précédents sont oubliés
        public void Arm()
        {
            DodgeAt = ParryAt = null;
            _listening = true;
        }

        public void Disarm() => _listening = false;

        // Pour le pilote automatique et les tests
        public void Simulate(double? dodgeAt, double? parryAt)
        {
            if (DodgeAt == null) DodgeAt = dodgeAt;
            if (ParryAt == null) ParryAt = parryAt;
        }

        public void Dispose()
        {
            _dodge.Dispose();
            _parry.Dispose();
        }
    }
}
