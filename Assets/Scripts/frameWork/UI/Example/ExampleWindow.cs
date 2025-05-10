using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace frameWork.UI.Example
{
    /// <summary>
    /// 示例窗口
    /// </summary>
    public class ExampleWindow : BaseWindow
    {
        // UI组件引用
        private Button _closeButton;
        private Text _titleText;
        private Text _contentText;
        private Button _confirmButton;
        private Button _cancelButton;
        
        // 窗口数据
        private ExampleWindowData _data;

        /// <summary>
        /// 窗口初始化
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

            // 获取UI组件引用
            _closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
            _titleText = transform.Find("TitleText")?.GetComponent<Text>();
            _contentText = transform.Find("ContentText")?.GetComponent<Text>();
            _confirmButton = transform.Find("ConfirmButton")?.GetComponent<Button>();
            _cancelButton = transform.Find("CancelButton")?.GetComponent<Button>();

            // 注册按钮事件
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseButtonClick);
            }
            
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmButtonClick);
            }
            
            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelButtonClick);
            }
        }

        /// <summary>
        /// 窗口打开
        /// </summary>
        /// <param name="args">传递参数</param>
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);

            // 处理传入参数，支持字符串和ExampleWindowData
            if (args is string title && _titleText != null)
            {
                _titleText.text = title;
                
                // 如果只传入标题，隐藏其他内容
                if (_contentText != null) _contentText.text = "";
                if (_confirmButton != null) _confirmButton.gameObject.SetActive(false);
                if (_cancelButton != null) _cancelButton.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 注入窗口数据
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        public override void InjectData(IWindowData windowData)
        {
            base.InjectData(windowData);
            
            if (windowData is ExampleWindowData data)
            {
                _data = data;
                UpdateUI();
            }
        }
        
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            if (_data == null) return;
            
            // 设置标题
            if (_titleText != null)
            {
                _titleText.text = _data.Title;
            }
            
            // 设置内容
            if (_contentText != null)
            {
                _contentText.text = _data.Content;
            }
            
            // 设置按钮显示状态
            if (_confirmButton != null)
            {
                _confirmButton.gameObject.SetActive(_data.ShowConfirmButton);
            }
            
            if (_cancelButton != null)
            {
                _cancelButton.gameObject.SetActive(_data.ShowCancelButton);
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();

            // 清空数据
            _data = null;
        }

        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void OnCloseButtonClick()
        {
            Close();
        }
        
        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void OnConfirmButtonClick()
        {
            // 调用确认回调
            _data?.OnConfirm?.Invoke();
            Close();
        }
        
        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void OnCancelButtonClick()
        {
            // 调用取消回调
            _data?.OnCancel?.Invoke();
            Close();
        }
    }
} 