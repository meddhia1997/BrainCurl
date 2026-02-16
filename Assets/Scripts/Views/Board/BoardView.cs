using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [SerializeField] private LayoutPresetSO layoutPreset;
    [SerializeField] private GameObject cardPrefab;

    private RectTransform _rectTransform;
    private GridLayoutGroup _grid;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _grid = GetComponent<GridLayoutGroup>();
    }

    public void BuildBoard()
    {
        ClearBoard();

        ConfigureGrid();
        SpawnCards();
    }

    private void ConfigureGrid()
    {
        _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _grid.constraintCount = layoutPreset.Cols;

        _grid.spacing = layoutPreset.Spacing;
        _grid.padding = new RectOffset(
            (int)layoutPreset.Padding.x,
            (int)layoutPreset.Padding.x,
            (int)layoutPreset.Padding.y,
            (int)layoutPreset.Padding.y
        );

        CalculateCellSize();
    }

    private void CalculateCellSize()
    {
        float totalWidth = _rectTransform.rect.width;
        float totalHeight = _rectTransform.rect.height;

        float horizontalPadding = layoutPreset.Padding.x * 2;
        float verticalPadding = layoutPreset.Padding.y * 2;

        float horizontalSpacing = layoutPreset.Spacing.x * (layoutPreset.Cols - 1);
        float verticalSpacing = layoutPreset.Spacing.y * (layoutPreset.Rows - 1);

        float cellWidth = (totalWidth - horizontalPadding - horizontalSpacing) / layoutPreset.Cols;
        float cellHeight = (totalHeight - verticalPadding - verticalSpacing) / layoutPreset.Rows;

        _grid.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void SpawnCards()
    {
        int total = layoutPreset.Rows * layoutPreset.Cols;

        for (int i = 0; i < total; i++)
        {
            Instantiate(cardPrefab, transform);
        }
    }

    private void ClearBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
