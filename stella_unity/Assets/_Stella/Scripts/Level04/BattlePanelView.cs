using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level04
{
    /// <summary>
    /// Keeps the single active battle card and its communication text in sync.
    /// </summary>
    public sealed class BattlePanelView : MonoBehaviour
    {
        [SerializeField] private Image battleCardImage;
        [SerializeField] private Text battleCardLabel;
        [SerializeField] private Text dialogueText;

        public string VisibleCardLabel
        {
            get { return battleCardLabel == null ? string.Empty : battleCardLabel.text; }
        }

        public void Configure(Image cardImage, Text cardLabel, Text dialogue)
        {
            battleCardImage = cardImage;
            battleCardLabel = cardLabel;
            dialogueText = dialogue;
        }

        public void ShowStartingChoice()
        {
            SetCard("RITUAL", new Color(0.30f, 0.36f, 0.48f, 1f));
            SetDialogue("Who should begin the Krik greeting?");
        }

        public void ShowStellaTurn(int remainingCrystals)
        {
            SetCard("STELLA", new Color(0.18f, 0.52f, 0.86f, 1f));
            SetDialogue("Choose 1, 2, or 3 crystals.\n" + remainingCrystals + " remain.");
        }

        public void ShowKrikTurn(int remainingCrystals)
        {
            SetCard("KRIK", new Color(0.50f, 0.27f, 0.68f, 1f));
            SetDialogue("⌁ ◇ ⋮  The Krik considers the greeting...\n" + remainingCrystals + " remain.");
        }

        public void ShowStellaVictory()
        {
            SetCard("STELLA", new Color(0.18f, 0.52f, 0.86f, 1f));
            SetDialogue("The Krik accepts Stella's greeting.");
        }

        public void ShowKrikVictory()
        {
            SetCard("KRIK", new Color(0.50f, 0.27f, 0.68f, 1f));
            SetDialogue("⌁⌁◇⋮! The Krik begins an incomprehensibly long ceremonial speech...");
        }

        private void SetCard(string label, Color color)
        {
            if (battleCardLabel != null)
            {
                battleCardLabel.text = label;
            }

            if (battleCardImage != null)
            {
                battleCardImage.color = color;
            }
        }

        private void SetDialogue(string message)
        {
            if (dialogueText != null)
            {
                dialogueText.text = message;
            }
        }
    }
}
