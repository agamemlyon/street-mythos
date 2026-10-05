using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Test de déformation du squelette : plie coudes, genoux et torse, puis capture.
    // -executeMethod StreetMythos.Editor.PoseTest.Run
    public static class PoseTest
    {
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/J0_Test.unity");
            var hero = GameObject.Find("Yanis");
            if (hero == null) { Debug.LogError("[Pose] Yanis absent de la scène"); return; }
            Transform B(string n) => hero.GetComponentsInChildren<Transform>().First(t => t.name == n);

            // Garde de boxe : bras ramenés devant, coudes pliés à environ 110°
            B("LeftArm").Rotate(Vector3.up, 70f, Space.World);
            B("LeftArm").Rotate(Vector3.forward, -45f, Space.World);
            B("LeftForeArm").Rotate(Vector3.up, 110f, Space.World);
            B("RightArm").Rotate(Vector3.up, -70f, Space.World);
            B("RightArm").Rotate(Vector3.forward, 45f, Space.World);
            B("RightForeArm").Rotate(Vector3.up, -110f, Space.World);

            // Fente : cuisse gauche levée, genou plié à 90°, jambe droite fléchie
            B("LeftUpLeg").Rotate(Vector3.right, -60f, Space.World);
            B("LeftLeg").Rotate(Vector3.right, 90f, Space.World);
            B("RightUpLeg").Rotate(Vector3.right, 25f, Space.World);
            B("RightLeg").Rotate(Vector3.right, 40f, Space.World);

            // Torse penché et tourné
            B("Spine").Rotate(Vector3.right, 15f, Space.World);
            B("Spine01").Rotate(Vector3.up, 20f, Space.World);

            var cam = Camera.main;
            var key = new GameObject("Key", typeof(Light)).GetComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.2f;
            key.transform.rotation = Quaternion.Euler(25f, 20f, 0);

            var b = hero.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, c) => { a.Encapsulate(c); return a; });
            Shot(cam, b.center + new Vector3(0, 0.1f, -2.6f), b.center, "Builds/pose_face.png");
            Shot(cam, b.center + new Vector3(-2.6f, 0.1f, 0.4f), b.center, "Builds/pose_profil.png");
            Shot(cam, b.center + new Vector3(-1.6f, 0.9f, -1.6f), b.center, "Builds/pose_34.png");
        }

        static void Shot(Camera cam, Vector3 pos, Vector3 target, string path)
        {
            cam.transform.position = pos;
            cam.transform.LookAt(target);
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            Debug.Log($"[Pose] {path}");
        }
    }
}
