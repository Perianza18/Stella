using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level05
{
    /// <summary>Coordinates the small Unity presentation around GemsGame.</summary>
    public sealed class Level05Controller : MonoBehaviour
    {
        [SerializeField] private Text remainingText;
        [SerializeField] private Text rangeText;
        [SerializeField] private InputField amountField;
        [SerializeField] private Text historyText;
        [SerializeField] private Text noticeText;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject retryPanel;

        private GemsGame game;
        private int selectedAmount = 1;
        private readonly StringBuilder history = new StringBuilder();

        public GemsGame Game { get { return game; } }
        public Button ConfirmButton { get { return confirmButton; } }
        public Button DecreaseButton { get { return decreaseButton; } }
        public Button IncreaseButton { get { return increaseButton; } }
        public int SelectedAmountForTests { get { return selectedAmount; } }

        private void Awake() { EnsureGame(); }
        private void Start() { InitializeControls(); }

        internal void InitializeControls()
        {
            WireControls();
            ResetSequence();
        }

        public void Configure(
            Text remaining,
            Text range,
            InputField amount,
            Text historyLog,
            Text notice,
            Button decrease,
            Button increase,
            Button confirm,
            GameObject retryOverlay,
            Button retry)
        {
            remainingText = remaining;
            rangeText = range;
            amountField = amount;
            historyText = historyLog;
            noticeText = notice;
            decreaseButton = decrease;
            increaseButton = increase;
            confirmButton = confirm;
            retryPanel = retryOverlay;
            retryButton = retry;
            EnsureGame();
            WireControls();
            ResetSequence();
        }

        public void Decrease()
        {
            selectedAmount = Math.Max(1, selectedAmount - 1);
            RefreshAmount();
        }

        public void Increase()
        {
            int limit = GemsGame.TurnLimit(game.RemainingGems);
            selectedAmount = Math.Min(limit, selectedAmount + 1);
            RefreshAmount();
        }

        public void Confirm()
        {
            if (game.IsGameOver) return;

            ApplyAndLog(Participant.Stella, selectedAmount);
            if (CheckGameOver()) return;

            int guardianAmount = game.OptimalMove(game.RemainingGems);
            ApplyAndLog(Participant.Guardian, guardianAmount);
            if (CheckGameOver()) return;

            selectedAmount = 1;
            RefreshAmount();
        }

        public void Retry()
        {
            ResetSequence();
        }

        public void HandleAmountTyped(string typed)
        {
            if (game == null || game.IsGameOver) return;
            if (!int.TryParse(typed, out int parsed)) parsed = selectedAmount;
            int limit = GemsGame.TurnLimit(game.RemainingGems);
            selectedAmount = Mathf.Clamp(parsed, 1, limit);
            RefreshAmount();
        }

        internal void SetGameForTests(GemsGame replacement)
        {
            game = replacement ?? throw new ArgumentNullException(nameof(replacement));
            selectedAmount = 1;
            RefreshAmount();
        }

        internal void ConfirmForTests() { Confirm(); }

        private void ApplyAndLog(Participant participant, int amount)
        {
            game.ApplyMove(amount);
            string who = participant == Participant.Stella ? "Stella" : "Guardian";
            AppendHistory(who + " takes " + amount + ". " + game.RemainingGems + " gems left.");
        }

        private bool CheckGameOver()
        {
            if (!game.IsGameOver) return false;

            bool stellaWon = game.Winner == Participant.Stella;
            SetNotice(stellaWon ? "STELLA TAKES THE LAST GEM!" : "THE GUARDIAN TAKES THE LAST GEM.");
            if (retryPanel != null) retryPanel.SetActive(true);
            SetInteractable(false);
            RefreshAmount();
            return true;
        }

        private void ResetSequence()
        {
            EnsureGame();
            game = new GemsGame();
            selectedAmount = 1;
            history.Length = 0;
            if (historyText != null) historyText.text = string.Empty;
            SetNotice(string.Empty);
            if (retryPanel != null) retryPanel.SetActive(false);
            SetInteractable(true);
            RefreshAmount();
        }

        private void RefreshAmount()
        {
            if (game == null) return;

            int limit = GemsGame.TurnLimit(game.RemainingGems);
            selectedAmount = Mathf.Clamp(selectedAmount, 1, limit);

            if (remainingText != null) remainingText.text = game.RemainingGems + " GEMS LEFT";
            if (rangeText != null) rangeText.text = "TAKE 1-" + limit;
            if (amountField != null) amountField.SetTextWithoutNotify(selectedAmount.ToString());

            bool interactable = !game.IsGameOver;
            if (decreaseButton != null) decreaseButton.interactable = interactable && selectedAmount > 1;
            if (increaseButton != null) increaseButton.interactable = interactable && selectedAmount < limit;
            if (confirmButton != null) confirmButton.interactable = interactable;
            if (amountField != null) amountField.interactable = interactable;
        }

        private void AppendHistory(string line)
        {
            history.AppendLine(line);
            if (historyText != null) historyText.text = history.ToString();
        }

        private void SetNotice(string message)
        {
            if (noticeText != null) noticeText.text = message;
        }

        private void SetInteractable(bool interactable)
        {
            if (decreaseButton != null) decreaseButton.interactable = interactable;
            if (increaseButton != null) increaseButton.interactable = interactable;
            if (confirmButton != null) confirmButton.interactable = interactable;
            if (amountField != null) amountField.interactable = interactable;
        }

        private void EnsureGame()
        {
            if (game == null) game = new GemsGame();
        }

        private void WireControls()
        {
            Wire(decreaseButton, Decrease);
            Wire(increaseButton, Increase);
            Wire(confirmButton, Confirm);
            Wire(retryButton, Retry);
            if (amountField != null)
            {
                amountField.onEndEdit.RemoveListener(HandleAmountTyped);
                amountField.onEndEdit.AddListener(HandleAmountTyped);
            }
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (!button) return;
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }
}
