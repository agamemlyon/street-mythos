using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StreetMythos.Editor
{
    // Build Web en ligne de commande :
    // Unity -batchmode -quit -projectPath <clone-build> -executeMethod StreetMythos.Editor.BuildScript.BuildWeb [-devBuild]
    public static class BuildScript
    {
        const string OutputDir = "Builds/Web";

        [MenuItem("Street Mythos/Build Web")]
        public static void BuildWeb()
        {
            ProjectConfigurator.ConfigureWeb();

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) throw new Exception("Aucune scène dans les Build Settings");

            bool dev = Environment.GetCommandLineArgs().Contains("-devBuild");
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputDir,
                target = BuildTarget.WebGL,
                options = dev ? BuildOptions.Development : BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            Debug.Log($"[Build] {s.result} · {s.totalSize / (1024f * 1024f):0.0} Mo · {s.totalTime.TotalSeconds:0} s · {s.totalErrors} erreurs");
            if (s.result == BuildResult.Succeeded) WriteSizeReport();

            if (Application.isBatchMode)
                EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }

        // Taille réelle des fichiers livrés, comparée au budget de démarrage (25 Mo)
        static void WriteSizeReport()
        {
            // Les vidéos (StreamingAssets) sont lues à la demande : hors budget de démarrage
            var files = Directory.GetFiles(OutputDir, "*", SearchOption.AllDirectories).Where(f => !f.Replace('\\', '/').Contains("/StreamingAssets/")).ToArray();
            long total = files.Sum(f => new FileInfo(f).Length);
            var lines = files.Select(f => $"{new FileInfo(f).Length / 1024,10} Ko  {f.Replace('\\', '/')}").ToList();
            lines.Add($"TOTAL {total / (1024f * 1024f):0.00} Mo (budget démarrage : 25 Mo)");
            File.WriteAllLines("Builds/web-size.txt", lines);
        }
    }
}
