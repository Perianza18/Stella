using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level02.Tests
{
    public sealed class Level02ControllerTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private Level02Controller controller;
        private List<BoardCellView> cells;
        private Button actionButton;
        private GameObject startingPanel;
        private Text activeCardLabel;

        [SetUp]
        public void SetUp()
        {
            controller = CreateObject("Controller").AddComponent<Level02Controller>();
            cells = CreateCells();
            actionButton = CreateButton("ContextActionButton");
            startingPanel = CreateObject("StartingPlayerChoice");

            controller.Configure(
                cells,
                activeCardLabel = CreateText("Card"),
                CreateText("Speaker"),
                CreateText("Dialogue"),
                actionButton,
                startingPanel,
                CreateButton("StellaStarts"),
                CreateButton("KeeperStarts"));
            controller.SetRandomSourceForTests(new FixedRandom(0));
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(createdObjects[index]);
            createdObjects.Clear();
        }

        [Test]
        public void StartingChoiceIsVisibleAndBoardInputIsLocked()
        {
            Assert.That(startingPanel.activeSelf, Is.True);
            Assert.That(actionButton.gameObject.activeSelf, Is.False);
            Assert.That(controller.Game, Is.Null);
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(cells[0].InputEnabled, Is.False);
            Assert.That(cells[24].VisualState, Is.EqualTo(BoardCellVisualState.Blocked));
            Assert.That(controller.Phase, Is.EqualTo(Level02Phase.Discovery));
        }

        [Test]
        public void StellaCanSelectDeselectAndNeverSelectMoreThanFour()
        {
            controller.ChooseStellaStarts();
            Press((0, 0), (0, 1), (0, 2), (1, 0), (1, 1));
            Assert.That(controller.SelectedCellCount, Is.EqualTo(4));

            controller.HandleCellPressed(Cell(0, 1));
            Assert.That(controller.SelectedCellCount, Is.EqualTo(3));
        }

        [Test]
        public void ContextButtonReflectsPartialInvalidAndValidSelections()
        {
            controller.ChooseStellaStarts();
            Assert.That(actionButton.gameObject.activeSelf, Is.True);
            Assert.That(actionButton.interactable, Is.False);
            Assert.That(ActionLabel, Is.EqualTo("PLACE PIECE"));

            Press((0, 0), (0, 1), (1, 0), (1, 1));
            Assert.That(actionButton.interactable, Is.False);
            Assert.That(ActionLabel, Is.EqualTo("NOT A VALID L"));
            Assert.That(Cell(0, 0).VisualState, Is.EqualTo(BoardCellVisualState.InvalidPreview));

            Press((0, 0), (0, 1), (1, 0), (1, 1));
            Press((0, 0), (0, 1), (0, 2), (1, 0));
            Assert.That(actionButton.interactable, Is.True);
            Assert.That(ActionLabel, Is.EqualTo("PLACE PIECE"));
            Assert.That(Cell(0, 0).VisualState, Is.EqualTo(BoardCellVisualState.ValidPreview));
        }

        [Test]
        public void ConfirmingStellaMoveCommitsGroupAndHidesActionDuringKeeperTurn()
        {
            controller.ChooseStellaStarts();
            Press((0, 0), (0, 1), (0, 2), (1, 0));
            actionButton.onClick.Invoke();

            Assert.That(Cell(0, 0).VisualState, Is.EqualTo(BoardCellVisualState.Stella));
            Assert.That(Cell(0, 1).VisualState, Is.EqualTo(BoardCellVisualState.Stella));
            Assert.That(Cell(0, 2).VisualState, Is.EqualTo(BoardCellVisualState.Stella));
            Assert.That(Cell(1, 0).VisualState, Is.EqualTo(BoardCellVisualState.Stella));
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(actionButton.gameObject.activeSelf, Is.False);
            Assert.That(controller.Game.CurrentParticipant, Is.EqualTo(RuinsParticipant.Alien));
        }

        [Test]
        public void KeeperStartLocksInputThenImmediateMoveReturnsControlToStella()
        {
            controller.ChooseAlienStarts();
            Assert.That(activeCardLabel.text, Is.EqualTo("KEEPER"));
            Assert.That(controller.PlayerInputEnabled, Is.False);
            Assert.That(actionButton.gameObject.activeSelf, Is.False);
            Assert.That(controller.Game.CurrentParticipant, Is.EqualTo(RuinsParticipant.Alien));

            controller.PerformAlienMoveImmediately();

            Assert.That(controller.Game.CurrentParticipant, Is.EqualTo(RuinsParticipant.Stella));
            Assert.That(activeCardLabel.text, Is.EqualTo("STELLA"));
            Assert.That(controller.PlayerInputEnabled, Is.True);
            Assert.That(actionButton.gameObject.activeSelf, Is.True);
            Assert.That(CountState(BoardCellVisualState.Occupied), Is.EqualTo(4));
        }

        [Test]
        public void KeeperSequencePreviewsAllFourTogetherBeforeCommit()
        {
            controller.ChooseAlienStarts();
            IEnumerator sequence = controller.CreateAlienTurnSequenceForTests();

            Assert.That(sequence.MoveNext(), Is.True);
            Assert.That(CountState(BoardCellVisualState.AlienPreview), Is.Zero);
            Assert.That(sequence.MoveNext(), Is.True);
            Assert.That(CountState(BoardCellVisualState.AlienPreview), Is.EqualTo(4));
            Assert.That(sequence.MoveNext(), Is.True, "Committed Keeper stones get a readable hold.");
            Assert.That(sequence.MoveNext(), Is.False);
            Assert.That(CountState(BoardCellVisualState.Occupied), Is.EqualTo(4));
            Assert.That(controller.PlayerInputEnabled, Is.True);
        }

        [Test]
        public void DiscoveryLossShowsRetryAndButtonRestoresFreshBoard()
        {
            controller.ChooseStellaStarts();
            controller.FinishMatchForTests(RuinsParticipant.Alien);

            Assert.That(actionButton.gameObject.activeSelf, Is.True);
            Assert.That(actionButton.interactable, Is.True);
            Assert.That(ActionLabel, Is.EqualTo("RETRY"));
            actionButton.onClick.Invoke();

            Assert.That(controller.Game, Is.Null);
            Assert.That(startingPanel.activeSelf, Is.True);
            Assert.That(actionButton.gameObject.activeSelf, Is.False);
            Assert.That(CountState(BoardCellVisualState.Free), Is.EqualTo(48));
            Assert.That(CountState(BoardCellVisualState.Blocked), Is.EqualTo(1));
        }

        [Test]
        public void DiscoveryVictoryTransitionsToProofWithoutCompletingLevel()
        {
            controller.ChooseStellaStarts();
            controller.FinishMatchForTests(RuinsParticipant.Stella);

            Assert.That(controller.Phase, Is.EqualTo(Level02Phase.Proof));
            Assert.That(controller.Game.StartingParticipant, Is.EqualTo(RuinsParticipant.Alien));
            Assert.That(startingPanel.activeSelf, Is.False);
            Assert.That(actionButton.gameObject.activeSelf, Is.False);
            Assert.That(controller.Game.IsGameOver, Is.False);
            Assert.That(CountState(BoardCellVisualState.Occupied), Is.EqualTo(16));
        }

        [Test]
        public void ProofLossRetriesProofInSameScene()
        {
            controller.TransitionToProofForTests();
            controller.FinishMatchForTests(RuinsParticipant.Alien);

            Assert.That(actionButton.gameObject.activeSelf, Is.True);
            Assert.That(ActionLabel, Is.EqualTo("RETRY"));
            actionButton.onClick.Invoke();

            Assert.That(controller.Phase, Is.EqualTo(Level02Phase.Proof));
            Assert.That(startingPanel.activeSelf, Is.False);
            Assert.That(actionButton.gameObject.activeSelf, Is.False);
            Assert.That(controller.Game.StartingParticipant, Is.EqualTo(RuinsParticipant.Alien));
        }

        [Test]
        public void OnlyProofVictoryCompletesLevelWithCleanFinalAction()
        {
            controller.TransitionToProofForTests();
            controller.FinishMatchForTests(RuinsParticipant.Stella);

            Assert.That(controller.Phase, Is.EqualTo(Level02Phase.Completed));
            Assert.That(actionButton.gameObject.activeSelf, Is.True);
            Assert.That(actionButton.interactable, Is.True);
            Assert.That(ActionLabel, Is.EqualTo("PLAY AGAIN"));
        }

        [Test]
        public void RecentParticipantColourNeutralizesToOccupiedStone()
        {
            controller.ChooseStellaStarts();
            Press((0, 0), (0, 1), (0, 2), (1, 0));
            controller.ConfirmStellaPlacement();
            Assert.That(CountState(BoardCellVisualState.Stella), Is.EqualTo(4));

            controller.NeutralizeRecentMoveForTests();
            Assert.That(CountState(BoardCellVisualState.Occupied), Is.EqualTo(4));
        }

        private string ActionLabel => actionButton.GetComponentInChildren<Text>().text;

        private void Press(params (int row, int column)[] coordinates)
        {
            foreach ((int row, int column) coordinate in coordinates)
                controller.HandleCellPressed(Cell(coordinate.row, coordinate.column));
        }

        private BoardCellView Cell(int row, int column)
        {
            return cells[row * RuinsBoardGame.BoardSize + column];
        }

        private int CountState(BoardCellVisualState state)
        {
            int count = 0;
            foreach (BoardCellView cell in cells)
                if (cell.VisualState == state) count++;
            return count;
        }

        private List<BoardCellView> CreateCells()
        {
            List<BoardCellView> result = new List<BoardCellView>();
            for (int row = 0; row < RuinsBoardGame.BoardSize; row++)
            {
                for (int column = 0; column < RuinsBoardGame.BoardSize; column++)
                {
                    GameObject cellObject = CreateObject("Cell_" + row + "_" + column);
                    Image image = cellObject.AddComponent<Image>();
                    Button button = cellObject.AddComponent<Button>();
                    Text label = CreateText("Label_" + row + "_" + column);
                    label.transform.SetParent(cellObject.transform, false);
                    BoardCellView view = cellObject.AddComponent<BoardCellView>();
                    view.Configure(row, column, image, button, label, controller.HandleCellPressed);
                    result.Add(view);
                }
            }
            return result;
        }

        private Button CreateButton(string name)
        {
            GameObject buttonObject = CreateObject(name);
            buttonObject.AddComponent<Image>();
            Button button = buttonObject.AddComponent<Button>();
            Text label = CreateText(name + "Label");
            label.transform.SetParent(buttonObject.transform, false);
            return button;
        }

        private Text CreateText(string name) { return CreateObject(name).AddComponent<Text>(); }

        private GameObject CreateObject(string name)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private sealed class FixedRandom : System.Random
        {
            private readonly int value;
            public FixedRandom(int value) { this.value = value; }

            public override int Next(int minValue, int maxValue)
            {
                return Math.Min(Math.Max(value, minValue), maxValue - 1);
            }
        }
    }
}
