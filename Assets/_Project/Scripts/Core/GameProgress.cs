using System;
using System.Collections.Generic;
using UnityEngine;

namespace StreetMythos.Core
{
    // Progression de la partie : position, combats gagnés, drapeaux d'histoire, niveau.
    // Sauvegarde JSON versionnée dans PlayerPrefs (IndexedDB sur le web), un seul emplacement (TECH_DESIGN 4.3).
    [Serializable]
    public sealed class GameProgress
    {
        public const int Version = 1;
        const string Key = "street_mythos_save";

        public int version = Version;
        public string scene = "Q1_CroixRousse";
        public float px, py, pz, yaw;
        public bool hasPosition;
        public int level = 1;
        public int xp;
        public int balles;
        public int reput;
        public List<string> defeated = new List<string>();
        public List<string> flags = new List<string>();

        public static GameProgress Current { get; private set; } = new GameProgress();

        // XP pour passer du niveau n au niveau n+1
        public static int XpToNext(int level) => 40 + level * 30;
        public const int MaxLevel = 10;

        public bool Has(string flag) => flags.Contains(flag);
        public void Set(string flag) { if (!flags.Contains(flag)) flags.Add(flag); }
        public bool IsDefeated(string zone) => defeated.Contains(zone);

        // Ajoute de l'XP ; renvoie le nombre de niveaux gagnés
        public int GainXp(int amount)
        {
            int ups = 0;
            xp += amount;
            while (level < MaxLevel && xp >= XpToNext(level))
            {
                xp -= XpToNext(level);
                level++;
                ups++;
            }
            return ups;
        }

        public void SetPosition(Vector3 p, float y)
        {
            px = p.x; py = p.y; pz = p.z; yaw = y;
            hasPosition = true;
        }

        public Vector3 Position => new Vector3(px, py, pz);

        public static bool HasSave
        {
            get { try { return PlayerPrefs.HasKey(Key); } catch { return false; } }
        }

        public static void NewGame() => Current = new GameProgress();

        public static void Save()
        {
            try
            {
                PlayerPrefs.SetString(Key, JsonUtility.ToJson(Current));
                PlayerPrefs.Save();
            }
            catch (Exception e) { Debug.LogWarning($"[Sauvegarde] impossible : {e.Message}"); }
        }

        public static bool Load()
        {
            try
            {
                if (!PlayerPrefs.HasKey(Key)) return false;
                var p = JsonUtility.FromJson<GameProgress>(PlayerPrefs.GetString(Key));
                if (p == null || p.version > Version) return false; // sauvegarde d'une version future : ignorée
                Current = p;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Sauvegarde] illisible : {e.Message}");
                return false;
            }
        }

        public static GameProgress FromJson(string json) => JsonUtility.FromJson<GameProgress>(json);
        public string ToJson() => JsonUtility.ToJson(this);
    }

    // Demande de combat transmise de l'exploration à l'arène, puis le résultat en retour
    public static class BattleRequest
    {
        public static string EncounterId;
        public static string ZoneId;          // zone de la carte à marquer vaincue
        public static int Opening;            // 0 normal, 1 héros d'abord, 2 ennemis d'abord
        public static string ReturnScene;
        public static bool Pending => !string.IsNullOrEmpty(EncounterId);

        public static void Clear()
        {
            EncounterId = ZoneId = ReturnScene = null;
            Opening = 0;
        }
    }
}
