using UnityEngine;
using System.Collections.Generic;

public enum UILayer
{
    ScreenLayer,
    UILayer,
    TopLayer,
    EffectLayer
}

public class UILayerManager : MonoBehaviour
{
    public static UILayerManager Instance = null;
    // 内部层级字典，用于快速查找
    private Dictionary<UILayer, Transform> uiLayers = new Dictionary<UILayer, Transform>();

    // Canvas引用
    [SerializeField] private Canvas mainCanvas;

    // 可拖拽设置的UI层级
    [Header("UI层级设置")]
    [SerializeField] private Transform screenLayer;
    [SerializeField] private Transform uiLayer;
    [SerializeField] private Transform topLayer;
    [SerializeField] private Transform effectLayer;

    private void Awake()
    {
        UILayerManager.Instance = this;
        Init();
    }

    // 初始化方法，由GameManager调用
    public void Init()
    {
        // 如果没有设置Canvas，则尝试获取
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }
        }

        // 确保Canvas存在
        if (mainCanvas == null)
        {
            Debug.LogError("无法找到Canvas，UILayerManager初始化失败");
            return;
        }

        // 初始化UI层级
        InitUILayers();
    }

    // 初始化UI层级
    private void InitUILayers()
    {
        uiLayers.Clear();

        // 添加已经设置的层级到字典
        if (screenLayer != null)
            uiLayers[UILayer.ScreenLayer] = screenLayer;
        else
            Debug.LogWarning("未设置ScreenLayer");

        if (uiLayer != null)
            uiLayers[UILayer.UILayer] = uiLayer;
        else
            Debug.LogWarning("未设置UILayer");

        if (topLayer != null)
            uiLayers[UILayer.TopLayer] = topLayer;
        else
            Debug.LogWarning("未设置TopLayer");

        if (effectLayer != null)
            uiLayers[UILayer.EffectLayer] = effectLayer;
        else
            Debug.LogWarning("未设置EffectLayer");
    }

    // 获取指定层级
    public Transform GetLayer(UILayer layer)
    {
        if (uiLayers.TryGetValue(layer, out Transform layerTransform))
        {
            return layerTransform;
        }

        Debug.LogWarning($"未找到层级 {layer}");
        return null;
    }

    // 获取主Canvas
    public Canvas GetMainCanvas()
    {
        return mainCanvas;
    }
}