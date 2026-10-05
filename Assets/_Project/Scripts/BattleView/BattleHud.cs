using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using StreetMythos.Battle;

namespace StreetMythos.BattleView
{
    // Interface de combat (UI Toolkit) : timeline, barres, menu d'actions, consignes et chiffres flottants
    public sealed class BattleHud
    {
        readonly VisualElement _timeline, _party, _foes, _menu, _floats;
        readonly Label _hint, _banner;
        readonly Dictionary<BattleUnit, VisualElement> _rows = new Dictionary<BattleUnit, VisualElement>();

        public BattleHud(VisualElement root)
        {
            _timeline = root.Q("timeline");
            _party = root.Q("party");
            _foes = root.Q("foes");
            _menu = root.Q("menu");
            _floats = root.Q("floats");
            _hint = root.Q<Label>("hint");
            _banner = root.Q<Label>("banner");
        }

        public static string Short(BattleUnit u) { var w = u.Def.Name.Split(' ')[0]; return w.Length > 7 ? w.Substring(0, 6) + '.' : w; }

        public void Build(BattleModel model)
        {
            foreach (var u in model.Units)
            {
                var row = new VisualElement();
                row.AddToClassList("unit-row");
                var name = new Label(u.Def.Name);
                name.AddToClassList("unit-name");
                row.Add(name);
                row.Add(Bar("hp", "hp"));
                row.Add(Bar(u.Team == Team.Heroes ? "flow" : "moral", "res"));
                (u.Team == Team.Heroes ? _party : _foes).Add(row);
                _rows[u] = row;
            }
        }

        static VisualElement Bar(string kind, string name)
        {
            var line = new VisualElement { name = name };
            line.AddToClassList("bars");
            var bar = new VisualElement();
            bar.AddToClassList("bar");
            bar.AddToClassList(kind);
            var fill = new VisualElement { name = "fill" };
            fill.AddToClassList("bar-fill");
            bar.Add(fill);
            var text = new Label { name = "text" };
            text.AddToClassList("bar-text");
            line.Add(bar);
            line.Add(text);
            return line;
        }

        static void SetBar(VisualElement line, float ratio, string text)
        {
            line.Q("fill").style.width = Length.Percent(Mathf.Clamp01(ratio) * 100f);
            line.Q<Label>("text").text = text;
        }

        public void Refresh(BattleModel model)
        {
            foreach (var (u, row) in _rows.Select(kv => (kv.Key, kv.Value)))
            {
                SetBar(row.Q("hp"), u.Hp / (float)u.Def.Stats.MaxHp, $"PV {u.Hp}");
                if (u.Team == Team.Heroes) SetBar(row.Q("res"), u.Flow / (float)Rules.MaxFlow, $"Flow {u.Flow}");
                else SetBar(row.Q("res"), u.Moral / (float)Rules.MaxMoral, u.IsDestabilized ? "Déstabilisé !" : $"Moral {u.Moral}");
                row.EnableInClassList("active", u == model.Current);
                row.EnableInClassList("down", !u.IsAlive);
            }

            _timeline.Query(className: "tl-slot").ForEach(e => e.RemoveFromHierarchy());
            var preview = model.PreviewTimeline();
            for (int i = 0; i < preview.Count; i++)
            {
                var slot = new Label(Short(preview[i]));
                slot.AddToClassList("tl-slot");
                slot.AddToClassList(preview[i].Team == Team.Heroes ? "tl-hero" : "tl-enemy");
                if (i == 0) slot.AddToClassList("tl-current");
                _timeline.Add(slot);
            }
        }

        // ---------- Menus ----------

        public sealed class Option
        {
            public string Label;
            public bool Enabled = true;
            public Action OnPick;
            public bool IsBack;
        }

        public void ShowMenu(string title, IEnumerable<Option> options)
        {
            _menu.Clear();
            var t = new Label(title);
            t.AddToClassList("menu-title");
            _menu.Add(t);
            foreach (var o in options)
            {
                var b = new Button(() => o.OnPick?.Invoke()) { text = o.Label };
                b.SetEnabled(o.Enabled);
                if (o.IsBack) b.AddToClassList("back");
                _menu.Add(b);
            }
            _menu.RemoveFromClassList("hidden");
        }

        public void HideMenu() => _menu.AddToClassList("hidden");

        public void ShowHint(string text)
        {
            _hint.text = text;
            _hint.RemoveFromClassList("hidden");
        }

        public void HideHint() => _hint.AddToClassList("hidden");

        public void ShowBanner(string text)
        {
            _banner.text = text;
            _banner.RemoveFromClassList("hidden");
        }

        public void HideBanner() => _banner.AddToClassList("hidden");

        // Chiffre ou mot qui s'élève au-dessus d'un combattant
        public void Float(Vector3 world, string text, string style, MonoBehaviour host, float duration = 1f)
        {
            var cam = Camera.main;
            if (cam == null || _floats.panel == null) return;
            var label = new Label(text);
            label.AddToClassList("float");
            if (!string.IsNullOrEmpty(style)) label.AddToClassList(style);
            _floats.Add(label);
            Vector2 p = RuntimePanelUtils.CameraTransformWorldToPanel(_floats.panel, world, cam);
            label.style.left = p.x - 30;
            label.style.top = p.y;
            host.StartCoroutine(Rise(label, p, duration));
        }

        static System.Collections.IEnumerator Rise(Label label, Vector2 p, float duration)
        {
            for (float e = 0; e < duration; e += Time.unscaledDeltaTime)
            {
                float t = e / duration;
                label.style.top = p.y - 60f * t;
                label.style.opacity = 1f - Mathf.Max(0, t - 0.6f) / 0.4f;
                yield return null;
            }
            label.RemoveFromHierarchy();
        }
    }
}
