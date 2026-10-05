using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using StreetMythos.Core;
using StreetMythos.Exploration;

namespace StreetMythos.Tests.PlayMode
{
    // Démarrage du quartier et passage au combat au contact d'un ennemi (TECH_DESIGN § 9)
    public class ExplorationTests
    {
        [UnityTest]
        public IEnumerator TouchingPigeonsStartsTheTutorialFight()
        {
            GameProgress.NewGame();
            GameProgress.Current.Set("intro_vue");          // saute la scène d'introduction
            BattleRequest.Clear();
            SceneManager.LoadScene("Q1_CroixRousse");
            yield return null;
            yield return null;

            // Le menu titre attend un choix : on le contourne en libérant le joueur
            var player = Object.FindFirstObjectByType<PlayerController>();
            Assert.IsNotNull(player, "joueur absent");
            var zone = GameObject.Find("zone_pigeons");
            Assert.IsNotNull(zone, "zone des pigeons absente");
            var title = Object.FindFirstObjectByType<UIDocument>().rootVisualElement.Q("title");
            title?.AddToClassList("hidden");
            player.Frozen = false;

            var start = zone.transform.position;
            yield return new WaitForSeconds(0.5f);
            Assert.AreNotEqual(start, zone.transform.position, "les pigeons ne patrouillent pas");

            // Téléporte le joueur sur les pigeons
            var cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.position = zone.transform.position;
            cc.enabled = true;
            for (int i = 0; i < 10 && !BattleRequest.Pending && SceneManager.GetActiveScene().name != "Arena_Q1"; i++) yield return null;
            Assert.IsTrue(BattleRequest.Pending || SceneManager.GetActiveScene().name == "Arena_Q1", "le combat n'a pas été lancé");
            Assert.AreEqual("q1_tuto_pigeons", BattleRequest.EncounterId);
        }
    }
}
