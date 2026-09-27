using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level02.Tests
{
    public sealed class Level02SceneTests
    {
        private const string ScenePath = "Assets/_Stella/Scenes/Level02_RuinsBoard.unity";

        [Test]
        public void SceneContainsControllerAndExactlyFortyNineBoardButtons()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Level02Controller controller = Object.FindObjectOfType<Level02Controller>();
            GameObject grid = GameObject.Find("BoardGrid_7x7");

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.CellViews.Count, Is.EqualTo(49));
            Assert.That(grid, Is.Not.Null);
            Assert.That(grid.transform.childCount, Is.EqualTo(49));
            Assert.That(grid.GetComponent<GridLayoutGroup>().constraintCount, Is.EqualTo(7));
        }

        [Test]
        public void SceneRestoresFullScreenStartingChoiceAndLocksBoard()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Level02Controller controller = Object.FindObjectOfType<Level02Controller>();
            GameObject choice = GameObject.Find("StartingPlayerChoice");
            BoardCellView centre = GameObject.Find("Cell_3_3").GetComponent<BoardCellView>();

            Assert.That(choice, Is.Not.Null);
            Assert.That(choice.activeSelf, Is.True);
            Assert.That(choice.transform.parent.name, Is.EqualTo("Level02Canvas"));
            Assert.That(choice.GetComponent<RectTransform>().anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(choice.GetComponent<RectTransform>().anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(GameObject.Find("StartingChoiceCard").GetComponent<RectTransform>().sizeDelta,
                Is.EqualTo(new Vector2(940f, 320f)));
            Assert.That(GameObject.Find("StellaStartsButton"), Is.Not.Null);
            Assert.That(GameObject.Find("KeeperStartsButton"), Is.Not.Null);
            Assert.That(FindAny("ContextActionButton").activeSelf, Is.False);
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(centre.GetComponentInChildren<Text>().text, Is.EqualTo("X"));
            Assert.That(centre.InputEnabled, Is.False);
        }

        [Test]
        public void SceneUsesSingleLeftPanelAndDominantSquareBoard()
        {
            EditorSceneManager.OpenScene(ScenePath);
            RectTransform gameplay = GameObject.Find("GameplayRegion").GetComponent<RectTransform>();
            RectTransform narrative = GameObject.Find("NarrativePanel").GetComponent<RectTransform>();
            RectTransform board = GameObject.Find("BoardRegion").GetComponent<RectTransform>();
            RectTransform grid = GameObject.Find("BoardGrid_7x7").GetComponent<RectTransform>();
            RectTransform battleCard = GameObject.Find("BattleCard").GetComponent<RectTransform>();
            CanvasScaler scaler = Object.FindObjectOfType<CanvasScaler>();
            Level02Controller controller = Object.FindObjectOfType<Level02Controller>();

            Canvas.ForceUpdateCanvases();
            Assert.That(FindAny("TopStatusBar_Thin"), Is.Null);
            Assert.That(gameplay.anchorMax.y - gameplay.anchorMin.y, Is.GreaterThan(0.90f));
            Assert.That(narrative.position.x, Is.LessThan(board.position.x));
            Assert.That(narrative.rect.width / gameplay.rect.width, Is.InRange(0.25f, 0.28f));
            Assert.That(battleCard.sizeDelta.x, Is.EqualTo(battleCard.sizeDelta.y).Within(0.01f));
            Assert.That(battleCard.sizeDelta, Is.EqualTo(new Vector2(220f, 220f)));
            Assert.That(grid.sizeDelta.x, Is.EqualTo(grid.sizeDelta.y).Within(0.01f));
            Assert.That(grid.sizeDelta.x, Is.GreaterThanOrEqualTo(770f));
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1600f, 900f)));
            Assert.That(controller.AlienThinkingDelaySeconds, Is.GreaterThan(0f));
            Assert.That(controller.AlienPreviewDelaySeconds, Is.GreaterThan(0f));
        }

        [Test]
        public void SceneOmitsLegacyInstructionAndResultUi()
        {
            EditorSceneManager.OpenScene(ScenePath);

            Assert.That(FindAny("LevelTitle"), Is.Null);
            Assert.That(GameObject.Find("DialogueBox"), Is.Not.Null);
            Assert.That(FindAny("StatusText"), Is.Null);
            Assert.That(GameObject.Find("BattleCard"), Is.Not.Null);
            Assert.That(FindAny("ContextActionButton"), Is.Not.Null);
            Assert.That(FindAny("OrderText"), Is.Null);
            Assert.That(FindAny("SelectionText"), Is.Null);
            Assert.That(FindAny("FeedbackText"), Is.Null);
            Assert.That(FindAny("ClearButton"), Is.Null);
            Assert.That(FindAny("ResultOverlay"), Is.Null);
        }

        private static GameObject FindAny(string name)
        {
            foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
                if (candidate.scene.IsValid() && candidate.name == name) return candidate;
            return null;
        }
    }
}
