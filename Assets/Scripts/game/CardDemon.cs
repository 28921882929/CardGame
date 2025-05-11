using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.frameWork.Manager;

public class CardDemon : MonoBehaviour
{
    [Header("预制件")]
    [SerializeField] private GameObject cardPrefab; // 卡牌预制件
    [SerializeField] private GameObject slotPrefab; // 插槽预制件

    [Header("父节点")]
    [SerializeField] private Transform cardParent; // 卡牌父节点
    [SerializeField] private Transform slotParent; // 插槽父节点

    [Header("配置")]
    [SerializeField] private int cardCount = 5; // 生成的卡牌数量
    [SerializeField] private int slotCount = 3; // 生成的插槽数量
    [SerializeField] private float cardFanRadius = 300f; // 卡牌扇形半径
    [SerializeField] private float cardFanAngle = 30f; // 卡牌扇形角度
    [SerializeField] private float cardSpacing = 50f; // 卡牌间距
    [SerializeField] private float slotSpacing = 100f; // 插槽间距

    private List<GameObject> cards = new List<GameObject>();
    private List<GameObject> slots = new List<GameObject>();

    // 对象池名称常量
    private const string CARD_POOL_NAME = "CardPool";
    private const string SLOT_POOL_NAME = "SlotPool";

    private void Awake()
    {
        // 确保在场景中只有一个实例
        CardDemon[] demons = FindObjectsOfType<CardDemon>();
        if (demons.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        Debug.Log("添加监听");
        EventManager.Instance.AddListener("DemoTest", OnGameStart);
    }

    /// <summary>
    /// 游戏开始事件处理
    /// </summary>
    private void OnGameStart()
    {
        Debug.Log("游戏开始");
        // 检查父节点是否已指定，否则使用当前对象作为父节点
        if (cardParent == null) cardParent = transform;
        if (slotParent == null) slotParent = transform;

        // 初始化对象池
        InitObjectPools();

        // 创建卡牌和插槽
        CreateCards();
        CreateSlots();
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitObjectPools()
    {
        // 初始化卡牌对象池
        if (cardPrefab != null)
        {
            ObjectPoolManager.Instance.CreatePool(CARD_POOL_NAME, cardPrefab, cardCount, transform);
        }
        else
        {
            Debug.LogError("卡牌预制件未指定！");
        }

        // 初始化插槽对象池
        if (slotPrefab != null)
        {
            ObjectPoolManager.Instance.CreatePool(SLOT_POOL_NAME, slotPrefab, slotCount, transform);
        }
        else
        {
            Debug.LogError("插槽预制件未指定！");
        }
    }

    /// <summary>
    /// 创建卡牌
    /// </summary>
    private void CreateCards()
    {
        // 清空现有卡牌
        foreach (var card in cards)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(CARD_POOL_NAME, card);
        }
        cards.Clear();

        // 根据卡牌数量和间距计算扇形角度
        float totalWidth = (cardCount - 1) * cardSpacing;
        float calculatedAngle = cardFanAngle;

        // 如果卡牌间距参数有效，则根据间距计算角度
        if (cardSpacing > 0)
        {
            // 根据弧长公式计算角度：弧长 = 半径 * 角度(弧度)
            // 转换为角度：角度 = (弧长 / 半径) * (180 / PI)
            calculatedAngle = (totalWidth / cardFanRadius) * Mathf.Rad2Deg;
        }

        // 计算卡牌之间的角度间隔
        float angleStep = calculatedAngle / (cardCount - 1);
        float startAngle = -calculatedAngle / 2f;

        // 临时存储卡牌及其X坐标的列表，用于排序
        List<KeyValuePair<GameObject, float>> cardPositions = new List<KeyValuePair<GameObject, float>>();

        // 创建卡牌并按扇形排列
        for (int i = 0; i < cardCount; i++)
        {
            // 从对象池获取卡牌
            GameObject card = ObjectPoolManager.Instance.GetObjectFromPool(CARD_POOL_NAME);
            if (card != null)
            {
                // 设置卡牌父节点
                card.transform.SetParent(cardParent, false);

                // 计算卡牌位置（扇形排列）
                float angle = startAngle + angleStep * i;
                float radians = angle * Mathf.Deg2Rad;
                float x = Mathf.Sin(radians) * cardFanRadius;
                float y = Mathf.Cos(radians) * cardFanRadius;

                // 设置卡牌本地位置和旋转
                card.transform.localPosition = new Vector3(x, y, 0);
                card.transform.localRotation = Quaternion.Euler(0, 0, -angle);

                // 存储卡牌及其X坐标
                cardPositions.Add(new KeyValuePair<GameObject, float>(card, x));
            }
        }

        // 根据X坐标从左到右排序卡牌
        cardPositions.Sort((a, b) => a.Value.CompareTo(b.Value));

        // 清空当前列表并按排序后的顺序重新添加卡牌
        cards.Clear();

        // 设置排序顺序并添加到列表中（从左到右，最右边的在最上层）
        for (int i = 0; i < cardPositions.Count; i++)
        {
            GameObject card = cardPositions[i].Key;

            // 设置卡牌的排序顺序（左边低层级，右边高层级）
            Canvas cardCanvas = card.GetComponent<Canvas>();
            if (cardCanvas != null)
            {
                cardCanvas.sortingOrder = i;
            }

            // 初始化卡牌上的IUIHandler接口
            InitUIHandlers(card);

            // 将卡牌添加到列表中
            cards.Add(card);
        }
    }

    /// <summary>
    /// 创建插槽
    /// </summary>
    private void CreateSlots()
    {
        // 清空现有插槽
        foreach (var slot in slots)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(SLOT_POOL_NAME, slot);
        }
        slots.Clear();

        // 计算插槽的起始X坐标（居中）
        float startX = (slotCount - 1) * slotSpacing / 2f * -1f;

        // 创建插槽
        for (int i = 0; i < slotCount; i++)
        {
            // 从对象池获取插槽
            GameObject slot = ObjectPoolManager.Instance.GetObjectFromPool(SLOT_POOL_NAME);
            if (slot != null)
            {
                // 设置插槽父节点
                slot.transform.SetParent(slotParent, false);

                // 设置插槽本地位置
                slot.transform.localPosition = new Vector3(startX + i * slotSpacing, 0, 0);

                // 将插槽添加到列表中
                slots.Add(slot);

                // 初始化插槽上的IUIHandler接口
                InitUIHandlers(slot);
            }
        }
    }

    /// <summary>
    /// 初始化对象上的所有IUIHandler接口
    /// </summary>
    /// <param name="obj">要初始化的游戏对象</param>
    private void InitUIHandlers(GameObject obj)
    {
        // 获取对象上的所有IUIHandler组件
        IUIHandler[] handlers = obj.GetComponents<IUIHandler>();

        // 为每个组件调用InitHandler方法
        foreach (var handler in handlers)
        {
            handler.InitHandler();
        }
    }

    private void OnDestroy()
    {
        // 清理事件监听
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener("GameStart", OnGameStart);
        }
    }
}