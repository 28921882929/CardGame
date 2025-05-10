using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frameWork.UI.Example
{
    /// <summary>
    /// 示例窗口数据
    /// </summary>
    public class ExampleWindowData : IWindowData
    {
        // 标题
        public string Title { get; set; }
        
        // 内容
        public string Content { get; set; }
        
        // 显示确认按钮
        public bool ShowConfirmButton { get; set; } = true;
        
        // 显示取消按钮
        public bool ShowCancelButton { get; set; } = true;
        
        // 确认按钮回调
        public System.Action OnConfirm { get; set; }
        
        // 取消按钮回调
        public System.Action OnCancel { get; set; }
    }
} 