using System;
using System.Collections.Generic;
using System.Linq;
using StreetMythos.Battle;
using UnityEngine;

namespace StreetMythos.Data
{
    // Formes JSON de Data/Json (JsonUtility : champs publics, énumérations en texte)
    [Serializable] public class SkillJson
    {
        public string id, name, target = "SingleEnemy";
        public int delay = 120, flowCost = 2, power, hits = 1, moralDamage, healPercent, flowGrant,
                   pushBack, tauntTurns, defendTurns, stunIfMoralBelow, teamSpdBuffPercent, buffTurns;
    }
    [Serializable] public class MoveJson { public string id, name; public int delay = 100, power = 100, hits = 1; public bool targetsAll; }
    [Serializable] public class HeroJson { public string id, name; public bool ines; public int hp, atk, def, spd; public string[] skills; public int[] skillLevels; }
    [Serializable] public class EnemyJson
    {
        public string id, name, weakness, resistance;
        public bool boss, elite;
        public int hp, atk, def, spd, xp, balles;
        public MoveJson[] moves;
    }
    [Serializable] public class EncounterJson { public string id, name; public string[] enemies; public bool tutorial, elite, boss; }

    [Serializable] class SkillFile { public SkillJson[] skills; }
    [Serializable] class HeroFile { public HeroJson[] heroes; }
    [Serializable] class EnemyFile { public EnemyJson[] enemies; }
    [Serializable] class EncounterFile { public EncounterJson[] encounters; }

    // Données de jeu chargées depuis le JSON, converties en définitions pour le modèle de combat
    public sealed class GameData
    {
        public const float StatGrowthPerLevel = 0.08f; // +8 % de PV, ATQ et DEF par niveau

        public readonly Dictionary<string, SkillJson> Skills;
        public readonly Dictionary<string, HeroJson> Heroes;
        public readonly Dictionary<string, EnemyJson> Enemies;
        public readonly Dictionary<string, EncounterJson> Encounters;

        public GameData(string skillsJson, string heroesJson, string enemiesJson, string encountersJson)
        {
            Skills = JsonUtility.FromJson<SkillFile>(skillsJson).skills.ToDictionary(s => s.id);
            Heroes = JsonUtility.FromJson<HeroFile>(heroesJson).heroes.ToDictionary(h => h.id);
            Enemies = JsonUtility.FromJson<EnemyFile>(enemiesJson).enemies.ToDictionary(e => e.id);
            Encounters = JsonUtility.FromJson<EncounterFile>(encountersJson).encounters.ToDictionary(e => e.id);
        }

        // Liste des incohérences (références cassées, valeurs hors bornes) ; vide si tout va bien
        public List<string> Validate()
        {
            var errors = new List<string>();
            foreach (var h in Heroes.Values)
            {
                if (h.skills == null || h.skillLevels == null || h.skills.Length != h.skillLevels.Length)
                    errors.Add($"{h.id} : skills et skillLevels doivent avoir la même longueur");
                else foreach (var s in h.skills) if (!Skills.ContainsKey(s)) errors.Add($"{h.id} : compétence inconnue {s}");
                if (h.spd <= 0) errors.Add($"{h.id} : VIT nulle");
            }
            foreach (var s in Skills.Values)
            {
                if (!Enum.TryParse<TargetKind>(s.target, out _)) errors.Add($"{s.id} : cible inconnue {s.target}");
                if (s.flowCost < 0 || s.flowCost > Rules.MaxFlow) errors.Add($"{s.id} : coût de Flow hors bornes");
            }
            foreach (var e in Enemies.Values)
            {
                if (e.moves == null || e.moves.Length == 0) errors.Add($"{e.id} : aucune attaque");
                if (!string.IsNullOrEmpty(e.weakness) && !Enum.TryParse<VanneType>(e.weakness, out _)) errors.Add($"{e.id} : faiblesse inconnue");
                if (!string.IsNullOrEmpty(e.resistance) && !Enum.TryParse<VanneType>(e.resistance, out _)) errors.Add($"{e.id} : résistance inconnue");
                if (e.weakness == e.resistance && !string.IsNullOrEmpty(e.weakness)) errors.Add($"{e.id} : faiblesse et résistance identiques");
                if (e.spd <= 0) errors.Add($"{e.id} : VIT nulle");
            }
            foreach (var enc in Encounters.Values)
            {
                if (enc.enemies == null || enc.enemies.Length is 0 or > 4) errors.Add($"{enc.id} : 1 à 4 ennemis attendus");
                else foreach (var id in enc.enemies) if (!Enemies.ContainsKey(id)) errors.Add($"{enc.id} : ennemi inconnu {id}");
            }
            return errors;
        }

        public static SkillDef ToSkill(SkillJson s) => new SkillDef
        {
            Id = s.id, Name = s.name, Delay = s.delay, FlowCost = s.flowCost,
            Target = (TargetKind)Enum.Parse(typeof(TargetKind), s.target),
            Power = s.power, Hits = s.hits, MoralDamage = s.moralDamage, HealPercent = s.healPercent,
            FlowGrant = s.flowGrant, PushBack = s.pushBack, TauntTurns = s.tauntTurns, DefendTurns = s.defendTurns,
            StunIfMoralBelow = s.stunIfMoralBelow, TeamSpdBuffPercent = s.teamSpdBuffPercent, BuffTurns = s.buffTurns,
        };

        static int Grow(int value, int level) => (int)Math.Round(value * (1f + StatGrowthPerLevel * (level - 1)));

        public UnitDef BuildHero(string id, int level)
        {
            var h = Heroes[id];
            var def = new UnitDef
            {
                Id = h.id, Name = h.name, Team = Team.Heroes, IsInes = h.ines,
                Stats = new Stats(Grow(h.hp, level), Grow(h.atk, level), Grow(h.def, level), h.spd),
            };
            for (int i = 0; i < h.skills.Length; i++)
                if (level >= h.skillLevels[i]) def.Skills.Add(ToSkill(Skills[h.skills[i]]));
            return def;
        }

        public UnitDef BuildEnemy(string id, int instance = 0)
        {
            var e = Enemies[id];
            var def = new UnitDef
            {
                Id = instance == 0 ? e.id : $"{e.id}_{instance + 1}", Name = e.name, Team = Team.Enemies,
                IsBoss = e.boss, IsElite = e.elite,
                Stats = new Stats(e.hp, e.atk, e.def, e.spd),
                Weakness = string.IsNullOrEmpty(e.weakness) ? (VanneType?)null : (VanneType)Enum.Parse(typeof(VanneType), e.weakness),
                Resistance = string.IsNullOrEmpty(e.resistance) ? (VanneType?)null : (VanneType)Enum.Parse(typeof(VanneType), e.resistance),
            };
            foreach (var m in e.moves)
                def.Moves.Add(new EnemyMove { Id = m.id, Name = m.name, Delay = m.delay, Power = m.power, Hits = m.hits, TargetsAll = m.targetsAll });
            return def;
        }

        public List<UnitDef> BuildEncounter(string id) =>
            Encounters[id].enemies.Select((e, i) => BuildEnemy(e, i)).ToList();
    }
}
