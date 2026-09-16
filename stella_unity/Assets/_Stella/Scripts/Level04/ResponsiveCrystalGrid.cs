using UnityEngine;
using UnityEngine.UI;

namespace Stella.Level04
{
    /// <summary>
    /// Sizes a fixed two-by-ten grid without changing any crystal's position.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class ResponsiveCrystalGrid : MonoBehaviour
    {
        [SerializeField] private int columns = 10;
        [SerializeField] private int rows = 2;
        [SerializeField] private float spacing = 12f;

        private RectTransform gridRect;
        private GridLayoutGroup gridLayout;

        private void Awake()
        {
            CacheComponents();
            UpdateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateCellSize();
        }
#endif

        private void CacheComponents()
        {
            if (gridRect == null)
            {
                gridRect = GetComponent<RectTransform>();
            }

            if (gridLayout == null)
            {
                gridLayout = GetComponent<GridLayoutGroup>();
            }
        }

        private void UpdateCellSize()
        {
            CacheComponents();

            if (gridRect == null || gridLayout == null || columns <= 0 || rows <= 0)
            {
                return;
            }

            float availableWidth = gridRect.rect.width - (spacing * (columns - 1));
            float availableHeight = gridRect.rect.height - (spacing * (rows - 1));
            float cellWidth = availableWidth / columns;
            float cellHeight = availableHeight / rows;
            float squareSize = Mathf.Max(1f, Mathf.Min(cellWidth, cellHeight));

            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;
            gridLayout.spacing = new Vector2(spacing, spacing);
            gridLayout.cellSize = new Vector2(squareSize, squareSize);
        }
    }
}
