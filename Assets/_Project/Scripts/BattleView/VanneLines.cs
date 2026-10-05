using System.Text.RegularExpressions;
using StreetMythos.Battle;
using UnityEngine;

namespace StreetMythos.BattleView
{
    // Textes de tchatche tirés du fichier Ink compilé (Dialogues/vannes.json)
    public sealed class VanneLines
    {
        readonly Ink.Runtime.Story _story;

        public VanneLines(TextAsset compiledInk)
        {
            if (compiledInk != null) _story = new Ink.Runtime.Story(compiledInk.text);
        }

        public string Vanne(BattleUnit hero, VanneType type) => Knot($"{hero.Def.Id}_{type.ToString().ToLowerInvariant()}");

        // Réplique de l'ennemi selon le résultat (efficace, neutre, ratee)
        public string Reply(BattleUnit enemy, string result)
        {
            string baseId = Regex.Replace(enemy.Def.Id, @"_\d+$", "");
            string r = result.Replace("é", "e");
            return Knot($"{baseId}_{r}");
        }

        string Knot(string name)
        {
            if (_story == null) return null;
            try
            {
                _story.ChoosePathString(name);
                return _story.ContinueMaximally().Trim();
            }
            catch (System.Exception)
            {
                return null; // nœud absent : pas de texte, le combat continue
            }
        }
    }
}
