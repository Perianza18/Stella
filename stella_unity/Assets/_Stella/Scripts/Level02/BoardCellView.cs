using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level02
{
    public enum BoardCellVisualState
    {
        Free,
        Blocked,
        Selected,
        ValidPreview,
        InvalidPreview,
        Stella,
        AlienPreview,
        Alien,
        Occupied
    }

    /// <summary>Displays one generated board button and forwards its coordinate.</summary>
    public sealed class BoardCellView : MonoBehaviour
    {
        private static readonly Color FreeColor = new Color(0.38f, 0.42f, 0.43f, 1f);
        private static readonly Color BlockedColor = new Color(0.12f, 0.14f, 0.16f, 1f);
        private static readonly Color SelectedColor = new Color(0.94f, 0.68f, 0.18f, 1f);
        private static readonly Color ValidColor = new Color(0.30f, 0.72f, 0.43f, 1f);
        private static readonly Color InvalidColor = new Color(0.78f, 0.24f, 0.23f, 1f);
        private static readonly Color StellaColor = new Color(0.16f, 0.48f, 0.88f, 1f);
        private static readonly Color AlienPreviewColor = new Color(0.72f, 0.42f, 0.94f, 1f);
        private static readonly Color AlienColor = new Color(0.46f, 0.22f, 0.68f, 1f);
        private static readonly Color OccupiedColor = new Color(0.24f, 0.29f, 0.29f, 1f);

        [SerializeField] private int row;
        [SerializeField] private int column;
        [SerializeField] private Image image;
        [SerializeField] private Button button;
        [SerializeField] private Text label;

        private Action<BoardCellView> pressedCallback;

        public BoardCoordinate Coordinate => new BoardCoordinate(row, column);
        public BoardCellVisualState VisualState { get; private set; }
        public bool InputEnabled => button != null && button.interactable;

        public void Configure(
            int boardRow,
            int boardColumn,
            Image cellImage,
            Button cellButton,
            Text cellLabel,
            Action<BoardCellView> onPressed)
        {
            row = boardRow;
            column = boardColumn;
            image = cellImage;
            button = cellButton;
            label = cellLabel;
            SetPressedCallback(onPressed);
            SetVisualState(row == RuinsBoardGame.CentreIndex && column == RuinsBoardGame.CentreIndex
                ? BoardCellVisualState.Blocked
                : BoardCellVisualState.Free);
        }

        public void SetPressedCallback(Action<BoardCellView> onPressed)
        {
            pressedCallback = onPressed;
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(HandlePressed);
            button.onClick.AddListener(HandlePressed);
        }

        public void SetVisualState(BoardCellVisualState state)
        {
            VisualState = state;
            if (image == null)
            {
                return;
            }

            image.color = ColorFor(state);
            if (label != null)
            {
                label.text = state == BoardCellVisualState.Blocked ? "X" : string.Empty;
                label.color = Color.white;
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            if (button != null)
            {
                bool selectable = VisualState == BoardCellVisualState.Free ||
                                  VisualState == BoardCellVisualState.Selected ||
                                  VisualState == BoardCellVisualState.ValidPreview ||
                                  VisualState == BoardCellVisualState.InvalidPreview;
                button.interactable = enabled && selectable;
            }
        }

        private void HandlePressed()
        {
            if (button != null && button.interactable && pressedCallback != null)
            {
                pressedCallback(this);
            }
        }

        private static Color ColorFor(BoardCellVisualState state)
        {
            switch (state)
            {
                case BoardCellVisualState.Blocked: return BlockedColor;
                case BoardCellVisualState.Selected: return SelectedColor;
                case BoardCellVisualState.ValidPreview: return ValidColor;
                case BoardCellVisualState.InvalidPreview: return InvalidColor;
                case BoardCellVisualState.Stella: return StellaColor;
                case BoardCellVisualState.AlienPreview: return AlienPreviewColor;
                case BoardCellVisualState.Alien: return AlienColor;
                case BoardCellVisualState.Occupied: return OccupiedColor;
                default: return FreeColor;
            }
        }
    }
}
