using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Stella.Level01.Tests
{
    public sealed class Level01SceneTests
    {
        private const string ScenePath = "Assets/_Stella/Scenes/Level01_DoubleLock.unity";

        [Test]
        public void SceneContainsTheFixedLevel1ControlSet()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Level01Controller controller = Object.FindObjectOfType<Level01Controller>();

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.HistoryRows.Count, Is.EqualTo(6));
            Assert.That(controller.SlotButtons.Count, Is.EqualTo(4));
            Assert.That(controller.ColorButtons.Count, Is.EqualTo(6));
            Assert.That(controller.AttemptPips.Count, Is.EqualTo(6));
            Assert.That(GameObject.Find("Lock1Label"), Is.Not.Null);
            Assert.That(GameObject.Find("Lock2Label"), Is.Not.Null);
            Assert.That(GameObject.Find("AttemptLights").transform.parent.name, Is.EqualTo("TopRegion"));
        }

        [Test]
        public void SceneStartsOnLockOneWithSubmitDisabledAndEmptyHistory()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Level01Controller controller = Object.FindObjectOfType<Level01Controller>();

            Assert.That(GameObject.Find("SubmitButton").GetComponent<Button>().interactable, Is.False);
            Assert.That(GameObject.Find("HistoryRow_1").GetComponentInChildren<Text>().text, Is.EqualTo("—"));
            Assert.That(FindAny("RetryPanel").activeSelf, Is.False);
        }

        [Test]
        public void SceneKeepsFeedbackLegendAndDoesNotCreatePerSlotFeedbackRows()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Assert.That(GameObject.Find("Legend"), Is.Not.Null);
            Assert.That(GameObject.Find("HistoryRows").transform.childCount, Is.EqualTo(6));
            Assert.That(GameObject.Find("TopRegion"), Is.Not.Null);
            Assert.That(GameObject.Find("StatusRegion"), Is.Not.Null);
        }

        [Test]
        public void ControllerAllowsRepeatedColourDraftsAndClearOnlyResetsDraft()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Level01Controller controller = Object.FindObjectOfType<Level01Controller>();
            DoubleLockGame game = new DoubleLockGame(new System.Random(0));
            game.SetSecretForTests(
                DoubleLockColor.Red,
                DoubleLockColor.Blue,
                DoubleLockColor.Green,
                DoubleLockColor.Yellow);
            controller.SetGameForTests(game);

            controller.HandleColorPressed(0);
            controller.HandleColorPressed(0);
            controller.HandleColorPressed(0);
            controller.HandleColorPressed(0);
            Assert.That(GameObject.Find("SubmitButton").GetComponent<Button>().interactable, Is.True);

            controller.ClearGuess();
            Assert.That(GameObject.Find("SubmitButton").GetComponent<Button>().interactable, Is.False);
            Assert.That(game.AttemptsUsed, Is.Zero);
        }

        [Test]
        public void SceneButtonsDriveDraftReplacementClearAndSubmit()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Level01Controller controller = Object.FindObjectOfType<Level01Controller>();
            DoubleLockGame game = new DoubleLockGame(new System.Random(0));
            game.SetSecretForTests(
                DoubleLockColor.Green,
                DoubleLockColor.Green,
                DoubleLockColor.Green,
                DoubleLockColor.Green);
            controller.InitializeControls();
            controller.SetGameForTests(game);

            Button red = GameObject.Find("ColourButton_RED").GetComponent<Button>();
            Button blue = GameObject.Find("ColourButton_BLUE").GetComponent<Button>();
            Button yellow = GameObject.Find("ColourButton_YELLOW").GetComponent<Button>();
            Button clear = GameObject.Find("ClearButton").GetComponent<Button>();
            Button submit = GameObject.Find("SubmitButton").GetComponent<Button>();

            PointerClick(red);
            PointerClick(blue);
            PointerClick(red);
            PointerClick(blue);
            Assert.That(controller.DraftSlotForTests(0), Is.EqualTo(DoubleLockColor.Red));
            Assert.That(controller.DraftSlotForTests(1), Is.EqualTo(DoubleLockColor.Blue));
            Assert.That(controller.DraftSlotForTests(2), Is.EqualTo(DoubleLockColor.Red));
            Assert.That(controller.DraftSlotForTests(3), Is.EqualTo(DoubleLockColor.Blue));
            Assert.That(submit.interactable, Is.True);

            PointerClick(controller.SlotButtons[0]);
            PointerClick(yellow);
            Assert.That(controller.DraftSlotForTests(0), Is.EqualTo(DoubleLockColor.Yellow));
            Assert.That(controller.SelectedSlotForTests, Is.EqualTo(-1));

            PointerClick(red);
            Assert.That(controller.DraftSlotForTests(0), Is.EqualTo(DoubleLockColor.Yellow), "A full draft must not be overwritten without an explicit slot selection.");

            PointerClick(clear);
            Assert.That(controller.DraftSlotForTests(0), Is.Null);
            Assert.That(game.AttemptsUsed, Is.Zero);
            Assert.That(submit.interactable, Is.False);

            PointerClick(red);
            PointerClick(blue);
            PointerClick(red);
            PointerClick(blue);
            PointerClick(submit);
            Assert.That(game.AttemptsUsed, Is.EqualTo(1));
            Assert.That(GameObject.Find("HistoryRow_1").GetComponentInChildren<Text>().text, Is.EqualTo("1"));
        }

        [Test]
        public void SceneUsesOnlyButtonGraphicsAsRaycastTargets()
        {
            EditorSceneManager.OpenScene(ScenePath);
            foreach (Graphic graphic in Object.FindObjectsOfType<Graphic>())
            {
                if (!graphic.raycastTarget) continue;
                Assert.That(graphic.GetComponent<Button>(), Is.Not.Null, graphic.name + " should not intercept pointer input.");
            }
        }

        [Test]
        public void LandscapeLayoutKeepsEveryInteractionSectionInTheMainRegion()
        {
            EditorSceneManager.OpenScene(ScenePath);
            RectTransform top = GameObject.Find("TopRegion").GetComponent<RectTransform>();
            RectTransform main = GameObject.Find("MainRegion").GetComponent<RectTransform>();
            Transform reasoning = GameObject.Find("ReasoningRegion").transform;

            Assert.That(top.anchorMax.y - top.anchorMin.y, Is.InRange(0.12f, 0.16f));
            Assert.That(main.anchorMax.y - main.anchorMin.y, Is.InRange(0.75f, 0.82f));
            Assert.That(GameObject.Find("HistoryRows").transform.IsChildOf(reasoning), Is.True);
            Assert.That(GameObject.Find("ActiveGuessPanel").transform.IsChildOf(reasoning), Is.True);
            Assert.That(GameObject.Find("ColourControls").transform.IsChildOf(reasoning), Is.True);
            Assert.That(GameObject.Find("ActionRow").transform.IsChildOf(reasoning), Is.True);
            Assert.That(GameObject.Find("ColourControls").GetComponent<HorizontalLayoutGroup>(), Is.Not.Null);

            GridLayoutGroup feedback = GameObject.Find("FeedbackGroup").GetComponent<GridLayoutGroup>();
            Assert.That(feedback.constraint, Is.EqualTo(GridLayoutGroup.Constraint.FixedColumnCount));
            Assert.That(feedback.constraintCount, Is.EqualTo(2));
        }

        private static GameObject FindAny(string name)
        {
            foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
                if (candidate.scene.IsValid() && candidate.name == name) return candidate;
            return null;
        }

        private static void PointerClick(Button button)
        {
            EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
            Assert.That(eventSystem, Is.Not.Null);
            PointerEventData pointer = new PointerEventData(eventSystem) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
    }
}
