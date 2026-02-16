using UnityEngine;
using UnityEngine.UI;

public sealed class BoardView : MonoBehaviour
{
    [SerializeField] private LayoutPresetSO defaultPreset;
    [SerializeField] private GameObject cardPrefab;

    private RectTransform _rectTransform;
    private GridLayoutGroup _grid;

    public LayoutPresetSO CurrentPreset { get; private set; }

    public int Rows => CurrentPreset != null ? CurrentPreset.Rows : (defaultPreset != null ? defaultPreset.Rows : 0);
    public int Cols => CurrentPreset != null ? CurrentPreset.Cols : (defaultPreset != null ? defaultPreset.Cols : 0);
    public int TotalCards => Rows * Cols;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _grid = GetComponent<GridLayoutGroup>();
        CurrentPreset = defaultPreset;
    }

    public void BuildBoard(IEventBus bus, BoardState board, CardCatalogSO catalog)
    {
        BuildBoard(CurrentPreset, bus, board, catalog);
    }

    public void BuildBoard(LayoutPresetSO preset, IEventBus bus, BoardState board, CardCatalogSO catalog)
    {
        if (preset == null)
        {
            Debug.LogError("BoardView.BuildBoard called with null preset.");
            return;
        }

        CurrentPreset = preset;

        ClearBoard();
        ConfigureGrid(preset);
        SpawnCards(preset, bus, board, catalog);
    }

    public void ClearBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void ConfigureGrid(LayoutPresetSO preset)
    {
        _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _grid.constraintCount = preset.Cols;

        _grid.spacing = preset.Spacing;
        _grid.padding = new RectOffset(
            (int)preset.Padding.x,
            (int)preset.Padding.x,
            (int)preset.Padding.y,
            (int)preset.Padding.y
        );

        CalculateCellSize(preset);
    }

    private void CalculateCellSize(LayoutPresetSO preset)
    {
        float totalWidth = _rectTransform.rect.width;
        float totalHeight = _rectTransform.rect.height;

        float horizontalPadding = preset.Padding.x * 2f;
        float verticalPadding = preset.Padding.y * 2f;

        float horizontalSpacing = preset.Spacing.x * (preset.Cols - 1);
        float verticalSpacing = preset.Spacing.y * (preset.Rows - 1);

        float cellWidth = (totalWidth - horizontalPadding - horizontalSpacing) / preset.Cols;
        float cellHeight = (totalHeight - verticalPadding - verticalSpacing) / preset.Rows;

        _grid.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void SpawnCards(LayoutPresetSO preset, IEventBus bus, BoardState board, CardCatalogSO catalog)
    {
        int total = preset.Rows * preset.Cols;

        int pairsNeeded = total / 2;
        if (catalog == null || catalog.Sprites == null || catalog.Sprites.Length < pairsNeeded)
        {
            Debug.LogError($"CardCatalogSO missing/too small. Need at least {pairsNeeded} sprites for layout {preset.Rows}x{preset.Cols}.");
            return;
        }

        for (int i = 0; i < total; i++)
        {
            var go = Object.Instantiate(cardPrefab, transform);

            var view = go.GetComponent<CardView>();
            var anim = go.GetComponent<CardAnimator>();

            int pairId = board.GetPairId(i);
            Sprite sprite = catalog.Sprites[pairId];

            bool isMatched = board.States[i] == CardState.Matched;

            view.Init(i, sprite, bus, startInteractable: !isMatched);
            anim.Init(i, bus, startFaceUp: isMatched);
        }
    }
}
