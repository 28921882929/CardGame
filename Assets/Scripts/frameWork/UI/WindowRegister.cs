using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frameWork.UI
{
    /// <summary>
    /// 窗口注册类：统一注册窗口信息
    /// </summary>
    public static class WindowRegister
    {
        // 窗口信息字典
        private static Dictionary<string, WindowInfo> _windowInfos = new Dictionary<string, WindowInfo>();

        /// <summary>
        /// 初始化注册表，注册所有窗口
        /// </summary>
        public static void Init()
        {
            // 清空字典
            _windowInfos.Clear();
            
            // 在这里注册所有窗口
            // 注册示例：
            // Register("LoginWindow", "UI/Prefabs/LoginWindow", UILayer.Normal);
            // Register("MainMenuWindow", "UI/Prefabs/MainMenuWindow", UILayer.Normal);
        }

        /// <summary>
        /// 注册窗口
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="layer">UI层级</param>
        public static void Register(string windowName, string prefabPath, UILayer layer = UILayer.Normal)
        {
            if (_windowInfos.ContainsKey(windowName))
            {
                Debug.LogWarning($"窗口 {windowName} 已被注册！");
                return;
            }

            WindowInfo info = new WindowInfo()
            {
                WindowName = windowName,
                PrefabPath = prefabPath,
                Layer = layer
            };

            _windowInfos.Add(windowName, info);
        }
        
        /// <summary>
        /// 注册窗口（扩展版本）
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="layer">UI层级</param>
        /// <param name="sortingOrder">排序顺序</param>
        /// <param name="needMask">是否需要遮罩</param>
        /// <param name="maskAlpha">遮罩透明度</param>
        /// <param name="isClickThrough">是否可以点击穿透</param>
        /// <param name="canBeCovered">是否可以被覆盖</param>
        public static void RegisterExtended(
            string windowName, 
            string prefabPath, 
            UILayer layer = UILayer.Normal,
            int sortingOrder = 0,
            bool needMask = true,
            float maskAlpha = 0.5f,
            bool isClickThrough = false,
            bool canBeCovered = true)
        {
            if (_windowInfos.ContainsKey(windowName))
            {
                Debug.LogWarning($"窗口 {windowName} 已被注册！");
                return;
            }

            WindowInfo info = new WindowInfo()
            {
                WindowName = windowName,
                PrefabPath = prefabPath,
                Layer = layer,
                SortingOrder = sortingOrder,
                NeedMask = needMask,
                MaskAlpha = maskAlpha,
                IsClickThrough = isClickThrough,
                CanBeCovered = canBeCovered
            };

            _windowInfos.Add(windowName, info);
        }

        /// <summary>
        /// 获取窗口信息
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <returns>窗口信息</returns>
        public static WindowInfo GetWindowInfo(string windowName)
        {
            _windowInfos.TryGetValue(windowName, out WindowInfo info);
            return info;
        }

        /// <summary>
        /// 获取已注册的所有窗口信息
        /// </summary>
        /// <returns>窗口信息字典</returns>
        public static Dictionary<string, WindowInfo> GetAllWindowInfos()
        {
            return _windowInfos;
        }
    }
} 