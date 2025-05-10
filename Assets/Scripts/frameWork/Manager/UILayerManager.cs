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
    // 单例实例
    private static UILayerManager _instance = null;
    public static UILayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("UILayerManager").AddComponent<UILayerManager>();
                _instance.Init();
            }
            return _instance;
        }
    }
    // 内部层级字典，用于快速查找
    private Dictionary<UILayer, Transform> uiLayers = new Dictionary<UILayer, Transform>();

    // Canvas引用
    [SerializeField] private Canvas mainCanvas;

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

        // 初始化并创建UI层级
        CreateUILayers();
    }

    // 创建所有UI层级
    private void CreateUILayers()
    {
        uiLayers.Clear();

        // 遍历所有UI层级枚举并创建对应的GameObject
        foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
        {
            // 创建层级GameObject
            GameObject layerObj = new GameObject(layer.ToString());
            RectTransform rectTransform = layerObj.AddComponent<RectTransform>();

            // 设置为Canvas的子对象
            rectTransform.SetParent(mainCanvas.transform, false);

            // 设置RectTransform铺满父对象
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // 设置层级顺序
            rectTransform.SetSiblingIndex((int)layer);

            // 添加到字典
            uiLayers[layer] = rectTransform;
        }
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