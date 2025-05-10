using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.frameWork.Manager;
using TMPro;

public class EventTestDemo : MonoBehaviour
{
    [Header("按钮引用")]
    public Button button1;
    public Button button2;
    
    [Header("文本显示")]
    public TextMeshProUGUI statusText;
    
    // 事件名称常量
    private const string EVENT_NO_PARAM = "TestEventNoParam";
    private const string EVENT_WITH_PARAM = "TestEventWithParam";
    
    void Start()
    {
        // 初始化
        if (statusText != null)
        {
            statusText.text = "等待测试...";
        }
        
        // 注册事件监听
        EventManager.Instance.AddListener(EVENT_NO_PARAM, OnEventNoParam);
        EventManager.Instance.AddListener<string>(EVENT_WITH_PARAM, OnEventWithParam);
        
        // 设置按钮监听
        if (button1 != null)
        {
            button1.onClick.AddListener(OnButton1Click);
        }
        
        if (button2 != null)
        {
            button2.onClick.AddListener(OnButton2Click);
        }
        
        Debug.Log("事件测试组件已初始化，请点击按钮测试事件系统");
    }
    
    // 按钮1点击 - 测试无参数事件
    private void OnButton1Click()
    {
        Debug.Log("触发无参数事件");
        EventManager.Instance.TriggerEvent(EVENT_NO_PARAM);
    }
    
    // 按钮2点击 - 测试带参数事件
    private void OnButton2Click()
    {
        string message = "这是一个参数消息 - " + System.DateTime.Now.ToString("HH:mm:ss");
        Debug.Log($"触发带参数事件: {message}");
        EventManager.Instance.TriggerEvent(EVENT_WITH_PARAM, message);
    }
    
    // 无参数事件处理
    private void OnEventNoParam()
    {
        Debug.Log("接收到无参数事件");
        if (statusText != null)
        {
            statusText.text = "接收到无参数事件 - " + System.DateTime.Now.ToString("HH:mm:ss");
        }
    }
    
    // 带参数事件处理
    private void OnEventWithParam(string message)
    {
        Debug.Log($"接收到带参数事件: {message}");
        if (statusText != null)
        {
            statusText.text = $"接收到消息: {message}";
        }
    }
    
    void OnDestroy()
    {
        // 移除事件监听
        EventManager.Instance.RemoveListener(EVENT_NO_PARAM, OnEventNoParam);
        EventManager.Instance.RemoveListener<string>(EVENT_WITH_PARAM, OnEventWithParam);
        
        // 移除按钮监听
        if (button1 != null)
        {
            button1.onClick.RemoveListener(OnButton1Click);
        }
        
        if (button2 != null)
        {
            button2.onClick.RemoveListener(OnButton2Click);
        }
    }
} 