using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level02
{
    public enum Level02Phase { Discovery, Proof, Completed }

    /// <summary>Orchestrates Discovery -> Proof while rules and strategy stay in pure C#.</summary>
    public sealed class Level02Controller : MonoBehaviour
    {
        private enum ContextAction { None, Place, RetryDiscovery, RetryProof, RestartLevel }

        private static readonly Color PartialAction = new Color(0.67f, 0.43f, 0.12f, 1f);
        private static readonly Color ValidAction = new Color(0.18f, 0.58f, 0.35f, 1f);
        private static readonly Color InvalidAction = new Color(0.62f, 0.19f, 0.18f, 1f);
        private static readonly Color PrimaryAction = new Color(0.18f, 0.58f, 0.66f, 1f);

        [SerializeField] private List<BoardCellView> cellViews = new List<BoardCellView>();
        [SerializeField] private Text activeCardLabel, dialogueSpeakerText, dialogueBodyText;
        [SerializeField] private Button contextActionButton;
        [SerializeField] private GameObject startingChoicePanel;
        [SerializeField] private Button stellaStartsButton, alienStartsButton;
        [SerializeField] private float alienThinkingDelaySeconds = 1.1f;
        [SerializeField] private float alienPreviewDelaySeconds = 0.55f;
        [SerializeField] private float committedMoveHoldSeconds = 0.65f;
        [SerializeField] private float proofTransitionDelaySeconds = 1.0f;

        private readonly List<BoardCoordinate> selectedCells = new List<BoardCoordinate>();
        private readonly HashSet<BoardCoordinate> recentMove = new HashSet<BoardCoordinate>();
        private RuinsBoardGame game;
        private RuinsBoardSolver solver;
        private System.Random randomSource;
        private bool playerInputEnabled;
        private RuinsParticipant? recentParticipant;
        private ContextAction contextAction;

        public RuinsBoardGame Game => game;
        public Level02Phase Phase { get; private set; }
        public int SelectedCellCount => selectedCells.Count;
        public bool PlayerInputEnabled => playerInputEnabled;
        public IReadOnlyList<BoardCellView> CellViews => cellViews;
        internal float AlienThinkingDelaySeconds => alienThinkingDelaySeconds;
        internal float AlienPreviewDelaySeconds => alienPreviewDelaySeconds;

        private void Awake() { EnsureDependencies(); }
        private void Start() { WireControls(); WireCells(); ResetDiscovery(); }

        public void Configure(
            List<BoardCellView> boardCells,
            Text cardLabel,
            Text dialogueSpeaker,
            Text dialogueBody,
            Button actionButton,
            GameObject startPanel,
            Button stellaStart,
            Button alienStart)
        {
            cellViews = boardCells;
            activeCardLabel = cardLabel;
            dialogueSpeakerText = dialogueSpeaker;
            dialogueBodyText = dialogueBody;
            contextActionButton = actionButton;
            startingChoicePanel = startPanel;
            stellaStartsButton = stellaStart;
            alienStartsButton = alienStart;
            EnsureDependencies();
            WireControls();
            WireCells();
            ResetDiscovery();
        }

        public void ChooseStellaStarts() => BeginDiscovery(RuinsParticipant.Stella);
        public void ChooseAlienStarts() => BeginDiscovery(RuinsParticipant.Alien);

        public void HandleCellPressed(BoardCellView cell)
        {
            if (!playerInputEnabled || game == null || game.CurrentParticipant != RuinsParticipant.Stella) return;
            BoardCoordinate coordinate = cell.Coordinate;
            if (game.GetCellState(coordinate.Row, coordinate.Column) != RuinsCellState.Free) return;
            int index = selectedCells.IndexOf(coordinate);
            if (index >= 0) selectedCells.RemoveAt(index);
            else if (selectedCells.Count < RuinsBoardGame.CellsPerPiece) selectedCells.Add(coordinate);
            RefreshPresentation();
        }

        public void ConfirmStellaPlacement()
        {
            if (!playerInputEnabled || game == null || game.CurrentParticipant != RuinsParticipant.Stella ||
                game.ValidatePlacement(selectedCells) != PlacementValidation.Valid) return;
            game.ApplyPlacement(selectedCells);
            ShowRecentMove(game.LastPlacedCells, RuinsParticipant.Stella);
            selectedCells.Clear();
            SetPlayerInputEnabled(false);
            RefreshPresentation();
            if (game.IsGameOver) { FinishMatch(); return; }
            BeginKeeperTurn();
        }

        public void Retry()
        {
            StopAllCoroutines();
            if (Phase == Level02Phase.Proof) BeginProof();
            else ResetDiscovery();
        }

        internal void SetRandomSourceForTests(System.Random random)
        {
            randomSource = random ?? throw new ArgumentNullException(nameof(random));
            solver = new RuinsBoardSolver();
        }

        internal IEnumerator CreateAlienTurnSequenceForTests() => PerformKeeperTurn();

        internal void PerformAlienMoveImmediately()
        {
            if (game == null || game.IsGameOver || game.CurrentParticipant != RuinsParticipant.Alien) return;
            CompleteKeeperMove(game.ChooseAlienMove());
        }

        internal void NeutralizeRecentMoveForTests() { ClearRecentMove(); RefreshPresentation(); }
        internal void TransitionToProofForTests() { StopAllCoroutines(); BeginProof(); }

        internal void FinishMatchForTests(RuinsParticipant winner)
        {
            game.SetWinnerForTests(winner);
            FinishMatch();
        }

        private void BeginDiscovery(RuinsParticipant starter)
        {
            StopAllCoroutines();
            EnsureDependencies();
            Phase = Level02Phase.Discovery;
            game = new RuinsBoardGame(starter, randomSource, null, solver);
            PrepareMatch();
            if (starter == RuinsParticipant.Stella) BeginStellaTurn();
            else BeginKeeperTurn();
        }

        private void BeginProof()
        {
            StopAllCoroutines();
            EnsureDependencies();
            Phase = Level02Phase.Proof;
            int presetIndex = randomSource.Next(0, RuinsBoardGame.ProofPresetCount);
            game = new RuinsBoardGame(
                RuinsParticipant.Alien,
                randomSource,
                RuinsBoardGame.GetProofPreset(presetIndex),
                solver);
            PrepareMatch();
            BeginKeeperTurn("This board has already been played. Continue.");
        }

        private void PrepareMatch()
        {
            selectedCells.Clear();
            ClearRecentMove();
            startingChoicePanel.SetActive(false);
            SetContextAction(ContextAction.None);
            RefreshPresentation();
        }

        private void BeginStellaTurn()
        {
            if (game.IsGameOver) { FinishMatch(); return; }
            ClearRecentMove();
            activeCardLabel.text = "STELLA";
            dialogueSpeakerText.text = "STELLA";
            dialogueBodyText.text = "My turn.";
            SetContextAction(ContextAction.Place);
            SetPlayerInputEnabled(true);
            RefreshPresentation();
        }

        private void BeginKeeperTurn(string dialogue = "The keeper studies the board.")
        {
            SetPlayerInputEnabled(false);
            selectedCells.Clear();
            if (game.IsGameOver) { FinishMatch(); return; }
            activeCardLabel.text = "KEEPER";
            dialogueSpeakerText.text = "KEEPER";
            dialogueBodyText.text = dialogue;
            SetContextAction(ContextAction.None);
            RefreshPresentation();
            if (Application.isPlaying) StartCoroutine(PerformKeeperTurn());
        }

        private IEnumerator PerformKeeperTurn()
        {
            yield return new WaitForSeconds(alienThinkingDelaySeconds);
            ClearRecentMove();
            RefreshPresentation();
            IReadOnlyList<BoardCoordinate> move = game.ChooseAlienMove();
            ShowKeeperPreview(move);
            yield return new WaitForSeconds(alienPreviewDelaySeconds);
            CompleteKeeperMove(move);
            yield return new WaitForSeconds(committedMoveHoldSeconds);
            ClearRecentMove();
            RefreshPresentation();
            if (!game.IsGameOver) BeginStellaTurn();
        }

        private void CompleteKeeperMove(IReadOnlyList<BoardCoordinate> move)
        {
            game.ApplyPlacement(move);
            ShowRecentMove(game.LastPlacedCells, RuinsParticipant.Alien);
            RefreshPresentation();
            if (game.IsGameOver) FinishMatch();
            else if (!Application.isPlaying) BeginStellaTurn();
        }

        private void ShowKeeperPreview(IReadOnlyList<BoardCoordinate> move)
        {
            HashSet<BoardCoordinate> preview = new HashSet<BoardCoordinate>(move);
            foreach (BoardCellView cell in cellViews)
            {
                if (!preview.Contains(cell.Coordinate)) continue;
                cell.SetVisualState(BoardCellVisualState.AlienPreview);
                cell.SetInputEnabled(false);
            }
        }

        private void FinishMatch()
        {
            StopAllCoroutines();
            selectedCells.Clear();
            SetPlayerInputEnabled(false);
            RefreshPresentation();

            if (Phase == Level02Phase.Discovery && game.Winner == RuinsParticipant.Stella)
            {
                activeCardLabel.text = "KEEPER";
                dialogueSpeakerText.text = "KEEPER";
                dialogueBodyText.text = "Again.";
                SetContextAction(ContextAction.None);
                if (Application.isPlaying) StartCoroutine(TransitionToProof());
                else BeginProof();
                return;
            }

            activeCardLabel.text = "KEEPER";
            dialogueSpeakerText.text = "KEEPER";
            if (Phase == Level02Phase.Proof && game.Winner == RuinsParticipant.Stella)
            {
                Phase = Level02Phase.Completed;
                dialogueBodyText.text = "You understand.";
                SetContextAction(ContextAction.RestartLevel, "PLAY AGAIN");
            }
            else if (Phase == Level02Phase.Proof)
            {
                dialogueBodyText.text = "Try again.";
                SetContextAction(ContextAction.RetryProof, "RETRY");
            }
            else
            {
                dialogueBodyText.text = "Try again.";
                SetContextAction(ContextAction.RetryDiscovery, "RETRY");
            }
        }

        private IEnumerator TransitionToProof()
        {
            yield return new WaitForSeconds(proofTransitionDelaySeconds);
            BeginProof();
        }

        private void ResetDiscovery()
        {
            game = null;
            Phase = Level02Phase.Discovery;
            selectedCells.Clear();
            ClearRecentMove();
            playerInputEnabled = false;
            foreach (BoardCellView cell in cellViews)
            {
                bool blocked = cell.Coordinate.Row == 3 && cell.Coordinate.Column == 3;
                cell.SetVisualState(blocked ? BoardCellVisualState.Blocked : BoardCellVisualState.Free);
                cell.SetInputEnabled(false);
            }
            activeCardLabel.text = "KEEPER";
            dialogueSpeakerText.text = "KEEPER";
            dialogueBodyText.text = "Four stones form one piece. Place them as an L.";
            SetContextAction(ContextAction.None);
            startingChoicePanel.SetActive(true);
        }

        private void RefreshPresentation()
        {
            PlacementValidation validation = game == null
                ? PlacementValidation.WrongCellCount
                : game.ValidatePlacement(selectedCells);
            foreach (BoardCellView cell in cellViews)
            {
                cell.SetVisualState(StateFor(cell.Coordinate, validation));
                cell.SetInputEnabled(playerInputEnabled);
            }

            if (contextAction != ContextAction.Place) return;
            if (selectedCells.Count == RuinsBoardGame.CellsPerPiece && validation != PlacementValidation.Valid)
            {
                SetActionAppearance("NOT A VALID L", false, InvalidAction);
            }
            else if (validation == PlacementValidation.Valid)
            {
                SetActionAppearance("PLACE PIECE", true, ValidAction);
            }
            else
            {
                SetActionAppearance("PLACE PIECE", false, PartialAction);
            }
        }

        private BoardCellVisualState StateFor(BoardCoordinate coordinate, PlacementValidation validation)
        {
            if (game == null)
                return coordinate.Row == 3 && coordinate.Column == 3
                    ? BoardCellVisualState.Blocked
                    : BoardCellVisualState.Free;
            RuinsCellState state = game.GetCellState(coordinate.Row, coordinate.Column);
            if (state == RuinsCellState.Blocked) return BoardCellVisualState.Blocked;
            if (selectedCells.Contains(coordinate))
            {
                if (selectedCells.Count < RuinsBoardGame.CellsPerPiece) return BoardCellVisualState.Selected;
                return validation == PlacementValidation.Valid
                    ? BoardCellVisualState.ValidPreview
                    : BoardCellVisualState.InvalidPreview;
            }
            if (recentMove.Contains(coordinate))
                return recentParticipant == RuinsParticipant.Stella
                    ? BoardCellVisualState.Stella
                    : BoardCellVisualState.Alien;
            return state == RuinsCellState.Occupied
                ? BoardCellVisualState.Occupied
                : BoardCellVisualState.Free;
        }

        private void HandleContextAction()
        {
            switch (contextAction)
            {
                case ContextAction.Place:
                    ConfirmStellaPlacement();
                    break;
                case ContextAction.RetryProof:
                    BeginProof();
                    break;
                case ContextAction.RetryDiscovery:
                case ContextAction.RestartLevel:
                    ResetDiscovery();
                    break;
            }
        }

        private void SetContextAction(ContextAction action, string label = null)
        {
            contextAction = action;
            bool visible = action != ContextAction.None;
            contextActionButton.gameObject.SetActive(visible);
            if (!visible) return;
            string resolvedLabel = label ?? (action == ContextAction.Place ? "PLACE PIECE" : "RETRY");
            SetActionAppearance(resolvedLabel, action != ContextAction.Place, PrimaryAction);
        }

        private void SetActionAppearance(string label, bool interactable, Color color)
        {
            Text buttonLabel = contextActionButton.GetComponentInChildren<Text>();
            if (buttonLabel != null) buttonLabel.text = label;
            Image image = contextActionButton.GetComponent<Image>();
            if (image != null) image.color = color;
            ColorBlock colors = contextActionButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.84f, 0.84f, 0.84f, 1f);
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            contextActionButton.colors = colors;
            contextActionButton.interactable = interactable;
        }

        private void ShowRecentMove(IEnumerable<BoardCoordinate> cells, RuinsParticipant participant)
        {
            ClearRecentMove();
            foreach (BoardCoordinate cell in cells) recentMove.Add(cell);
            recentParticipant = participant;
        }

        private void ClearRecentMove()
        {
            recentMove.Clear();
            recentParticipant = null;
        }

        private void SetPlayerInputEnabled(bool enabled) { playerInputEnabled = enabled; }

        private void EnsureDependencies()
        {
            if (randomSource == null)
                randomSource = new System.Random(unchecked(Environment.TickCount * 397) ^ GetInstanceID());
            if (solver == null) solver = new RuinsBoardSolver();
        }

        private void WireControls()
        {
            Wire(stellaStartsButton, ChooseStellaStarts);
            Wire(alienStartsButton, ChooseAlienStarts);
            Wire(contextActionButton, HandleContextAction);
        }

        private void WireCells()
        {
            foreach (BoardCellView cell in cellViews) cell.SetPressedCallback(HandleCellPressed);
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (!button) return;
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }
}
