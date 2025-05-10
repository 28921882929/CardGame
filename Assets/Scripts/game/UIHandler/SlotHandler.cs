using UnityEngine;
using UnityEngine.EventSystems;

public class SlotHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IUIHandler
{
    public bool IsOccupied { get; private set; } = false;
    public GameObject CurrentItem { get; private set; } = null;

    // 拖拽层级引用
    private Transform dragLayer;

    public void InitHandler()
    {
        Transform dragLayerTransform = UILayerManager.Instance.GetLayer(UILayer.TopLayer);
        dragLayer = dragLayerTransform;
    }

    // 当指针进入插槽时调用
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragHandler dragHandler = eventData.pointerDrag.GetComponent<DragHandler>();
            if (dragHandler != null)
            {
                // 通知拖拽处理器指针已进入插槽
                dragHandler.SetHoveringSlot(this);

                // 当指针进入插槽时就自动放入物品
                if (!IsOccupied)
                {
                    // 记录物品
                    IsOccupied = true;
                    CurrentItem = eventData.pointerDrag;

                    // 将拖拽物品设为此插槽的子对象
                    CurrentItem.transform.SetParent(transform);

                    // 重置物品在插槽中的位置
                    RectTransform itemRect = CurrentItem.GetComponent<RectTransform>();
                    if (itemRect != null)
                    {
                        itemRect.localPosition = Vector3.zero;
                        itemRect.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }
    }

    // 当指针离开插槽时调用
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragHandler dragHandler = eventData.pointerDrag.GetComponent<DragHandler>();
            if (dragHandler != null)
            {
                // 通知拖拽处理器指针已离开插槽
                dragHandler.ClearHoveringSlot(this);

                // 如果当前物品就是正在拖拽的物品，则将其从插槽中移出
                if (CurrentItem == eventData.pointerDrag)
                {
                    // 将物品移到拖拽层级
                    if (dragLayer != null)
                    {
                        CurrentItem.transform.SetParent(dragLayer);
                    }
                    else
                    {
                        // 如果没有设置拖拽层级，则尝试找到Canvas放置
                        Canvas canvas = FindObjectOfType<Canvas>();
                        if (canvas != null)
                        {
                            CurrentItem.transform.SetParent(canvas.transform);
                        }
                    }

                    // 将物品移动到鼠标位置
                    RectTransform itemRect = CurrentItem.GetComponent<RectTransform>();
                    if (itemRect != null)
                    {
                        Vector2 mousePosition;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            itemRect.parent.GetComponent<RectTransform>(),
                            eventData.position,
                            eventData.pressEventCamera,
                            out mousePosition
                        );
                        itemRect.localPosition = mousePosition;
                    }

                    // 清除插槽的占用状态
                    IsOccupied = false;
                    CurrentItem = null;
                }
            }
        }
    }

    // 从插槽中移除物品
    public void RemoveItem()
    {
        IsOccupied = false;
        CurrentItem = null;
    }

    // 获取插槽的位置
    public Vector2 GetPosition()
    {
        RectTransform slotRectTransform = GetComponent<RectTransform>();
        return slotRectTransform.anchoredPosition;
    }
}