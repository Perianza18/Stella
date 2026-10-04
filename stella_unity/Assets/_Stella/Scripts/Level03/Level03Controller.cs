using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level03
{
    public enum Axis
    {
        Row,
        Column
    }

    /// <summary>Coordinates the small Unity presentation around CollectorGame.</summary>
    public sealed class Level03Controller : MonoBehaviour
    {
        [SerializeField] private Text positionText;
        [SerializeField] private Text axisHintText;
        [SerializeField] private Text amountText;
        [SerializeField] private Text noticeText;
        [SerializeField] private Button rowAxisButton;
        [SerializeField] private Button columnAxisButton;
        [SerializeField] private Text rowAxisLabel;
        [SerializeField] private Text columnAxisLabel;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject retryPanel;

        private CollectorGame game;
        private Axis selectedAxis = Axis.Row;
        private int selectedAmount = 1;

        public CollectorGame Game { get { return game; } }
        public Axis SelectedAxisForTests { get { return selectedAxis; } }
        public int SelectedAmountForTests { get { return selectedAmount; } }

        private void Awake() { EnsureGame(); }
        private void Start() { InitializeControls(); }

        internal void InitializeControls()
        {
            WireControls();
            ResetSequence();
        }

        public void Configure(
            Text position,
            Text axisHint,
            Text amount,
            Text notice,
            Button rowAxis,
            Button columnAxis,
            Text rowLabel,
            Text columnLabel,
            Button decrease,
            Button increase,
            Button confirm,
            GameObject retryOverlay,
            Button retry)
        {
            positionText = position;
            axisHintText = axisHint;
            amountText = amount;
            noticeText = notice;
            rowAxisButton = rowAxis;
            columnAxisButton = columnAxis;
            rowAxisLabel = rowLabel;
            columnAxisLabel = columnLabel;
            decreaseButton = decrease;
            increaseButton = increase;
            confirmButton = confirm;
            retryPanel = retryOverlay;
            retryButton = retry;
            EnsureGame();
            WireControls();
            ResetSequence();
        }

        public void SelectRowAxis()
        {
            if (game.Row <= 0) return;
            selectedAxis = Axis.Row;
            selectedAmount = 1;
            RefreshControls();
        }

        public void SelectColumnAxis()
        {
            if (game.Column <= 0) return;
            selectedAxis = Axis.Column;
            selectedAmount = 1;
            RefreshControls();
        }

        public void Decrease()
        {
            selectedAmount = Math.Max(1, selectedAmount - 1);
            RefreshControls();
        }

        public void Increase()
        {
            int limit = selectedAxis == Axis.Row ? game.Row : game.Column;
            selectedAmount = Math.Min(Math.Max(1, limit), selectedAmount + 1);
            RefreshControls();
        }

        public void Confirm()
        {
            if (game.IsGameOver) return;

            ApplyStellaMove();
            if (CheckGameOver()) return;

            (int row, int column) = game.OptimalMove(game.Row, game.Column);
            game.ApplyMove(row, column);
            if (CheckGameOver()) return;

            PrepareNextTurn();
        }

        public void Retry()
        {
            ResetSequence();
        }

        internal void SetGameForTests(CollectorGame replacement)
        {
            game = replacement ?? throw new ArgumentNullException(nameof(replacement));
            PrepareNextTurn();
        }

        internal void ConfirmForTests() { Confirm(); }

        private void ApplyStellaMove()
        {
            int newRow = selectedAxis == Axis.Row ? game.Row - selectedAmount : game.Row;
            int newColumn = selectedAxis == Axis.Column ? game.Column - selectedAmount : game.Column;
            game.ApplyMove(newRow, newColumn);
        }

        private bool CheckGameOver()
        {
            if (!game.IsGameOver) return false;

            bool stellaWon = game.Winner == Participant.Stella;
            SetNotice(stellaWon
                ? "THE TOKEN LANDS ON (0,0)! THE COLLECTOR REVEALS THE CLUE."
                : "THE COLLECTOR REACHES (0,0) FIRST.");
            if (retryPanel != null) retryPanel.SetActive(true);
            SetInteractable(false);
            RefreshPosition();
            return true;
        }

        private void PrepareNextTurn()
        {
            if (game.Row <= 0 && game.Column > 0) selectedAxis = Axis.Column;
            else if (game.Column <= 0 && game.Row > 0) selectedAxis = Axis.Row;
            selectedAmount = 1;
            RefreshControls();
        }

        private void ResetSequence()
        {
            EnsureGame();
            game = new CollectorGame();
            selectedAxis = Axis.Row;
            selectedAmount = 1;
            SetNotice(string.Empty);
            if (retryPanel != null) retryPanel.SetActive(false);
            SetInteractable(true);
            RefreshControls();
        }

        private void RefreshControls()
        {
            RefreshPosition();
            if (axisHintText != null)
                axisHintText.text = selectedAxis == Axis.Row ? "REDUCING THE ROW" : "REDUCING THE COLUMN";
            if (amountText != null) amountText.text = selectedAmount.ToString();
            if (rowAxisLabel != null) rowAxisLabel.text = "ROW: " + game.Row;
            if (columnAxisLabel != null) columnAxisLabel.text = "COLUMN: " + game.Column;

            bool interactable = !game.IsGameOver;
            int limit = selectedAxis == Axis.Row ? game.Row : game.Column;
            if (decreaseButton != null) decreaseButton.interactable = interactable && selectedAmount > 1;
            if (increaseButton != null) increaseButton.interactable = interactable && selectedAmount < limit;
            if (confirmButton != null) confirmButton.interactable = interactable && limit > 0;
            if (rowAxisButton != null) rowAxisButton.interactable = interactable && game.Row > 0;
            if (columnAxisButton != null) columnAxisButton.interactable = interactable && game.Column > 0;
        }

        private void RefreshPosition()
        {
            if (positionText != null && game != null)
                positionText.text = "(" + game.Row + ", " + game.Column + ")";
        }

        private void SetNotice(string message)
        {
            if (noticeText != null) noticeText.text = message;
        }

        private void SetInteractable(bool interactable)
        {
            if (rowAxisButton != null) rowAxisButton.interactable = interactable;
            if (columnAxisButton != null) columnAxisButton.interactable = interactable;
            if (decreaseButton != null) decreaseButton.interactable = interactable;
            if (increaseButton != null) increaseButton.interactable = interactable;
            if (confirmButton != null) confirmButton.interactable = interactable;
        }

        private void EnsureGame()
        {
            if (game == null) game = new CollectorGame();
        }

        private void WireControls()
        {
            Wire(rowAxisButton, SelectRowAxis);
            Wire(columnAxisButton, SelectColumnAxis);
            Wire(decreaseButton, Decrease);
            Wire(increaseButton, Increase);
            Wire(confirmButton, Confirm);
            Wire(retryButton, Retry);
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (!button) return;
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }
}
