using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IUIHandler
{
    // 视觉效果参数
    [SerializeField] private float hoverHeight = 30f;     // 悬停时上浮高度
    [SerializeField] private float hoverScale = 1.2f;     // 悬停时放大比例
    [SerializeField] private float animationDuration = 0.2f; // 动画持续时间
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f); // 悬停颜色
    private Transform topLayer;

    // 悬停功能开关
    [SerializeField] private bool hoverEnabled = true;    // 是否启用悬停效果

    // 保存原始状态
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Color originalColor;
    private Transform originalParent;              // 原始父级对象
    private int originalSiblingIndex;              // 原始兄弟索引

    // 组件引用
    private Graphic graphic;

    // 协程控制
    private Coroutine animationCoroutine;

    // 卡牌当前状态
    private enum CardState { Normal, HoverIn, HoverOut }
    private CardState currentState = CardState.Normal;

    private void Awake()
    {
        // 获取组件
        graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            originalColor = graphic.color;
        }
    }

    public void InitHandler()
    {
        Debug.Log("InitHandler");
        topLayer = UILayerManager.Instance.GetLayer(UILayer.TopLayer);
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    /// <summary>
    /// 设置悬停功能的开关状态
    /// </summary>
    /// <param name="enabled">是否启用悬停效果</param>
    public void SetHoverEnabled(bool enabled)
    {
        hoverEnabled = enabled;
    }

    /// <summary>
    /// 设置卡牌的初始位置
    /// </summary>
    /// <param name="position">新的初始位置</param>
    /// <param name="siblingIndex">新的兄弟索引</param>
    public void SetInitialPosition(Vector3 position, int siblingIndex)
    {
        // 如果卡牌当前不在悬停或恢复动画中，直接设置位置
        if (currentState == CardState.Normal)
        {
            transform.position = position;
        }

        // 更新原始位置
        originalPosition = position;
        originalSiblingIndex = siblingIndex;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 检查悬停功能是否启用
        if (!hoverEnabled) return;

        // 只检查自己的状态，不再检查全局状态
        // 如果当前不在HoverOut状态，可以开始悬停
        if (currentState != CardState.HoverOut)
        {
            StartHoverAnimation();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 检查悬停功能是否启用
        if (!hoverEnabled) return;

        // 开始恢复动画
        StartResetAnimation();
    }

    private void StartHoverAnimation()
    {
        // 取消当前正在运行的任何动画
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // 开始新的悬停动画
        animationCoroutine = StartCoroutine(HoverAnimation());
    }

    private void StartResetAnimation()
    {
        // 只有当不在恢复动画中才开始恢复
        if (currentState != CardState.HoverOut)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(ResetAnimation());
        }
    }

    private IEnumerator HoverAnimation()
    {
        // 设置当前状态
        currentState = CardState.HoverIn;
        // 移动到顶层容器
        if (topLayer != null)
        {
            transform.SetParent(topLayer, true); // 保持世界坐标不变
        }
        else
        {
            Debug.LogWarning("无法从UILayerManager获取顶层容器，将使用SetAsLastSibling作为备选方案");
            transform.SetAsLastSibling();
        }

        // 目标值
        Vector3 targetPosition = originalPosition + new Vector3(0, hoverHeight, 0);
        Vector3 targetScale = originalScale * hoverScale;

        // 动画过渡
        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Color startColor = graphic != null ? graphic.color : Color.white;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (graphic != null)
            {
                graphic.color = Color.Lerp(startColor, hoverColor, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终状态
        transform.position = targetPosition;
        transform.localScale = targetScale;

        if (graphic != null)
        {
            graphic.color = hoverColor;
        }

        currentState = CardState.Normal;
        animationCoroutine = null;
    }

    private IEnumerator ResetAnimation()
    {
        currentState = CardState.HoverOut;

        // 当前状态
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Color startColor = graphic != null ? graphic.color : Color.white;

        // 动画过渡
        float elapsedTime = 0;
        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            transform.position = Vector3.Lerp(startPosition, originalPosition, t);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);

            if (graphic != null)
            {
                graphic.color = Color.Lerp(startColor, originalColor, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保恢复到原始状态
        transform.position = originalPosition;
        transform.localScale = originalScale;

        if (graphic != null)
        {
            graphic.color = originalColor;
        }

        // 恢复原始父级和索引
        transform.SetParent(originalParent, true);
        Debug.Log("set index: " + originalSiblingIndex + " to " + transform.name);
        transform.SetSiblingIndex(originalSiblingIndex);

        currentState = CardState.Normal;
        animationCoroutine = null;
    }
}
