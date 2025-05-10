using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frameWork.UI
{
    /// <summary>
    /// 窗口基类
    /// </summary>
    public abstract class BaseWindow : MonoBehaviour
    {
        // 窗口名称
        public string WindowName { get; set; }

        // 窗口是否已经初始化
        protected bool _isInitialized = false;
        
        // 窗口数据
        protected IWindowData _windowData;
        
        // 窗口信息
        private WindowInfo _windowInfo;
        public WindowInfo WindowInfo => _windowInfo;
        
        // 窗口的遮罩
        protected GameObject _maskObject;

        /// <summary>
        /// 窗口初始化，只会在第一次打开时调用
        /// </summary>
        public virtual void OnInit()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            Debug.Log($"窗口 {WindowName} 初始化");
        }

        /// <summary>
        /// 窗口打开时调用
        /// </summary>
        /// <param name="args">传递参数</param>
        public virtual void OnOpen(object args = null)
        {
            gameObject.SetActive(true);
            
            // 如果参数是IWindowData类型，则进行数据注入
            if (args is IWindowData windowData)
            {
                InjectData(windowData);
            }
            
            Debug.Log($"窗口 {WindowName} 打开");
        }

        /// <summary>
        /// 窗口关闭时调用
        /// </summary>
        public virtual void OnClose()
        {
            gameObject.SetActive(false);
            Debug.Log($"窗口 {WindowName} 关闭");
        }

        /// <summary>
        /// 窗口刷新时调用
        /// </summary>
        /// <param name="args">传递参数</param>
        public virtual void OnRefresh(object args = null)
        {
            // 如果参数是IWindowData类型，则进行数据注入
            if (args is IWindowData windowData)
            {
                InjectData(windowData);
            }
            
            Debug.Log($"窗口 {WindowName} 刷新");
        }

        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        public void Close()
        {
            WindowManager.Instance.CloseWindow(WindowName);
        }
        
        /// <summary>
        /// 注入窗口数据
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        public virtual void InjectData(IWindowData windowData)
        {
            _windowData = windowData;
        }
        
        /// <summary>
        /// 设置窗口信息
        /// </summary>
        /// <param name="windowInfo">窗口信息</param>
        public void SetWindowInfo(WindowInfo windowInfo)
        {
            _windowInfo = windowInfo;
        }
        
        /// <summary>
        /// 设置窗口排序
        /// </summary>
        /// <param name="order">排序顺序</param>
        public virtual void SetSortingOrder(int order)
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = order;
            }
            else
            {
                Canvas[] canvases = GetComponentsInChildren<Canvas>();
                foreach (var c in canvases)
                {
                    c.sortingOrder = order;
                }
            }
        }
        
        /// <summary>
        /// 创建遮罩
        /// </summary>
        /// <param name="alpha">透明度</param>
        public virtual void CreateMask(float alpha = 0.5f)
        {
            if (_maskObject != null)
                return;
                
            // 创建遮罩对象
            _maskObject = new GameObject("WindowMask");
            _maskObject.transform.SetParent(transform);
            _maskObject.transform.SetAsFirstSibling(); // 放在最底层
            
            // 添加必要组件
            RectTransform rectTransform = _maskObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 添加Image组件作为遮罩
            UnityEngine.UI.Image maskImage = _maskObject.AddComponent<UnityEngine.UI.Image>();
            maskImage.color = new Color(0, 0, 0, alpha);
        }
        
        /// <summary>
        /// 销毁遮罩
        /// </summary>
        public virtual void DestroyMask()
        {
            if (_maskObject != null)
            {
                Destroy(_maskObject);
                _maskObject = null;
            }
        }
    }
} 