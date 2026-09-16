using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Stella.Level04.Tests
{
    public sealed class Level04SceneTests
    {
        private const string ScenePath = "Assets/_Stella/Scenes/Level04_KrikGreeting.unity";

        [Test]
        public void SceneContainsStableTwentyCrystalLayoutAndOneBattleCard()
        {
            EditorSceneManager.OpenScene(ScenePath);

            Level04Controller controller = Object.FindObjectOfType<Level04Controller>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.CrystalViews.Count, Is.EqualTo(20));

            GameObject grid = GameObject.Find("CrystalGrid_2x10");
            Assert.That(grid, Is.Not.Null);
            Assert.That(grid.transform.childCount, Is.EqualTo(20));
            Assert.That(GameObject.Find("ActiveBattleCard"), Is.Not.Null);
            Assert.That(GameObject.Find("DialoguePanel"), Is.Not.Null);
            Assert.That(GameObject.Find("StellaBattleCard"), Is.Null);
            Assert.That(GameObject.Find("KrikBattleCard"), Is.Null);
        }

        [Test]
        public void SceneUsesLandscapeThirtySeventyResponsiveRegions()
        {
            EditorSceneManager.OpenScene(ScenePath);

            RectTransform top = GameObject.Find("TopRegion_30Percent").GetComponent<RectTransform>();
            RectTransform bottom = GameObject.Find("BottomRegion_70Percent").GetComponent<RectTransform>();

            Assert.That(top.anchorMin.y, Is.EqualTo(0.68f).Within(0.001f));
            Assert.That(top.anchorMax.y, Is.EqualTo(0.96f).Within(0.001f));
            Assert.That(bottom.anchorMin.y, Is.EqualTo(0.04f).Within(0.001f));
            Assert.That(bottom.anchorMax.y, Is.EqualTo(0.65f).Within(0.001f));
        }
    }
}
