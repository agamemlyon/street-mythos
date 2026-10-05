using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Compile chaque .ink de Dialogues en .json (TextAsset lu par le jeu). Lancé par la chaîne locale,
    // pour ne pas dépendre de la compilation automatique de l'éditeur (absente en batchmode).
    public static class InkBuild
    {
        const string Dir = "Assets/_Project/Dialogues";

        [MenuItem("Street Mythos/Compiler les dialogues Ink")]
        public static void CompileAll()
        {
            int errors = 0;
            foreach (var path in Directory.GetFiles(Dir, "*.ink"))
            {
                var issues = new List<string>();
                var compiler = new Ink.Compiler(File.ReadAllText(path), new Ink.Compiler.Options
                {
                    sourceFilename = Path.GetFileName(path),
                    countAllVisits = true,
                    errorHandler = (msg, type) => { if (type != Ink.ErrorType.Author) issues.Add($"{type} : {msg}"); },
                });
                var story = compiler.Compile();
                foreach (var i in issues) Debug.LogError($"[Ink] {path} {i}");
                if (story == null || issues.Exists(i => i.StartsWith("Error"))) { errors++; continue; }
                File.WriteAllText(Path.ChangeExtension(path, ".json"), story.ToJson());
                Debug.Log($"[Ink] {Path.GetFileName(path)} compilé");
            }
            AssetDatabase.Refresh();
            if (Application.isBatchMode && errors > 0) EditorApplication.Exit(1);
        }
    }
}
