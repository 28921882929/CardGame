using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frameWork.UI.Example
{
    /// <summary>
    /// UI框架使用示例
    /// </summary>
    public class UIFrameworkExample : MonoBehaviour
    {
        void Start()
        {
            // 初始化UI框架
            UIFramework.Init();
            
            // 注册示例窗口（使用扩展方法，设置遮罩透明度和排序顺序）
            RegisterExampleWindows();
        }

        /// <summary>
        /// 注册示例窗口
        /// </summary>
        private void RegisterExampleWindows()
        {
            // 使用扩展注册方法，可以设置更多参数
            WindowRegister.RegisterExtended(
                "ExampleWindow", 
                "UI/Prefabs/ExampleWindow", 
                UILayer.Normal,
                sortingOrder: 10,
                needMask: true,
                maskAlpha: 0.7f
            );
            
            // 注册更多示例窗口
            WindowRegister.RegisterExtended(
                "Window1", 
                "UI/Prefabs/ExampleWindow", 
                UILayer.Normal,
                sortingOrder: 0,
                needMask: true,
                maskAlpha: 0.5f
            );
            
            WindowRegister.RegisterExtended(
                "Window2", 
                "UI/Prefabs/ExampleWindow", 
                UILayer.Normal,
                sortingOrder: 0,
                needMask: true,
                maskAlpha: 0.5f
            );
            
            WindowRegister.RegisterExtended(
                "Window3", 
                "UI/Prefabs/ExampleWindow", 
                UILayer.Normal,
                sortingOrder: 0,
                needMask: true,
                maskAlpha: 0.5f
            );
            
            // 注册一个弹窗层的窗口
            WindowRegister.RegisterExtended(
                "PopupWindow", 
                "UI/Prefabs/ExampleWindow", 
                UILayer.PopUp,
                sortingOrder: 0,
                needMask: true,
                maskAlpha: 0.5f
            );
        }

        /// <summary>
        /// 打开示例窗口（简单方式）
        /// </summary>
        public void OpenSimpleWindow()
        {
            UIFramework.OpenWindow("ExampleWindow", "这是一个示例窗口");
        }
        
        /// <summary>
        /// 打开示例窗口（使用数据注入）
        /// </summary>
        public void OpenWindowWithData()
        {
            // 创建窗口数据
            ExampleWindowData data = new ExampleWindowData
            {
                Title = "带数据的窗口",
                Content = "这是通过数据注入显示的内容。\n点击确认或取消按钮将触发回调。",
                ShowConfirmButton = true,
                ShowCancelButton = true,
                OnConfirm = () => { Debug.Log("点击了确认按钮"); },
                OnCancel = () => { Debug.Log("点击了取消按钮"); }
            };
            
            // 打开窗口并注入数据
            UIFramework.OpenWindow("ExampleWindow", data);
        }
        
        /// <summary>
        /// 打开弹窗窗口
        /// </summary>
        public void OpenPopupWindow()
        {
            // 创建窗口数据
            ExampleWindowData data = new ExampleWindowData
            {
                Title = "弹窗层窗口",
                Content = "这是一个弹窗层的窗口，会显示在普通窗口之上。",
                ShowConfirmButton = true,
                ShowCancelButton = false,
                OnConfirm = () => { Debug.Log("关闭弹窗"); }
            };
            
            // 打开弹窗层窗口
            UIFramework.OpenWindow("PopupWindow", data);
        }
        
        /// <summary>
        /// 演示窗口栈功能 - 打开多个窗口形成栈
        /// </summary>
        public void DemoWindowStack()
        {
            // 依次打开3个窗口，每个窗口会将前一个窗口入栈
            OpenWindow("Window1", "窗口1 - 第一个打开的窗口");
            OpenWindow("Window2", "窗口2 - 第二个打开的窗口");
            OpenWindow("Window3", "窗口3 - 第三个打开的窗口");
        }
        
        /// <summary>
        /// 演示返回到上一个窗口
        /// </summary>
        public void DemoGoBack()
        {
            UIFramework.GoBack(UILayer.Normal);
        }
        
        /// <summary>
        /// 辅助方法：打开窗口并等待一段时间
        /// </summary>
        private void OpenWindow(string windowName, string title)
        {
            UIFramework.OpenWindow(windowName, title);
        }

        /// <summary>
        /// 关闭示例窗口
        /// </summary>
        public void CloseExampleWindow()
        {
            UIFramework.CloseWindow("ExampleWindow");
        }

        /// <summary>
        /// 关闭所有窗口
        /// </summary>
        public void CloseAllWindows()
        {
            UIFramework.CloseAllWindows();
        }
        
        /// <summary>
        /// 将窗口置于最前面
        /// </summary>
        public void BringWindowToFront()
        {
            WindowManager.Instance.BringWindowToFront("ExampleWindow");
        }
        
        /// <summary>
        /// 显示窗口栈状态
        /// </summary>
        public void ShowStackStatus()
        {
            int stackCount = UIFramework.GetStackCount(UILayer.Normal);
            BaseWindow currentWindow = UIFramework.GetCurrentWindow(UILayer.Normal);
            string currentWindowName = currentWindow != null ? currentWindow.WindowName : "无";
            
            Debug.Log($"当前显示窗口: {currentWindowName}, 栈中窗口数量: {stackCount}");
        }
    }
} 