using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace StreetMythos.Tests.EditMode
{
    // Lignes rouges (SPEC § 3.6) : aucun mot interdit dans les dialogues ni dans les données
    public class TextContentTests
    {
        static string Fold(string s)
        {
            var d = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in d)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(char.ToLowerInvariant(c));
            return sb.ToString();
        }

        static string[] Forbidden() =>
            File.ReadAllLines(Path.Combine(Application.dataPath, "_Project/Data/forbidden_words.txt"))
                .Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("#")).Select(Fold).ToArray();

        static string[] TextFiles() =>
            Directory.GetFiles(Path.Combine(Application.dataPath, "_Project/Dialogues"), "*.ink", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Path.Combine(Application.dataPath, "_Project/Data/Json"), "*.json"))
                .ToArray();

        [Test]
        public void NoForbiddenWordInGameTexts()
        {
            var words = Forbidden();
            Assert.Greater(words.Length, 10);
            var hits = (from f in TextFiles()
                        let text = Fold(File.ReadAllText(f))
                        from w in words
                        where Regex.IsMatch(text, $@"(?<![\p{{L}}]){Regex.Escape(w)}(?![\p{{L}}])")
                        select $"{Path.GetFileName(f)} : « {w} »").ToList();
            CollectionAssert.IsEmpty(hits);
        }

        [Test]
        public void TwentySevenVannesExist()
        {
            string ink = File.ReadAllText(Path.Combine(Application.dataPath, "_Project/Dialogues/vannes.ink"));
            int count = 0;
            foreach (var hero in new[] { "yanis", "ines", "momo" })
                foreach (var type in new[] { "clash", "chambrage", "mytho" })
                {
                    var m = Regex.Match(ink, $@"=== {hero}_{type} ===\s*\{{~(.*?)\}}", RegexOptions.Singleline);
                    Assert.IsTrue(m.Success, $"{hero}_{type} absent");
                    count += m.Groups[1].Value.Split('|').Length;
                }
            Assert.AreEqual(27, count, "3 vannes par héros et par type (SPEC § 4.5)");
        }

        [Test]
        public void EveryEnemyHasVanneReplies()
        {
            string ink = File.ReadAllText(Path.Combine(Application.dataPath, "_Project/Dialogues/vannes.ink"));
            foreach (var enemy in new[] { "pigeon_possede", "brumeux", "lion_de_pierre", "controleur_fantome", "gros_caillou" })
                foreach (var result in new[] { "efficace", "neutre", "ratee" })
                    StringAssert.Contains($"=== {enemy}_{result} ===", ink);
        }
    }
}
