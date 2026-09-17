using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level04
{
    /// <summary>
    /// Connects the Level 4 scene controls to the small, testable game model.
    /// </summary>
    public sealed class Level04Controller : MonoBehaviour
    {
        [SerializeField] private BattlePanelView battlePanel;
        [SerializeField] private List<CrystalView> crystalViews = new List<CrystalView>();
        [SerializeField] private Button doneButton;
        [SerializeField] private GameObject startingChoicePanel;
        [SerializeField] private Button stellaStartsButton;
        [SerializeField] private Button krikStartsButton;
        [SerializeField] private GameObject retryPanel;
        [SerializeField] private Button retryButton;
        [SerializeField] private float krikThinkingDelaySeconds = 1.3f;
        [SerializeField] private float krikCrystalStaggerSeconds = 0.18f;

        private readonly List<CrystalView> selectedCrystals = new List<CrystalView>();
        private KrikStrategy krikStrategy;
        private System.Random crystalRandom;
        private KrikGreetingGame game;
        private bool playerInputEnabled;

        public KrikGreetingGame Game
        {
            get { return game; }
        }

        public int SelectedCrystalCount
        {
            get { return selectedCrystals.Count; }
        }

        public bool PlayerInputEnabled
        {
            get { return playerInputEnabled; }
        }

        public IReadOnlyList<CrystalView> CrystalViews
        {
            get { return crystalViews; }
        }

        internal float KrikThinkingDelaySeconds
        {
            get { return krikThinkingDelaySeconds; }
        }

        internal float KrikCrystalStaggerSeconds
        {
            get { return krikCrystalStaggerSeconds; }
        }

        private void Awake()
        {
            EnsureRandomSources();
        }

        private void Start()
        {
            WireButtons();
            WireCrystalCallbacks();
            ResetToStartingChoice();
        }

        public void Configure(
            BattlePanelView panel,
            List<CrystalView> crystals,
            Button confirmButton,
            GameObject startPanel,
            Button stellaStartButton,
            Button krikStartButton,
            GameObject endPanel,
            Button restartButton)
        {
            battlePanel = panel;
            crystalViews = crystals;
            doneButton = confirmButton;
            startingChoicePanel = startPanel;
            stellaStartsButton = stellaStartButton;
            krikStartsButton = krikStartButton;
            retryPanel = endPanel;
            retryButton = restartButton;

            WireButtons();
            WireCrystalCallbacks();
        }

        public void ChooseStellaStarts()
        {
            BeginGame(Participant.Stella);
        }

        public void ChooseKrikStarts()
        {
            BeginGame(Participant.Krik);
        }

        public void HandleCrystalPressed(CrystalView crystal)
        {
            if (!playerInputEnabled || game == null || game.CurrentParticipant != Participant.Stella)
            {
                return;
            }

            if (crystal.State == CrystalVisualState.Selected)
            {
                selectedCrystals.Remove(crystal);
                crystal.SetState(CrystalVisualState.Active);
            }
            else if (crystal.State == CrystalVisualState.Active && selectedCrystals.Count < KrikGreetingGame.MaximumMove)
            {
                selectedCrystals.Add(crystal);
                crystal.SetState(CrystalVisualState.Selected);
            }

            UpdateDoneButton();
        }

        public void ConfirmStellaMove()
        {
            if (!playerInputEnabled || selectedCrystals.Count == 0 || game == null)
            {
                return;
            }

            int moveSize = selectedCrystals.Count;
            game.ApplyMove(moveSize);

            for (int index = 0; index < selectedCrystals.Count; index++)
            {
                selectedCrystals[index].SetState(CrystalVisualState.Deactivated);
            }

            selectedCrystals.Clear();
            SetPlayerInputEnabled(false);

            if (game.IsGameOver)
            {
                FinishGame();
                return;
            }

            battlePanel.ShowKrikTurn(game.RemainingCrystals);

            if (Application.isPlaying)
            {
                StartCoroutine(PerformKrikTurn());
            }
        }

        public void Retry()
        {
            StopAllCoroutines();
            ResetToStartingChoice();
        }

        private void BeginGame(Participant startingParticipant)
        {
            StopAllCoroutines();
            game = new KrikGreetingGame(startingParticipant);
            selectedCrystals.Clear();

            for (int index = 0; index < crystalViews.Count; index++)
            {
                crystalViews[index].SetState(CrystalVisualState.Active);
            }

            startingChoicePanel.SetActive(false);
            retryPanel.SetActive(false);

            if (startingParticipant == Participant.Stella)
            {
                BeginStellaTurn();
            }
            else
            {
                SetPlayerInputEnabled(false);
                battlePanel.ShowKrikTurn(game.RemainingCrystals);

                if (Application.isPlaying)
                {
                    StartCoroutine(PerformKrikTurn());
                }
            }
        }

        private IEnumerator PerformKrikTurn()
        {
            yield return new WaitForSeconds(krikThinkingDelaySeconds);

            EnsureRandomSources();
            int moveSize = krikStrategy.ChooseMove(game.RemainingCrystals);
            List<CrystalView> crystalsForMove = ChooseRandomActiveCrystals(moveSize);

            for (int index = 0; index < crystalsForMove.Count; index++)
            {
                crystalsForMove[index].SetState(CrystalVisualState.Deactivated);

                if (index < crystalsForMove.Count - 1)
                {
                    yield return new WaitForSeconds(krikCrystalStaggerSeconds);
                }
            }

            CompleteKrikMove(moveSize);
        }

        internal void PerformKrikMoveImmediately()
        {
            if (game == null || game.IsGameOver || game.CurrentParticipant != Participant.Krik)
            {
                return;
            }

            EnsureRandomSources();
            int moveSize = krikStrategy.ChooseMove(game.RemainingCrystals);
            List<CrystalView> crystalsForMove = ChooseRandomActiveCrystals(moveSize);

            for (int index = 0; index < crystalsForMove.Count; index++)
            {
                crystalsForMove[index].SetState(CrystalVisualState.Deactivated);
            }

            CompleteKrikMove(moveSize);
        }

        internal List<CrystalView> ChooseRandomActiveCrystals(int count)
        {
            EnsureRandomSources();
            List<CrystalView> activeCrystals = new List<CrystalView>();

            for (int index = 0; index < crystalViews.Count; index++)
            {
                if (crystalViews[index].State == CrystalVisualState.Active)
                {
                    activeCrystals.Add(crystalViews[index]);
                }
            }

            if (count < 0 || count > activeCrystals.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    "The Krik cannot choose more active crystals than remain.");
            }

            // A partial Fisher-Yates shuffle chooses unique locations without
            // disturbing the scene list or the stable two-by-ten layout.
            for (int index = 0; index < count; index++)
            {
                int randomIndex = crystalRandom.Next(index, activeCrystals.Count);
                CrystalView temporary = activeCrystals[index];
                activeCrystals[index] = activeCrystals[randomIndex];
                activeCrystals[randomIndex] = temporary;
            }

            return activeCrystals.GetRange(0, count);
        }

        internal IEnumerator CreateKrikTurnSequenceForTests()
        {
            return PerformKrikTurn();
        }

        internal void SetRandomSourcesForTests(System.Random strategyRandom, System.Random physicalRandom)
        {
            krikStrategy = new KrikStrategy(strategyRandom);
            crystalRandom = physicalRandom ?? throw new ArgumentNullException(nameof(physicalRandom));
        }

        private void CompleteKrikMove(int moveSize)
        {
            game.ApplyMove(moveSize);

            if (game.IsGameOver)
            {
                FinishGame();
                return;
            }

            BeginStellaTurn();
        }

        private void BeginStellaTurn()
        {
            battlePanel.ShowStellaTurn(game.RemainingCrystals);
            SetPlayerInputEnabled(true);
        }

        private void FinishGame()
        {
            SetPlayerInputEnabled(false);

            if (game.Winner == Participant.Stella)
            {
                battlePanel.ShowStellaVictory();
            }
            else
            {
                battlePanel.ShowKrikVictory();
            }

            retryPanel.SetActive(true);
        }

        private void ResetToStartingChoice()
        {
            game = null;
            selectedCrystals.Clear();

            for (int index = 0; index < crystalViews.Count; index++)
            {
                crystalViews[index].SetState(CrystalVisualState.Active);
            }

            startingChoicePanel.SetActive(true);
            retryPanel.SetActive(false);
            battlePanel.ShowStartingChoice();
            SetPlayerInputEnabled(false);
        }

        private void SetPlayerInputEnabled(bool enabled)
        {
            playerInputEnabled = enabled;

            for (int index = 0; index < crystalViews.Count; index++)
            {
                crystalViews[index].SetInputLocked(!enabled);
            }

            UpdateDoneButton();
        }

        private void UpdateDoneButton()
        {
            if (doneButton != null)
            {
                doneButton.interactable = playerInputEnabled && selectedCrystals.Count > 0;
            }
        }

        private void WireButtons()
        {
            WireButton(stellaStartsButton, ChooseStellaStarts);
            WireButton(krikStartsButton, ChooseKrikStarts);
            WireButton(doneButton, ConfirmStellaMove);
            WireButton(retryButton, Retry);
        }

        private void WireCrystalCallbacks()
        {
            for (int index = 0; index < crystalViews.Count; index++)
            {
                crystalViews[index].SetPressedCallback(HandleCrystalPressed);
            }
        }

        private void EnsureRandomSources()
        {
            if (krikStrategy == null)
            {
                krikStrategy = new KrikStrategy();
            }

            if (crystalRandom == null)
            {
                int seed = unchecked(Environment.TickCount * 397) ^ GetInstanceID();
                crystalRandom = new System.Random(seed);
            }
        }

        private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }
}
