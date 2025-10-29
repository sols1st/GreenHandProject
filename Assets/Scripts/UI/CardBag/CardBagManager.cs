using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBagManager : MonoBehaviour
{
    public static CardBagManager Instance { get; private set; }

    [Header("Root")] [SerializeField] GameObject cardPanel; // 面板根
    [SerializeField] GameObject useCard; // "使用"按钮所在区域

    [Header("Layout")] [Min(1)] public int columns = 1; // 每行卡牌数
    [SerializeField] Vector2 cellSize = new Vector2(160, 220);
    [SerializeField] Vector2 cellSpacing = new Vector2(16, 16);

    [Header("List")] [SerializeField] ScrollRect scrollRect;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] RectTransform content; // Content 的 RectTransform（顶对齐）
    [SerializeField] RectTransform viewport; // ScrollRect 下的 Viewport
    [SerializeField] CardItem itemPrefab;
    public GameObject cardBagBackground;

    public GameObject openCardBagButton;
    // 选中状态
    private Card _selectedCard;
    private CardItem _selectedItem;

    // 数据/池
    private readonly List<CardItem> pool = new();

    // 其它
    private string _openMode = "default";
    private string _type = "Item";

    // 行数
    int _rows;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupGrid();
    }

    // 设置网格与 ScrollRect/Viewport/Content
    private void SetupGrid()
    {
        // ScrollRect 基本设置
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 3f; // Unity默认值

        // 设置ScrollRect的基本组件
        scrollRect.viewport = viewport;
        scrollRect.content = content;


        // Grid 基本设置
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);
        grid.cellSize = cellSize;
        grid.spacing = cellSpacing;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;

        // 统一左右 padding，确保左右边距与行间距逻辑一致
        // var p = grid.padding;
        // p.left = Mathf.RoundToInt(cellSpacing.x + viewportOffsetLeft);
        // p.right = Mathf.RoundToInt(cellSpacing.x + viewportOffsetRight);
        // p.top = Mathf.RoundToInt(viewportOffsetTop);
        // p.bottom = Mathf.RoundToInt(viewportOffsetBottom);
        // grid.padding = p;

        // Content 顶对齐 + 水平拉伸
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0f, 1f);

        
        // 调试输出Grid padding设置
       // Debug.Log($"Grid padding - left: {p.left}, right: {p.right}, top: {p.top}, bottom: {p.bottom}");
    }

    public void Open(string openMode = "default", bool isForce = false, string[] functions = null)
    {
        if (openMode != "default")
        {
            _openMode = openMode;
            if (_openMode == "Dialogue")
            {
                if (cardPanel.activeSelf)
                {
                    openCardBagButton.GetComponent<OpenCardBagButton>().TriggerOnClick();
                }
                openCardBagButton.SetActive(false);
            }
            cardBagBackground.SetActive(true);
            if (useCard != null) useCard.SetActive(true);
        }

        if (!cardPanel.activeSelf) cardPanel.SetActive(true);
        Refresh(false, _type, isForce, functions);
    }

    public void Close()
    {
        if (useCard != null) useCard.SetActive(false);
        if (cardPanel != null) cardPanel.SetActive(false);
    }

    public void RefreshNewCard()
    {
        if (cardPanel.activeSelf)
        {
            Refresh(true, _type);
        }
    }

    public void UpdateSelectedCard(CardItem cardItem, Card card)
    {
        if (_openMode == "default") return;
        _selectedItem?.UpdateBackground();

        if (_selectedItem == cardItem)
        {
            _selectedCard = null;
            _selectedItem = null;
            return;
        }

        _selectedCard = card;
        _selectedItem = cardItem;
        cardItem?.UpdateBackground();
    }

    public void UseCard()
    {
        if (_selectedCard == null) return;
        switch (_openMode)
        {
            case "DeductionManager":
                DeductionManager.Instance.UseCard(_selectedCard);
                break;
            case "Dialogue":
                // todo 对话使用
                DialogueManager.Instance.OnCardSelected(_selectedCard.id);
                openCardBagButton.SetActive(true);
                break;
        }
        _openMode =  "default";
        _selectedCard = null;
        _selectedItem?.UpdateBackground();
        _selectedItem = null;
        if (useCard != null) useCard.SetActive(false);
        if (cardPanel != null) cardPanel.SetActive(false);
        cardBagBackground.SetActive(false);
    }

    // 核心刷新：重建/重算并定位
    public void Refresh(bool keepScroll, string type = "", bool isForce = false,  string[] functions = null)
    {
        // if(pool.Count > 0 && !isForce) return;
        var saved = keepScroll ? scrollRect.verticalNormalizedPosition : 1f;
        if (!string.IsNullOrEmpty(type)) _type = type;
        var cards = CardManager.Instance.GetTypeCards(_type, functions);
        if (_selectedItem != null)
        {
            _selectedItem.UpdateBackground();
            _selectedItem = null;
        }
        
        Populate(cards);
        // ResizeContent(cards.Count);

        // 设置布局 - 注释掉单行限制，让Viewport使用原始尺寸显示多行卡牌
        FitViewportToSingleRow(); // 原来的单行限制导致多行卡牌被裁剪
        ResizeContentByCurrentGrid(cards.Count);

        // 水平贴左
        content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);

        StartCoroutine(RestoreAndSyncAfterLayout(keepScroll ? saved : 1f));
    }

    private void Populate(List<Card> cards)
    {
        // 扩容
        for (int i = pool.Count; i < cards.Count; i++)
        {
            var item = Instantiate(itemPrefab, content);
            item.gameObject.SetActive(false);
            pool.Add(item);
        }

        // 先设置所有卡牌的激活状态
        for (int i = 0; i < pool.Count; i++)
        {
            bool active = i < cards.Count;
            var ui = pool[i];
            ui.gameObject.SetActive(active);
            if (active)
            {
                ui.Init(cards[i]);
                // 检查当前卡牌是否是选中的卡牌，如果是则设置选中状态
                if (_selectedCard != null && cards[i] == _selectedCard)
                {
                    _selectedItem = ui;
                    ui.UpdateBackground();
                }
            }
        }

        // 强制更新一次布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // 水平贴左，避免残留偏移
        content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);
    }

    private IEnumerator RestoreAndSyncAfterLayout(float pos)
    {
        // 等待更多帧确保UI完全更新
        yield return null;
        yield return null;

        // 强制更新所有画布
        Canvas.ForceUpdateCanvases();

        // 重建布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        // 重建GridLayoutGroup所在的RectTransform
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<GridLayoutGroup>().transform as RectTransform);
        yield return null;

        // 确保ScrollRect完全停止并设置位置
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(pos);

        // 再次强制更新确保设置生效
        yield return null;
        Canvas.ForceUpdateCanvases();
    }
    
    // 根据当前 grid（包含最新 padding/spacing）重新计算内容高度
    private void ResizeContentByCurrentGrid(int totalCount)
    {
        // 重新计算行数
        int cols = Mathf.Max(1, columns);
        int rows = Mathf.CeilToInt(totalCount / (float)cols);

        float h = grid.padding.top + grid.padding.bottom
                                   + rows * grid.cellSize.y
                                   + Mathf.Max(0, rows - 1) * grid.spacing.y;

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    // 设置视口高度和padding
    private void FitViewportToSingleRow()
    {
        // if (viewport == null) viewport = scrollRect.viewport;

        float rowH = grid.cellSize.y;
        float spacingY = grid.spacing.y;
        float vpH = rowH + 2f * spacingY; // 顶/底留白 = 行间距

        // 设置 Viewport 高度（水平保持铺满父级）
        viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vpH);

        // 设置padding - 确保左右间距在单行和多行时保持一致
        var p = grid.padding;
        p.top = Mathf.RoundToInt(spacingY);
        p.bottom = Mathf.RoundToInt(spacingY);
        // 左右间距始终使用 cellSpacing.x，确保一致性
        p.left = Mathf.RoundToInt(cellSpacing.x);
        p.right = Mathf.RoundToInt(cellSpacing.x);
        grid.padding = p;
    }
}