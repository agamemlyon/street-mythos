using UnityEngine;
using UnityEngine.Profiling;

namespace StreetMythos.Core
{
    // Affiche images par seconde et mémoire (debug J0, retiré du build public)
    public sealed class PerfOverlay : MonoBehaviour
    {
        float _smoothDt = 1f / 60f;
        GUIStyle _style;

        void Update() => _smoothDt = Mathf.Lerp(_smoothDt, Time.unscaledDeltaTime, 0.05f);

        void OnGUI()
        {
            if (!Debug.isDebugBuild) return;
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 18 };
            long mb = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            GUI.Label(new Rect(10, 10, 400, 30), $"{1f / _smoothDt:0} ips · {mb} Mo", _style);
        }
    }
}
