using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.frameWork.Manager;
using TMPro;

public class EventTestReceiver : MonoBehaviour
{
    [Header("文本显示")]
    public TextMeshProUGUI receiverStatusText;
    
    // 使用与EventTestDemo相同的事件名
    private const string EVENT_NO_PARAM = "TestEventNoParam";
    private const string EVENT_WITH_PARAM = "TestEventWithParam";
    
    void Start()
    {
        // 初始化
        if (receiverStatusText != null)
        {
            receiverStatusText.text = "等待接收事件...";
        }
        
        // 注册相同的事件监听，但是有不同的处理函数
        EventManager.Instance.AddListener(EVENT_NO_PARAM, OnNoParamEventReceived);
        EventManager.Instance.AddListener<string>(EVENT_WITH_PARAM, OnParamEventReceived);
        
        Debug.Log("事件接收组件已初始化，可以接收事件");
    }
    
    // 无参数事件接收处理
    private void OnNoParamEventReceived()
    {
        Debug.Log("接收器接收到无参数事件");
        if (receiverStatusText != null)
        {
            receiverStatusText.text = "接收器：收到无参数事件 - " + System.DateTime.Now.ToString("HH:mm:ss");
        }
    }
    
    // 带参数事件接收处理
    private void OnParamEventReceived(string message)
    {
        Debug.Log($"接收器接收到带参数事件: {message}");
        if (receiverStatusText != null)
        {
            receiverStatusText.text = $"接收器：{message}";
        }
    }
    
    void OnDestroy()
    {
        // 移除事件监听
        EventManager.Instance.RemoveListener(EVENT_NO_PARAM, OnNoParamEventReceived);
        EventManager.Instance.RemoveListener<string>(EVENT_WITH_PARAM, OnParamEventReceived);
    }
} 