using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.frameWork.Manager;

public class CardDemon : MonoBehaviour
{
    [Header("预制件")]
    [SerializeField] private GameObject cardPrefab; // 卡牌预制件
    [SerializeField] private GameObject slotPrefab; // 插槽预制件

    [Header("配置")]
    [SerializeField] private int cardCount = 10; // 生成的卡牌数量
    [SerializeField] private int slotCount = 3; // 生成的插槽数量
    [SerializeField] private float cardFanRadius = 300f; // 卡牌扇形半径
    [SerializeField] private float cardFanAngle = 30f; // 卡牌扇形角度
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

        // 根据屏幕尺寸计算卡牌起始位置
        Vector3 cardStartPosition = new Vector3(Screen.width / 2, Screen.height * 0.2f, 0);

        // 计算卡牌之间的角度间隔
        float angleStep = cardFanAngle / (cardCount - 1);
        float startAngle = -cardFanAngle / 2f;

        // 创建卡牌并按扇形排列
        for (int i = 0; i < cardCount; i++)
        {
            // 从对象池获取卡牌
            GameObject card = ObjectPoolManager.Instance.GetObjectFromPool(CARD_POOL_NAME);
            if (card != null)
            {
                // 计算卡牌位置（扇形排列）
                float angle = startAngle + angleStep * i;
                float radians = angle * Mathf.Deg2Rad;
                float x = Mathf.Sin(radians) * cardFanRadius;
                float y = Mathf.Cos(radians) * cardFanRadius;

                // 设置卡牌位置和旋转
                card.transform.position = cardStartPosition + new Vector3(x, y, 0);
                card.transform.rotation = Quaternion.Euler(0, 0, -angle);

                // 设置卡牌的排序顺序（最后一张在最上面）
                Canvas cardCanvas = card.GetComponent<Canvas>();
                if (cardCanvas != null)
                {
                    cardCanvas.sortingOrder = i;
                }

                // 将卡牌添加到列表中
                cards.Add(card);

                // 初始化卡牌上的IUIHandler接口
                InitUIHandlers(card);
            }
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

        // 根据屏幕尺寸计算插槽起始位置
        Vector3 slotStartPosition = new Vector3(Screen.width / 2, Screen.height * 0.8f, 0);
        
        // 计算第一个插槽的位置（居中）
        float startX = slotStartPosition.x + (slotCount - 1) * slotSpacing / 2f * -1f;

        // 创建插槽
        for (int i = 0; i < slotCount; i++)
        {
            // 从对象池获取插槽
            GameObject slot = ObjectPoolManager.Instance.GetObjectFromPool(SLOT_POOL_NAME);
            if (slot != null)
            {
                // 设置插槽位置
                slot.transform.position = new Vector3(startX + i * slotSpacing, slotStartPosition.y, slotStartPosition.z);

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