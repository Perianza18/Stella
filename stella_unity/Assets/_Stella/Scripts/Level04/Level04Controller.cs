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
        [SerializeField] private float krikThinkingDelaySeconds = 0.65f;

        private readonly List<CrystalView> selectedCrystals = new List<CrystalView>();
        private readonly KrikStrategy krikStrategy = new KrikStrategy();
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

            PerformKrikMoveImmediately();
        }

        internal void PerformKrikMoveImmediately()
        {
            if (game == null || game.IsGameOver || game.CurrentParticipant != Participant.Krik)
            {
                return;
            }

            int moveSize = krikStrategy.ChooseMove(game.RemainingCrystals);
            DeactivateFirstActiveCrystals(moveSize);
            game.ApplyMove(moveSize);

            if (game.IsGameOver)
            {
                FinishGame();
                return;
            }

            BeginStellaTurn();
        }

        private void DeactivateFirstActiveCrystals(int count)
        {
            int deactivatedCount = 0;

            for (int index = 0; index < crystalViews.Count && deactivatedCount < count; index++)
            {
                if (crystalViews[index].State != CrystalVisualState.Active)
                {
                    continue;
                }

                crystalViews[index].SetState(CrystalVisualState.Deactivated);
                deactivatedCount++;
            }
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
