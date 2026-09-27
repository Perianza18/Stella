using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level01
{
    /// <summary>Renders one fixed Mastermind history row without interpreting its feedback.</summary>
    public sealed class GuessHistoryRowView : MonoBehaviour
    {
        [SerializeField] private Text attemptLabel;
        [SerializeField] private Image[] guessSwatches = new Image[DoubleLockGame.CodeLength];
        [SerializeField] private Image[] feedbackPips = new Image[DoubleLockGame.CodeLength];
        [SerializeField] private Color emptyColor = new Color(0.16f, 0.20f, 0.22f, 0.45f);

        public void Configure(Text attempt, Image[] guesses, Image[] feedback)
        {
            attemptLabel = attempt;
            guessSwatches = guesses;
            feedbackPips = feedback;
            ClearRow();
        }

        public void ClearRow()
        {
            if (attemptLabel != null) attemptLabel.text = "—";
            SetAll(guessSwatches, emptyColor);
            SetAll(feedbackPips, new Color(0.12f, 0.15f, 0.16f, 0.25f));
        }

        public void Render(int attemptNumber, IReadOnlyList<DoubleLockColor> guess, DoubleLockFeedback feedback, System.Func<DoubleLockColor, Color> colorFor)
        {
            if (attemptLabel != null) attemptLabel.text = attemptNumber.ToString();
            for (int index = 0; index < DoubleLockGame.CodeLength; index++)
                guessSwatches[index].color = colorFor(guess[index]);

            int greenCount = feedback.Exact;
            int yellowCount = feedback.Misplaced;
            for (int index = 0; index < feedbackPips.Length; index++)
            {
                feedbackPips[index].color = index < greenCount
                    ? new Color(0.22f, 0.85f, 0.42f, 1f)
                    : index < greenCount + yellowCount
                        ? new Color(0.93f, 0.72f, 0.16f, 1f)
                        : new Color(0.42f, 0.47f, 0.49f, 1f);
            }
        }

        private static void SetAll(IEnumerable<Image> images, Color color)
        {
            if (images == null) return;
            foreach (Image image in images)
                if (image != null) image.color = color;
        }
    }
}
