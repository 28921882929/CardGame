using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frameWork.UI
{
    /// <summary>
    /// UI层级
    /// </summary>
    public enum UILayer
    {
        Background = 0,  // 背景层
        Normal = 1,      // 普通层
        Top = 2,         // 顶层
        PopUp = 3,       // 弹窗层
        Guide = 4,       // 引导层
        Toast = 5        // 提示层
    }

    /// <summary>
    /// 窗口信息类
    /// </summary>
    public class WindowInfo
    {
        // 窗口名称
        public string WindowName { get; set; }

        // 预制体路径
        public string PrefabPath { get; set; }

        // UI层级
        public UILayer Layer { get; set; } = UILayer.Normal;

        // 是否可以点击穿透
        public bool IsClickThrough { get; set; } = false;

        // 是否可被其他窗口覆盖
        public bool CanBeCovered { get; set; } = true;

        // 是否自动注册
        public bool AutoRegister { get; set; } = true;
        
        // 排序权重（同层级内，权重越高显示越靠前）
        public int SortingOrder { get; set; } = 0;
        
        // 是否需要背景遮罩
        public bool NeedMask { get; set; } = true;
        
        // 遮罩透明度 (0-1)
        public float MaskAlpha { get; set; } = 0.5f;
    }
} 