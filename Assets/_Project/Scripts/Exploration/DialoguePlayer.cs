using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using StreetMythos.Core;

namespace StreetMythos.Exploration
{
    // Joue un nœud Ink dans une boîte de dialogue (UI Toolkit). Tags reconnus :
    // # speaker:<id>, # combat:<rencontre>. Les variables Ink « reput » et « vu_guignol » sont reportées dans la sauvegarde.
    public sealed class DialoguePlayer
    {
        static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { "yanis", "Yanis" }, { "ines", "Inès" }, { "momo", "Momo" }, { "guignol", "Guignol" },
            { "gros_caillou", "Gros Caillou" }, { "tonton_jojo", "Tonton Jojo" },
        };

        readonly Ink.Runtime.Story _story;
        readonly VisualElement _box, _choices;
        readonly Label _speaker, _text;
        readonly InputAction _next;
        int _picked = -1;

        public bool Playing { get; private set; }

        public DialoguePlayer(TextAsset compiled, VisualElement root)
        {
            _story = new Ink.Runtime.Story(compiled.text);
            _box = root.Q("dialogue");
            _speaker = root.Q<Label>("speaker");
            _text = root.Q<Label>("line");
            _choices = root.Q("choices");
            _next = new InputAction("Suite", InputActionType.Button);
            _next.AddBinding("<Keyboard>/space");
            _next.AddBinding("<Keyboard>/e");
            _next.AddBinding("<Keyboard>/enter");
            _next.AddBinding("<Mouse>/leftButton");
            _next.AddBinding("<Gamepad>/buttonSouth");
            _next.Enable();
        }

        // Joue le nœud ; onCombat est appelé si un tag de combat apparaît (le dialogue s'arrête alors)
        public IEnumerator Play(string knot, Action<string> onCombat)
        {
            Playing = true;
            _box.RemoveFromClassList("hidden");
            _story.ChoosePathString(knot);
            string combat = null;
            while (true)
            {
                while (_story.canContinue)
                {
                    string line = _story.Continue().Trim();
                    foreach (var tag in _story.currentTags)
                    {
                        var t = tag.Trim();
                        if (t.StartsWith("speaker:")) _speaker.text = Names.TryGetValue(t.Substring(8), out var n) ? n : t.Substring(8);
                        if (t.StartsWith("combat:")) combat = t.Substring(7);
                    }
                    if (line.Length == 0) continue;
                    _text.text = line;
                    yield return null; // l'appui qui a ouvert le dialogue ne doit pas sauter la première ligne
                    while (!_next.WasPressedThisFrame()) yield return null;
                }
                if (_story.currentChoices.Count == 0) break;

                _picked = -1;
                _choices.Clear();
                foreach (var c in _story.currentChoices)
                {
                    int i = c.index;
                    _choices.Add(new Button(() => _picked = i) { text = c.text });
                }
                while (_picked < 0) yield return null;
                _choices.Clear();
                _story.ChooseChoiceIndex(_picked);
            }

            var p = GameProgress.Current;
            p.reput = (int)_story.variablesState["reput"];
            if ((bool)_story.variablesState["vu_guignol"]) p.Set("vu_guignol");
            _box.AddToClassList("hidden");
            Playing = false;
            if (combat != null) onCombat?.Invoke(combat);
        }

        public void Dispose() => _next.Dispose();
    }
}
