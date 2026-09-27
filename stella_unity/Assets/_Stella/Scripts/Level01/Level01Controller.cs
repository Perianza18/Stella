using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level01
{
    /// <summary>Coordinates the small Unity presentation around DoubleLockGame.</summary>
    public sealed class Level01Controller : MonoBehaviour
    {
        private static readonly Color[] Palette =
        {
            new Color(0.88f, 0.20f, 0.22f, 1f),
            new Color(0.20f, 0.46f, 0.92f, 1f),
            new Color(0.20f, 0.72f, 0.35f, 1f),
            new Color(0.94f, 0.76f, 0.16f, 1f),
            new Color(0.64f, 0.30f, 0.82f, 1f),
            new Color(0.94f, 0.47f, 0.14f, 1f)
        };

        [SerializeField] private Text levelTitle;
        [SerializeField] private Text lock1Label;
        [SerializeField] private Text lock2Label;
        [SerializeField] private Text attemptText;
        [SerializeField] private Text noticeText;
        [SerializeField] private Text systemStateText;
        [SerializeField] private Text stellaStateText;
        [SerializeField] private List<Image> attemptPips = new List<Image>();
        [SerializeField] private List<GuessHistoryRowView> historyRows = new List<GuessHistoryRowView>();
        [SerializeField] private List<Button> slotButtons = new List<Button>();
        [SerializeField] private List<Image> slotSwatches = new List<Image>();
        [SerializeField] private List<Button> colorButtons = new List<Button>();
        [SerializeField] private Button clearButton;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject retryPanel;
        [SerializeField] private Image stellaCardImage;
        [SerializeField] private Image systemCardImage;

        private readonly DoubleLockColor?[] activeGuess = new DoubleLockColor?[DoubleLockGame.CodeLength];
        private DoubleLockGame game;
        private int selectedSlot = -1;
        private bool inputEnabled;

        public DoubleLockGame Game { get { return game; } }
        public IReadOnlyList<GuessHistoryRowView> HistoryRows { get { return historyRows; } }
        public IReadOnlyList<Button> SlotButtons { get { return slotButtons; } }
        public IReadOnlyList<Button> ColorButtons { get { return colorButtons; } }
        public IReadOnlyList<Image> AttemptPips { get { return attemptPips; } }
        public bool InputEnabled { get { return inputEnabled; } }

        private void Awake() { EnsureGame(); }
        private void Start() { InitializeControls(); }

        internal void InitializeControls()
        {
            WireControls();
            ResetSequence();
        }

        internal DoubleLockColor? DraftSlotForTests(int index) { return activeGuess[index]; }
        internal int SelectedSlotForTests { get { return selectedSlot; } }

        public void Configure(
            Text title,
            Text lockOne,
            Text lockTwo,
            Text attempts,
            Text notice,
            Text systemState,
            Text stellaState,
            List<Image> attemptIndicators,
            List<GuessHistoryRowView> rows,
            List<Button> slots,
            List<Image> swatches,
            List<Button> colours,
            Button clear,
            Button submit,
            GameObject retryOverlay,
            Button retry,
            Image stellaCard,
            Image systemCard)
        {
            levelTitle = title;
            lock1Label = lockOne;
            lock2Label = lockTwo;
            attemptText = attempts;
            noticeText = notice;
            systemStateText = systemState;
            stellaStateText = stellaState;
            attemptPips = attemptIndicators;
            historyRows = rows;
            slotButtons = slots;
            slotSwatches = swatches;
            colorButtons = colours;
            clearButton = clear;
            submitButton = submit;
            retryPanel = retryOverlay;
            retryButton = retry;
            stellaCardImage = stellaCard;
            systemCardImage = systemCard;
            EnsureGame();
            WireControls();
            ResetSequence();
        }

        public void HandleColorPressed(int colorIndex)
        {
            if (!inputEnabled || colorIndex < 0 || colorIndex >= DoubleLockGame.PaletteSize) return;
            int target = selectedSlot >= 0 ? selectedSlot : FirstEmptySlot();
            if (target < 0) return;
            activeGuess[target] = (DoubleLockColor)colorIndex;
            selectedSlot = -1;
            RefreshGuessPresentation();
        }

        public void HandleSlotPressed(int slotIndex)
        {
            if (!inputEnabled || slotIndex < 0 || slotIndex >= DoubleLockGame.CodeLength) return;
            if (!activeGuess[slotIndex].HasValue) return;
            selectedSlot = selectedSlot == slotIndex ? -1 : slotIndex;
            RefreshGuessPresentation();
        }

        public void ClearGuess()
        {
            if (!inputEnabled) return;
            ClearActiveGuess();
            RefreshGuessPresentation();
        }

        public void SubmitGuess()
        {
            if (!inputEnabled || !IsGuessComplete()) return;
            List<DoubleLockColor> guess = new List<DoubleLockColor>(DoubleLockGame.CodeLength);
            for (int index = 0; index < activeGuess.Length; index++) guess.Add(activeGuess[index].Value);
            DoubleLockSubmitResult result = game.Submit(guess);
            inputEnabled = false;
            RenderAttemptIndicators();

            if (result.BeginnerLuckTriggered)
            {
                ClearHistory();
                ClearActiveGuess();
                SetNotice("BEGINNER'S LUCK!\nThe lock resets with a new code.");
                SetSystemState("BEGINNER'S LUCK!", false);
                if (Application.isPlaying) StartCoroutine(ResumeAfterBeginnerLuck());
                else BeginPlaying();
                return;
            }

            int rowIndex = result.AttemptsUsed - 1;
            if (rowIndex >= 0 && rowIndex < historyRows.Count)
                historyRows[rowIndex].Render(result.AttemptsUsed, guess, result.Feedback, ColorFor);
            ClearActiveGuess();

            if (result.LockAdvanced)
            {
                ClearHistory();
                SetNotice("COLOURS MAY REPEAT");
                SetSystemState("LOCK 1 OPENED", true);
                UpdateLockIndicators();
                if (Application.isPlaying) StartCoroutine(BeginLockTwoAfterDelay());
                else BeginPlaying();
                return;
            }

            if (result.Failed)
            {
                RevealSecret(result.RevealedSecret);
                inputEnabled = false;
                SetNotice(result.LockNumber == 2 ? "SECURITY RESET" : "LOCK FAILED");
                SetSystemState(result.LockNumber == 2 ? "SECURITY RESET" : "LOCK FAILED", false);
                retryPanel.SetActive(true);
                RefreshAll();
                return;
            }

            if (result.Completed)
            {
                inputEnabled = false;
                SetNotice("ARCHIVE ACCESS GRANTED");
                SetSystemState("ARCHIVE ACCESS GRANTED", true);
                UpdateLockIndicators();
                RefreshAll();
                return;
            }

            SetNotice(string.Empty);
            BeginPlaying();
        }

        public void Retry()
        {
            StopAllCoroutines();
            ResetSequence();
        }

        internal void SetGameForTests(DoubleLockGame replacement)
        {
            game = replacement ?? throw new ArgumentNullException(nameof(replacement));
            ResetPresentationOnly();
        }

        internal void SubmitForTests() { SubmitGuess(); }

        private IEnumerator ResumeAfterBeginnerLuck()
        {
            yield return new WaitForSeconds(0.65f);
            SetNotice(string.Empty);
            BeginPlaying();
        }

        private IEnumerator BeginLockTwoAfterDelay()
        {
            yield return new WaitForSeconds(0.65f);
            SetNotice("COLOURS MAY REPEAT");
            BeginPlaying();
        }

        private void BeginPlaying()
        {
            game.ResumePlaying();
            inputEnabled = true;
            SetSystemState(game.CurrentLock == 1 ? "LOCK 1 ACTIVE" : "LOCK 2 ACTIVE", true);
            UpdateLockIndicators();
            RefreshAll();
        }

        private void ResetSequence()
        {
            StopAllCoroutines();
            EnsureGame();
            game.ResetSequence();
            retryPanel.SetActive(false);
            ClearHistory();
            ClearActiveGuess();
            SetNotice(string.Empty);
            inputEnabled = true;
            SetSystemState("LOCK 1 ACTIVE", true);
            UpdateLockIndicators();
            RefreshAll();
        }

        private void ResetPresentationOnly()
        {
            retryPanel.SetActive(false);
            ClearHistory();
            ClearActiveGuess();
            inputEnabled = true;
            SetNotice(string.Empty);
            SetSystemState("LOCK " + game.CurrentLock + " ACTIVE", true);
            UpdateLockIndicators();
            RefreshAll();
        }

        private void RefreshAll()
        {
            RenderAttemptIndicators();
            RefreshGuessPresentation();
            if (game == null) return;
            attemptText.text = "LOCK " + game.CurrentLock + " OF 2 — ATTEMPT " + Math.Min(game.AttemptsUsed + 1, DoubleLockGame.MaxAttempts) + "/6";
        }

        private void RefreshGuessPresentation()
        {
            for (int index = 0; index < slotSwatches.Count; index++)
            {
                bool filled = activeGuess[index].HasValue;
                slotSwatches[index].color = filled ? ColorFor(activeGuess[index].Value) : new Color(0.16f, 0.20f, 0.22f, 1f);
                Outline outline = slotButtons[index].GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = inputEnabled && selectedSlot == index;
                    outline.effectColor = Color.white;
                }
            }
            clearButton.interactable = inputEnabled && AnyGuessFilled();
            submitButton.interactable = inputEnabled && IsGuessComplete();
        }

        private void RenderAttemptIndicators()
        {
            int used = game == null ? 0 : game.AttemptsUsed;
            for (int index = 0; index < attemptPips.Count; index++)
                attemptPips[index].color = index < used
                    ? new Color(0.80f, 0.88f, 0.90f, 1f)
                    : new Color(0.20f, 0.25f, 0.27f, 0.45f);
        }

        private void RevealSecret(IReadOnlyList<DoubleLockColor> secret)
        {
            if (secret == null) return;
            ClearActiveGuess();
            for (int index = 0; index < DoubleLockGame.CodeLength; index++) activeGuess[index] = secret[index];
        }

        private void ClearHistory()
        {
            foreach (GuessHistoryRowView row in historyRows) row.ClearRow();
        }

        private void ClearActiveGuess()
        {
            for (int index = 0; index < activeGuess.Length; index++) activeGuess[index] = null;
            selectedSlot = -1;
        }

        private void UpdateLockIndicators()
        {
            if (lock1Label == null || lock2Label == null || game == null) return;
            lock1Label.text = game.CurrentLock > 1 || game.IsCompleted ? "✓ LOCK 1" : "LOCK 1 ACTIVE";
            lock2Label.text = game.IsCompleted ? "✓ LOCK 2" : game.CurrentLock == 2 ? "LOCK 2 ACTIVE" : "LOCK 2";
        }

        private void SetNotice(string message)
        {
            if (noticeText != null) noticeText.text = message;
        }

        private void SetSystemState(string message, bool active)
        {
            if (systemStateText != null) systemStateText.text = message;
            if (systemCardImage != null) systemCardImage.color = active
                ? new Color(0.24f, 0.53f, 0.62f, 1f)
                : new Color(0.39f, 0.22f, 0.25f, 1f);
            if (stellaStateText != null) stellaStateText.text = "STELLA\n" + (inputEnabled ? "READY" : "WAITING");
            if (stellaCardImage != null) stellaCardImage.color = inputEnabled
                ? new Color(0.18f, 0.52f, 0.86f, 1f)
                : new Color(0.28f, 0.33f, 0.37f, 1f);
        }

        private int FirstEmptySlot()
        {
            for (int index = 0; index < activeGuess.Length; index++)
                if (!activeGuess[index].HasValue) return index;
            return -1;
        }

        private bool AnyGuessFilled()
        {
            for (int index = 0; index < activeGuess.Length; index++)
                if (activeGuess[index].HasValue) return true;
            return false;
        }

        private bool IsGuessComplete()
        {
            return FirstEmptySlot() < 0;
        }

        private void EnsureGame()
        {
            if (game == null) game = new DoubleLockGame();
        }

        private void WireControls()
        {
            for (int index = 0; index < colorButtons.Count; index++)
            {
                int colorIndex = index;
                colorButtons[index].onClick.RemoveAllListeners();
                colorButtons[index].onClick.AddListener(() => HandleColorPressed(colorIndex));
            }
            for (int index = 0; index < slotButtons.Count; index++)
            {
                int slotIndex = index;
                slotButtons[index].onClick.RemoveAllListeners();
                slotButtons[index].onClick.AddListener(() => HandleSlotPressed(slotIndex));
            }
            Wire(clearButton, ClearGuess);
            Wire(submitButton, SubmitGuess);
            Wire(retryButton, Retry);
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (!button) return;
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static Color ColorFor(DoubleLockColor color) { return Palette[(int)color]; }
    }
}
