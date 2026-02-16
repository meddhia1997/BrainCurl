using UnityEngine;
using UnityEngine.UI;

public sealed class BoardView : MonoBehaviour
{
    [SerializeField] private LayoutPresetSO layoutPreset;
    [SerializeField] private GameObject cardPrefab;

    private RectTransform _rectTransform;
    private GridLayoutGroup _grid;

    public int Rows => layoutPreset.Rows;
    public int Cols => layoutPreset.Cols;
    public int TotalCards => layoutPreset.Rows * layoutPreset.Cols;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _grid = GetComponent<GridLayoutGroup>();
    }

    public void BuildBoard(IEventBus bus, BoardState board, CardCatalogSO catalog)
    {
        ClearBoard();
        ConfigureGrid();
        SpawnCards(bus, board, catalog);
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

        float horizontalPadding = layoutPreset.Padding.x * 2f;
        float verticalPadding = layoutPreset.Padding.y * 2f;

        float horizontalSpacing = layoutPreset.Spacing.x * (layoutPreset.Cols - 1);
        float verticalSpacing = layoutPreset.Spacing.y * (layoutPreset.Rows - 1);

        float cellWidth = (totalWidth - horizontalPadding - horizontalSpacing) / layoutPreset.Cols;
        float cellHeight = (totalHeight - verticalPadding - verticalSpacing) / layoutPreset.Rows;

        _grid.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void SpawnCards(IEventBus bus, BoardState board, CardCatalogSO catalog)
    {
        int total = TotalCards;

        int pairsNeeded = total / 2;
        if (catalog == null || catalog.Sprites == null || catalog.Sprites.Length < pairsNeeded)
        {
            Debug.LogError($"CardCatalogSO missing/too small. Need at least {pairsNeeded} sprites.");
            return;
        }

        for (int i = 0; i < total; i++)
        {
            var go = Instantiate(cardPrefab, transform);

            var view = go.GetComponent<CardView>();
            var anim = go.GetComponent<CardAnimator>();

            int pairId = board.GetPairId(i);
            Sprite sprite = catalog.Sprites[pairId];

            bool isMatched = board.States[i] == CardState.Matched;

            view.Init(i, sprite, bus, startInteractable: !isMatched);
            anim.Init(i, bus, startFaceUp: isMatched);
        }
    }

    private void ClearBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}
