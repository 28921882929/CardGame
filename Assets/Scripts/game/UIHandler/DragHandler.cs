using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IUIHandler
{
    private Vector2 startPosition;
    private Vector3 startScale;
    private int startSiblingIndex;
    private SlotHandler currentSlot = null;
    private SlotHandler hoveringSlot = null;
    private CanvasGroup canvasGroup = null;
    private HoverHandler hoverHandler = null;
    private Transform originalParent = null;
    private Transform dragLayer;

    // 平滑返回的参数
    [SerializeField] private float returnDuration = 0.3f;
    private bool isReturning = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        hoverHandler = GetComponent<HoverHandler>();
    }

    public void InitHandler()
    {
        Debug.Log("InitHandler");
        Transform dragLayerTransform = UILayerManager.Instance.GetLayer(UILayer.TopLayer);
        dragLayer = dragLayerTransform;
        startPosition = transform.position;
        startScale = transform.localScale;
        originalParent = transform.parent;
        startSiblingIndex = transform.GetSiblingIndex();
    }

    // 设置初始位置和缩放
    public void SetInitialPositionAndScale(Vector2 position, Vector3 scale, int siblingIndex, Transform parent)
    {
        startPosition = position;
        startScale = scale;
        startSiblingIndex = siblingIndex;
        originalParent = parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag: " + transform.name);
        // 如果开始拖拽时已在插槽中，则从插槽移除
        if (currentSlot != null)
        {
            currentSlot.RemoveItem();
            currentSlot = null;
        }

        // 如果正在返回动画中，取消返回
        if (isReturning)
        {
            StopAllCoroutines();
            isReturning = false;
        }

        // 将物品移动到拖拽层级
        if (dragLayer != null)
        {
            transform.SetParent(dragLayer);
            // 将物品置于顶层显示
            transform.SetAsLastSibling();
        }

        // 禁用CanvasGroup的交互
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        // 禁用悬停效果
        if (hoverHandler != null)
        {
            hoverHandler.SetHoverEnabled(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        // 只有当不在悬停插槽上时，才跟随鼠标移动
        if (hoveringSlot == null && dragLayer != null)
        {
            // 将鼠标位置转换为本地坐标
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragLayer.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );
            transform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 如果结束拖拽时没有悬停在任何插槽上，则平滑返回原位置
        if (hoveringSlot == null)
        {
            StartCoroutine(SmoothReturnToOrigin());
        }

        hoveringSlot = null;

        // 启用CanvasGroup的交互
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        // 启用悬停效果
        if (hoverHandler != null)
        {
            hoverHandler.SetHoverEnabled(true);
        }
    }

    // 平滑返回原始位置和缩放的协程
    private IEnumerator SmoothReturnToOrigin()
    {
        isReturning = true;

        // 当前状态
        Vector2 currentPosition = transform.position;
        Vector3 currentScale = transform.localScale;
        Transform currentParent = transform.parent;

        // 将物品移动到拖拽层级以防止返回过程中的视觉干扰
        if (dragLayer != null && currentParent != dragLayer)
        {
            transform.SetParent(dragLayer);
        }

        float elapsedTime = 0;

        while (elapsedTime < returnDuration)
        {
            float t = elapsedTime / returnDuration;
            t = Mathf.SmoothStep(0, 1, t); // 使用平滑插值

            // 更新位置和缩放
            transform.position = Vector2.Lerp(currentPosition, startPosition, t);
            transform.localScale = Vector3.Lerp(currentScale, startScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终值精确
        transform.position = startPosition;
        transform.localScale = startScale;

        // 返回到原始父节点和同级索引
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(startSiblingIndex);
        }

        isReturning = false;
    }

    // 设置当前悬停的插槽
    public void SetHoveringSlot(SlotHandler slot)
    {
        hoveringSlot = slot;
    }

    // 清除当前悬停的插槽（如果是同一个）
    public void ClearHoveringSlot(SlotHandler slot)
    {
        if (hoveringSlot == slot)
        {
            hoveringSlot = null;
        }
    }

    // 将物品放入插槽
    public void PlaceInSlot(SlotHandler slot)
    {
        // 如果该插槽已被占用，则不放入
        if (slot.IsOccupied && slot.CurrentItem != gameObject)
        {
            // 如果原位置有效，则平滑返回原位置
            if (originalParent != null)
            {
                StartCoroutine(SmoothReturnToOrigin());
            }
            return;
        }

        // 更新当前插槽引用
        currentSlot = slot;

        // 注意：不再设置anchoredPosition，因为SlotHandler会处理
    }

}
