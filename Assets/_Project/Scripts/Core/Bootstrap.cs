using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreetMythos.Core
{
    // Scène Boot : crée les services persistants puis charge le menu
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] string firstScene = "MainMenu";

        public static GameStateMachine States { get; private set; }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            States = new GameStateMachine();
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            if (Application.CanStreamedLevelBeLoaded(firstScene))
            {
                SceneManager.LoadScene(firstScene);
                States.Enter(GameState.Menu);
            }
        }
    }
}
