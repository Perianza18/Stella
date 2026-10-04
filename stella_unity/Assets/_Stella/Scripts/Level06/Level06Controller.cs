using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level06
{
    public enum EggAssignment
    {
        None,
        Left,
        Right
    }

    /// <summary>Coordinates the small Unity presentation around GalacticCustomsGame.</summary>
    public sealed class Level06Controller : MonoBehaviour
    {
        private static readonly Color UnassignedColor = new Color(0.16f, 0.21f, 0.23f, 1f);
        private static readonly Color LeftColor = new Color(0.20f, 0.46f, 0.92f, 1f);
        private static readonly Color RightColor = new Color(0.88f, 0.20f, 0.22f, 1f);
        private static readonly Color EliminatedColor = new Color(0.10f, 0.12f, 0.13f, 0.6f);
        private static readonly Color SelectedColor = new Color(0.20f, 0.72f, 0.35f, 1f);

        [SerializeField] private Text usesText;
        [SerializeField] private Text candidateCountText;
        [SerializeField] private Text noticeText;
        [SerializeField] private List<Button> eggButtons = new List<Button>();
        [SerializeField] private List<Image> eggSwatches = new List<Image>();
        [SerializeField] private Button weighButton;
        [SerializeField] private Button guessButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject retryPanel;

        private GalacticCustomsGame game;
        private readonly Dictionary<int, EggAssignment> assignments = new Dictionary<int, EggAssignment>();
        private int selectedGuess = -1;

        public GalacticCustomsGame Game { get { return game; } }
        public IReadOnlyList<Button> EggButtons { get { return eggButtons; } }
        public int SelectedGuessForTests { get { return selectedGuess; } }

        private void Awake() { EnsureGame(); }
        private void Start() { InitializeControls(); }

        internal void InitializeControls()
        {
            WireControls();
            ResetSequence();
        }

        public void Configure(
            Text uses,
            Text candidateCount,
            Text notice,
            List<Button> eggs,
            List<Image> swatches,
            Button weigh,
            Button guess,
            GameObject retryOverlay,
            Button retry)
        {
            usesText = uses;
            candidateCountText = candidateCount;
            noticeText = notice;
            eggButtons = eggs;
            eggSwatches = swatches;
            weighButton = weigh;
            guessButton = guess;
            retryPanel = retryOverlay;
            retryButton = retry;
            EnsureGame();
            WireControls();
            ResetSequence();
        }

        public void HandleEggPressed(int eggNumber)
        {
            if (!game.Candidates.Contains(eggNumber)) return;

            if (game.ReadyToGuess)
            {
                selectedGuess = eggNumber;
                RefreshEggs();
                return;
            }

            if (!game.CanWeigh) return;

            EggAssignment current = assignments.TryGetValue(eggNumber, out EggAssignment value) ? value : EggAssignment.None;
            assignments[eggNumber] = current switch
            {
                EggAssignment.None => EggAssignment.Left,
                EggAssignment.Left => EggAssignment.Right,
                _ => EggAssignment.None
            };
            RefreshEggs();
        }

        public void Weigh()
        {
            if (!game.CanWeigh) return;

            List<int> left = new List<int>();
            List<int> right = new List<int>();
            foreach (KeyValuePair<int, EggAssignment> pair in assignments)
            {
                if (pair.Value == EggAssignment.Left) left.Add(pair.Key);
                else if (pair.Value == EggAssignment.Right) right.Add(pair.Key);
            }

            if (left.Count == 0 || left.Count != right.Count) return;

            WeighingOutcome outcome = game.Weigh(left, right);
            SetNotice(DescribeOutcome(outcome));
            assignments.Clear();
            selectedGuess = -1;
            RefreshAll();
        }

        public void Guess()
        {
            if (!game.ReadyToGuess || selectedGuess < 0) return;

            bool won = game.Guess(selectedGuess);
            SetNotice(won
                ? "CORRECT! AGENT GLIP OPENS THE WAY."
                : "INCORRECT. THE STOWAWAY WAS NEVER PINNED DOWN.");
            if (retryPanel != null) retryPanel.SetActive(true);
            RefreshAll();
        }

        public void Retry()
        {
            ResetSequence();
        }

        internal void SetGameForTests(GalacticCustomsGame replacement)
        {
            game = replacement ?? throw new ArgumentNullException(nameof(replacement));
            assignments.Clear();
            selectedGuess = -1;
            RefreshAll();
        }

        internal void WeighForTests() { Weigh(); }
        internal void GuessForTests() { Guess(); }

        private static string DescribeOutcome(WeighingOutcome outcome)
        {
            switch (outcome)
            {
                case WeighingOutcome.Left: return "THE LEFT PAN IS HEAVIER.";
                case WeighingOutcome.Right: return "THE RIGHT PAN IS HEAVIER.";
                default: return "THE SCALE STAYS BALANCED.";
            }
        }

        private void ResetSequence()
        {
            EnsureGame();
            game = new GalacticCustomsGame();
            assignments.Clear();
            selectedGuess = -1;
            SetNotice(string.Empty);
            if (retryPanel != null) retryPanel.SetActive(false);
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshEggs(); // also refreshes the uses/suspect-count status text
        }

        private void RefreshStatus()
        {
            if (usesText != null) usesText.text = "BATTERY: " + game.UsesRemaining + "/" + GalacticCustomsGame.BatteryUses;
            if (candidateCountText != null) candidateCountText.text = game.Candidates.Count + " SUSPECT(S) LEFT";

            if (weighButton != null)
            {
                int leftCount = 0, rightCount = 0;
                foreach (EggAssignment value in assignments.Values)
                {
                    if (value == EggAssignment.Left) leftCount++;
                    else if (value == EggAssignment.Right) rightCount++;
                }
                weighButton.interactable = game.CanWeigh && leftCount > 0 && leftCount == rightCount;
                weighButton.gameObject.SetActive(!game.ReadyToGuess);
            }

            if (guessButton != null)
            {
                guessButton.interactable = game.ReadyToGuess && selectedGuess >= 0;
                guessButton.gameObject.SetActive(game.ReadyToGuess && !game.HasGuessed);
            }
        }

        private void RefreshEggs()
        {
            for (int index = 0; index < eggButtons.Count; index++)
            {
                int eggNumber = index + 1;
                bool isCandidate = game.Candidates.Contains(eggNumber);
                eggButtons[index].interactable = isCandidate && !game.HasGuessed;

                if (index >= eggSwatches.Count) continue;

                if (!isCandidate)
                {
                    eggSwatches[index].color = EliminatedColor;
                    continue;
                }

                if (game.ReadyToGuess)
                {
                    eggSwatches[index].color = eggNumber == selectedGuess ? SelectedColor : UnassignedColor;
                    continue;
                }

                EggAssignment assignment = assignments.TryGetValue(eggNumber, out EggAssignment value) ? value : EggAssignment.None;
                eggSwatches[index].color = assignment switch
                {
                    EggAssignment.Left => LeftColor,
                    EggAssignment.Right => RightColor,
                    _ => UnassignedColor
                };
            }

            RefreshStatus();
        }

        private void SetNotice(string message)
        {
            if (noticeText != null) noticeText.text = message;
        }

        private void EnsureGame()
        {
            if (game == null) game = new GalacticCustomsGame();
        }

        private void WireControls()
        {
            for (int index = 0; index < eggButtons.Count; index++)
            {
                int eggNumber = index + 1;
                eggButtons[index].onClick.RemoveAllListeners();
                eggButtons[index].onClick.AddListener(() => HandleEggPressed(eggNumber));
            }

            Wire(weighButton, Weigh);
            Wire(guessButton, Guess);
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
