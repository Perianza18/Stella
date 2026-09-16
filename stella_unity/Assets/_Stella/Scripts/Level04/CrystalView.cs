using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level04
{
    public enum CrystalVisualState
    {
        Active,
        Selected,
        Deactivated
    }

    /// <summary>
    /// Displays one crystal and forwards pointer/tap input to the level controller.
    /// </summary>
    public sealed class CrystalView : MonoBehaviour
    {
        [SerializeField] private Image crystalImage;
        [SerializeField] private Button crystalButton;
        [SerializeField] private Text crystalLabel;
        [SerializeField] private int crystalIndex;

        private Action<CrystalView> pressedCallback;

        public int CrystalIndex
        {
            get { return crystalIndex; }
        }

        public CrystalVisualState State { get; private set; }

        public void Configure(
            int crystalIndex,
            Image image,
            Button button,
            Text label,
            Action<CrystalView> onPressed)
        {
            this.crystalIndex = crystalIndex;
            crystalImage = image;
            crystalButton = button;
            crystalLabel = label;
            SetPressedCallback(onPressed);
            SetState(CrystalVisualState.Active);
        }

        public void SetPressedCallback(Action<CrystalView> onPressed)
        {
            pressedCallback = onPressed;

            if (crystalButton == null)
            {
                return;
            }

            crystalButton.onClick.RemoveListener(HandlePressed);
            crystalButton.onClick.AddListener(HandlePressed);
        }

        public void SetState(CrystalVisualState newState)
        {
            State = newState;

            if (crystalImage == null || crystalButton == null)
            {
                return;
            }

            switch (newState)
            {
                case CrystalVisualState.Active:
                    crystalImage.color = new Color(0.20f, 0.82f, 0.98f, 1f);
                    crystalButton.interactable = true;
                    SetLabelColor(new Color(0.02f, 0.12f, 0.20f, 1f));
                    break;

                case CrystalVisualState.Selected:
                    crystalImage.color = new Color(1f, 0.82f, 0.24f, 1f);
                    crystalButton.interactable = true;
                    SetLabelColor(new Color(0.20f, 0.10f, 0.02f, 1f));
                    break;

                case CrystalVisualState.Deactivated:
                    crystalImage.color = new Color(0.16f, 0.22f, 0.29f, 0.72f);
                    crystalButton.interactable = false;
                    SetLabelColor(new Color(0.48f, 0.56f, 0.64f, 1f));
                    break;
            }
        }

        public void SetInputLocked(bool isLocked)
        {
            if (crystalButton == null)
            {
                return;
            }

            crystalButton.interactable = !isLocked && State != CrystalVisualState.Deactivated;
        }

        private void HandlePressed()
        {
            if (State == CrystalVisualState.Deactivated || !crystalButton.interactable)
            {
                return;
            }

            if (pressedCallback != null)
            {
                pressedCallback(this);
            }
        }

        private void SetLabelColor(Color color)
        {
            if (crystalLabel != null)
            {
                crystalLabel.color = color;
            }
        }
    }
}
