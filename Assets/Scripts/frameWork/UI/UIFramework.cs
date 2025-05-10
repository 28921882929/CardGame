using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frameWork.UI
{
    /// <summary>
    /// UI框架初始化类
    /// </summary>
    public static class UIFramework
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化UI框架
        /// </summary>
        public static void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            Debug.Log("初始化UI框架...");

            // 初始化窗口注册表
            WindowRegister.Init();

            // 初始化窗口管理器
            WindowManager.Instance.ToString(); // 触发单例创建

            Debug.Log("UI框架初始化完成");
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <param name="args">传递参数</param>
        /// <returns>窗口实例</returns>
        public static BaseWindow OpenWindow(string windowName, object args = null)
        {
            return WindowManager.Instance.OpenWindow(windowName, args);
        }
        
        /// <summary>
        /// 打开窗口（使用窗口数据）
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <param name="windowData">窗口数据</param>
        /// <returns>窗口实例</returns>
        public static BaseWindow OpenWindow(string windowName, IWindowData windowData)
        {
            return WindowManager.Instance.OpenWindow(windowName, windowData);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        public static void CloseWindow(string windowName)
        {
            WindowManager.Instance.CloseWindow(windowName);
        }

        /// <summary>
        /// 关闭所有窗口
        /// </summary>
        public static void CloseAllWindows()
        {
            WindowManager.Instance.CloseAllWindows();
        }

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <returns>窗口实例</returns>
        public static BaseWindow GetWindow(string windowName)
        {
            return WindowManager.Instance.GetWindow(windowName);
        }
        
        /// <summary>
        /// 将窗口移到最前面
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        public static void BringWindowToFront(string windowName)
        {
            WindowManager.Instance.BringWindowToFront(windowName);
        }
        
        /// <summary>
        /// 返回上一个窗口（关闭当前层级窗口，显示栈中的上一个窗口）
        /// </summary>
        /// <param name="layer">UI层级</param>
        /// <returns>是否成功返回</returns>
        public static bool GoBack(UILayer layer)
        {
            return WindowManager.Instance.GoToPreviousWindow(layer);
        }
        
        /// <summary>
        /// 获取当前层级显示的窗口
        /// </summary>
        /// <param name="layer">UI层级</param>
        /// <returns>当前窗口</returns>
        public static BaseWindow GetCurrentWindow(UILayer layer)
        {
            return WindowManager.Instance.GetCurrentLayerWindow(layer);
        }
        
        /// <summary>
        /// 获取窗口栈中的窗口数量
        /// </summary>
        /// <param name="layer">UI层级</param>
        /// <returns>栈中窗口数量</returns>
        public static int GetStackCount(UILayer layer)
        {
            return WindowManager.Instance.GetWindowStackCount(layer);
        }
    }
} 