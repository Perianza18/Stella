using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level04.Tests
{
    public sealed class Level04ControllerTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private Level04Controller controller;
        private BattlePanelView battlePanel;
        private List<CrystalView> crystals;
        private Button doneButton;
        private GameObject startingPanel;
        private GameObject retryPanel;

        [SetUp]
        public void SetUp()
        {
            GameObject controllerObject = CreateObject("Controller");
            controller = controllerObject.AddComponent<Level04Controller>();

            battlePanel = CreateBattlePanel();
            crystals = CreateCrystals(controller);
            doneButton = CreateButton("Done");
            startingPanel = CreateObject("StartingPanel");
            retryPanel = CreateObject("RetryPanel");

            controller.Configure(
                battlePanel,
                crystals,
                doneButton,
                startingPanel,
                CreateButton("StellaStarts"),
                CreateButton("KrikStarts"),
                retryPanel,
                CreateButton("Retry"));
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                UnityEngine.Object.DestroyImmediate(createdObjects[index]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void StellaCanSelectAndDeselectACrystal()
        {
            controller.ChooseStellaStarts();

            controller.HandleCrystalPressed(crystals[0]);
            Assert.That(controller.SelectedCrystalCount, Is.EqualTo(1));
            Assert.That(crystals[0].State, Is.EqualTo(CrystalVisualState.Selected));
            Assert.That(doneButton.interactable, Is.True);

            controller.HandleCrystalPressed(crystals[0]);
            Assert.That(controller.SelectedCrystalCount, Is.Zero);
            Assert.That(crystals[0].State, Is.EqualTo(CrystalVisualState.Active));
            Assert.That(doneButton.interactable, Is.False);
        }

        [Test]
        public void FourthCrystalSelectionIsPrevented()
        {
            controller.ChooseStellaStarts();

            controller.HandleCrystalPressed(crystals[0]);
            controller.HandleCrystalPressed(crystals[1]);
            controller.HandleCrystalPressed(crystals[2]);
            controller.HandleCrystalPressed(crystals[3]);

            Assert.That(controller.SelectedCrystalCount, Is.EqualTo(3));
            Assert.That(crystals[3].State, Is.EqualTo(CrystalVisualState.Active));
        }

        [Test]
        public void ConfirmingMoveDeactivatesSelectedCrystalsInPlace()
        {
            controller.ChooseStellaStarts();
            controller.HandleCrystalPressed(crystals[4]);
            controller.HandleCrystalPressed(crystals[12]);

            controller.ConfirmStellaMove();

            Assert.That(controller.Game.RemainingCrystals, Is.EqualTo(18));
            Assert.That(crystals[4].State, Is.EqualTo(CrystalVisualState.Deactivated));
            Assert.That(crystals[12].State, Is.EqualTo(CrystalVisualState.Deactivated));
            Assert.That(crystals.Count, Is.EqualTo(20));
            Assert.That(controller.PlayerInputEnabled, Is.False);
        }

        [Test]
        public void BothStartingPlayerChoicesAreRespected()
        {
            controller.ChooseStellaStarts();

            Assert.That(controller.Game.CurrentParticipant, Is.EqualTo(Participant.Stella));
            Assert.That(controller.PlayerInputEnabled, Is.True);
            Assert.That(battlePanel.VisibleCardLabel, Is.EqualTo("STELLA"));

            controller.Retry();
            controller.ChooseKrikStarts();

            Assert.That(controller.Game.CurrentParticipant, Is.EqualTo(Participant.Krik));
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(battlePanel.VisibleCardLabel, Is.EqualTo("KRIK"));
        }

        [Test]
        public void KrikRespondsOptimallyAndReturnsControlToStella()
        {
            controller.ChooseStellaStarts();
            SelectActiveCrystals(1);
            controller.ConfirmStellaMove();

            controller.PerformKrikMoveImmediately();

            Assert.That(controller.Game.RemainingCrystals, Is.EqualTo(16));
            Assert.That(controller.Game.CurrentParticipant, Is.EqualTo(Participant.Stella));
            Assert.That(controller.PlayerInputEnabled, Is.True);
            Assert.That(CountCrystalsInState(CrystalVisualState.Deactivated), Is.EqualTo(4));
            Assert.That(battlePanel.VisibleCardLabel, Is.EqualTo("STELLA"));
        }

        [Test]
        public void KrikPhysicalChoiceUsesUniqueActiveCrystalsOnly()
        {
            crystals[0].SetState(CrystalVisualState.Deactivated);
            crystals[5].SetState(CrystalVisualState.Deactivated);
            crystals[11].SetState(CrystalVisualState.Deactivated);
            controller.SetRandomSourcesForTests(
                new FixedRandom(1),
                new FixedRandom(int.MaxValue));

            List<CrystalView> chosenCrystals = controller.ChooseRandomActiveCrystals(3);

            Assert.That(chosenCrystals.Count, Is.EqualTo(3));
            Assert.That(new HashSet<CrystalView>(chosenCrystals).Count, Is.EqualTo(3));
            for (int index = 0; index < chosenCrystals.Count; index++)
            {
                Assert.That(chosenCrystals[index].State, Is.EqualTo(CrystalVisualState.Active));
                Assert.That(chosenCrystals[index], Is.Not.EqualTo(crystals[0]));
                Assert.That(chosenCrystals[index], Is.Not.EqualTo(crystals[5]));
                Assert.That(chosenCrystals[index], Is.Not.EqualTo(crystals[11]));
            }

            CollectionAssert.AreNotEqual(
                new[] { crystals[1], crystals[2], crystals[3] },
                chosenCrystals,
                "A Krik move should not always consume the first active crystals in scene order.");
        }

        [Test]
        public void PlayerInputStaysLockedThroughThinkingAndCrystalStagger()
        {
            controller.SetRandomSourcesForTests(
                new FixedRandom(3),
                new FixedRandom(int.MaxValue));
            controller.ChooseKrikStarts();
            IEnumerator krikTurn = controller.CreateKrikTurnSequenceForTests();

            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(controller.KrikThinkingDelaySeconds, Is.InRange(1.2f, 1.4f));
            Assert.That(controller.KrikCrystalStaggerSeconds, Is.InRange(0.15f, 0.20f));

            Assert.That(krikTurn.MoveNext(), Is.True, "The first yield is the thinking pause.");
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(CountCrystalsInState(CrystalVisualState.Deactivated), Is.Zero);

            Assert.That(krikTurn.MoveNext(), Is.True, "The first crystal is followed by a stagger.");
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(CountCrystalsInState(CrystalVisualState.Deactivated), Is.EqualTo(1));

            Assert.That(krikTurn.MoveNext(), Is.True, "The second crystal is followed by a stagger.");
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(CountCrystalsInState(CrystalVisualState.Deactivated), Is.EqualTo(2));

            Assert.That(krikTurn.MoveNext(), Is.False, "The turn finishes after the third crystal.");
            Assert.That(CountCrystalsInState(CrystalVisualState.Deactivated), Is.EqualTo(3));
            Assert.That(controller.Game.RemainingCrystals, Is.EqualTo(17));
            Assert.That(controller.Game.CurrentParticipant, Is.EqualTo(Participant.Stella));
            Assert.That(controller.PlayerInputEnabled, Is.True);
        }

        [Test]
        public void StellaCanWinWhenKrikStartsAndRetryRestoresRound()
        {
            controller.ChooseKrikStarts();

            while (!controller.Game.IsGameOver)
            {
                int remainingBeforeKrikMove = controller.Game.RemainingCrystals;
                controller.PerformKrikMoveImmediately();

                if (controller.Game.IsGameOver)
                {
                    break;
                }

                int krikMove = remainingBeforeKrikMove - controller.Game.RemainingCrystals;
                SelectActiveCrystals(4 - krikMove);
                controller.ConfirmStellaMove();
            }

            Assert.That(controller.Game.Winner, Is.EqualTo(Participant.Stella));
            Assert.That(retryPanel.activeSelf, Is.True);
            Assert.That(battlePanel.VisibleCardLabel, Is.EqualTo("STELLA"));

            controller.Retry();

            Assert.That(CountCrystalsInState(CrystalVisualState.Active), Is.EqualTo(20));
            Assert.That(startingPanel.activeSelf, Is.True);
        }

        [Test]
        public void KrikCanWinAfterStellaStarts()
        {
            controller.ChooseStellaStarts();

            while (!controller.Game.IsGameOver)
            {
                SelectActiveCrystals(1);
                controller.ConfirmStellaMove();

                if (controller.Game.IsGameOver)
                {
                    break;
                }

                controller.PerformKrikMoveImmediately();
            }

            Assert.That(controller.Game.Winner, Is.EqualTo(Participant.Krik));
            Assert.That(retryPanel.activeSelf, Is.True);
            Assert.That(battlePanel.VisibleCardLabel, Is.EqualTo("KRIK"));
        }

        [Test]
        public void RetryRestoresAllTwentyCrystalsAndStartingChoice()
        {
            controller.ChooseStellaStarts();
            controller.HandleCrystalPressed(crystals[0]);
            controller.ConfirmStellaMove();

            controller.Retry();

            Assert.That(controller.Game, Is.Null);
            Assert.That(startingPanel.activeSelf, Is.True);
            Assert.That(retryPanel.activeSelf, Is.False);
            Assert.That(crystals.Count, Is.EqualTo(20));
            for (int index = 0; index < crystals.Count; index++)
            {
                Assert.That(crystals[index].State, Is.EqualTo(CrystalVisualState.Active));
            }
        }

        private BattlePanelView CreateBattlePanel()
        {
            GameObject panelObject = CreateObject("BattlePanel");
            BattlePanelView panel = panelObject.AddComponent<BattlePanelView>();
            Image image = panelObject.AddComponent<Image>();
            Text cardLabel = CreateText("CardLabel");
            Text dialogue = CreateText("Dialogue");
            panel.Configure(image, cardLabel, dialogue);
            return panel;
        }

        private void SelectActiveCrystals(int count)
        {
            int selectedCount = 0;

            for (int index = 0; index < crystals.Count && selectedCount < count; index++)
            {
                if (crystals[index].State != CrystalVisualState.Active)
                {
                    continue;
                }

                controller.HandleCrystalPressed(crystals[index]);
                selectedCount++;
            }

            Assert.That(selectedCount, Is.EqualTo(count));
        }

        private int CountCrystalsInState(CrystalVisualState state)
        {
            int count = 0;

            for (int index = 0; index < crystals.Count; index++)
            {
                if (crystals[index].State == state)
                {
                    count++;
                }
            }

            return count;
        }

        private List<CrystalView> CreateCrystals(Level04Controller levelController)
        {
            List<CrystalView> result = new List<CrystalView>();

            for (int index = 0; index < KrikGreetingGame.StartingCrystalCount; index++)
            {
                GameObject crystalObject = CreateObject("Crystal_" + index);
                Image image = crystalObject.AddComponent<Image>();
                Button button = crystalObject.AddComponent<Button>();
                Text label = CreateText("Label_" + index);
                CrystalView view = crystalObject.AddComponent<CrystalView>();
                view.Configure(index, image, button, label, levelController.HandleCrystalPressed);
                result.Add(view);
            }

            return result;
        }

        private Button CreateButton(string name)
        {
            GameObject buttonObject = CreateObject(name);
            buttonObject.AddComponent<Image>();
            return buttonObject.AddComponent<Button>();
        }

        private Text CreateText(string name)
        {
            GameObject textObject = CreateObject(name);
            return textObject.AddComponent<Text>();
        }

        private GameObject CreateObject(string name)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private sealed class FixedRandom : System.Random
        {
            private readonly int fixedValue;

            public FixedRandom(int fixedValue)
            {
                this.fixedValue = fixedValue;
            }

            public override int Next(int minValue, int maxValue)
            {
                return Math.Min(Math.Max(fixedValue, minValue), maxValue - 1);
            }
        }
    }
}
