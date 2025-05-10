using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace frameWork.UI
{
    /// <summary>
    /// 窗口管理器：负责窗口的打开、关闭、加载等操作
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        private static WindowManager _instance;
        public static WindowManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WindowManager");
                    _instance = go.AddComponent<WindowManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // 已打开的窗口字典
        private Dictionary<string, BaseWindow> _openedWindows = new Dictionary<string, BaseWindow>();
        // 窗口的父容器
        private Transform _uiRoot;
        // 各层级的根容器
        private Dictionary<UILayer, Transform> _layerRoots = new Dictionary<UILayer, Transform>();
        // 基础排序顺序
        private Dictionary<UILayer, int> _baseSortingOrders = new Dictionary<UILayer, int>();
        // 各层级的窗口栈（存储暂时隐藏的窗口）
        private Dictionary<UILayer, Stack<BaseWindow>> _windowStacks = new Dictionary<UILayer, Stack<BaseWindow>>();
        // 各层级的当前显示窗口
        private Dictionary<UILayer, BaseWindow> _currentLayerWindows = new Dictionary<UILayer, BaseWindow>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 创建UI根容器
            GameObject root = new GameObject("UI_Root");
            _uiRoot = root.transform;
            DontDestroyOnLoad(root);
            
            // 创建Canvas作为UI显示的容器
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(_uiRoot);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 初始化UI层级
            InitUILayers();
        }
        
        /// <summary>
        /// 初始化UI层级
        /// </summary>
        private void InitUILayers()
        {
            _layerRoots.Clear();
            _baseSortingOrders.Clear();
            _windowStacks.Clear();
            _currentLayerWindows.Clear();
            
            // 为每个UI层级创建一个根容器
            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                GameObject layerRoot = new GameObject(layer.ToString());
                layerRoot.transform.SetParent(_uiRoot);
                
                RectTransform rectTransform = layerRoot.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                _layerRoots[layer] = layerRoot.transform;
                
                // 设置基础排序顺序，每个层级间隔100，留足空间给同层级内不同窗口的排序
                _baseSortingOrders[layer] = (int)layer * 100;
                
                // 初始化每个层级的窗口栈
                _windowStacks[layer] = new Stack<BaseWindow>();
            }
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <param name="args">传递参数</param>
        /// <returns>窗口实例</returns>
        public BaseWindow OpenWindow(string windowName, object args = null)
        {
            // 如果窗口已经打开，直接返回
            if (_openedWindows.TryGetValue(windowName, out BaseWindow window))
            {
                // 如果窗口已经是当前层级窗口，则只刷新
                UILayer layer = window.WindowInfo.Layer;
                if (_currentLayerWindows.TryGetValue(layer, out BaseWindow currentWindow) && currentWindow == window)
                {
                    window.OnRefresh(args);
                    return window;
                }
                else
                {
                    // 如果窗口已打开但不是当前层级窗口，则将其设为当前窗口
                    SetCurrentLayerWindow(window);
                    window.OnRefresh(args);
                    return window;
                }
            }

            // 从注册表获取窗口信息
            WindowInfo windowInfo = WindowRegister.GetWindowInfo(windowName);
            if (windowInfo == null)
            {
                Debug.LogError($"窗口 {windowName} 未注册！");
                return null;
            }

            // 加载窗口预制体
            GameObject prefab = Resources.Load<GameObject>(windowInfo.PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"窗口预制体 {windowInfo.PrefabPath} 加载失败！");
                return null;
            }

            // 获取对应层级的父容器
            Transform layerRoot = _layerRoots[windowInfo.Layer];
            
            // 实例化窗口
            GameObject windowObj = Instantiate(prefab, layerRoot);
            window = windowObj.GetComponent<BaseWindow>();
            if (window == null)
            {
                Debug.LogError($"窗口 {windowName} 缺少BaseWindow组件！");
                Destroy(windowObj);
                return null;
            }

            // 初始化窗口
            window.WindowName = windowName;
            window.SetWindowInfo(windowInfo);
            
            // 设置窗口排序
            int sortingOrder = _baseSortingOrders[windowInfo.Layer] + windowInfo.SortingOrder;
            window.SetSortingOrder(sortingOrder);
            
            // 如果需要遮罩，添加遮罩
            if (windowInfo.NeedMask)
            {
                window.CreateMask(windowInfo.MaskAlpha);
            }
            
            window.OnInit();
            
            // 添加到已打开窗口字典
            _openedWindows[windowName] = window;
            
            // 处理同层级窗口管理
            SetCurrentLayerWindow(window);
            
            // 打开窗口
            window.OnOpen(args);
            
            // 重新排序所有窗口
            RefreshWindowOrder();

            return window;
        }
        
        /// <summary>
        /// 设置当前层级窗口
        /// </summary>
        /// <param name="window">要设为当前窗口的窗口</param>
        private void SetCurrentLayerWindow(BaseWindow window)
        {
            UILayer layer = window.WindowInfo.Layer;
            
            // 如果当前层级已有窗口显示
            if (_currentLayerWindows.TryGetValue(layer, out BaseWindow currentWindow))
            {
                // 如果是同一个窗口，无需处理
                if (currentWindow == window)
                    return;
                
                // 将当前窗口隐藏并入栈
                currentWindow.gameObject.SetActive(false);
                _windowStacks[layer].Push(currentWindow);
                Debug.Log($"窗口 {currentWindow.WindowName} 被隐藏并入栈");
            }
            
            // 设置新的当前窗口
            _currentLayerWindows[layer] = window;
            window.gameObject.SetActive(true);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        public void CloseWindow(string windowName)
        {
            if (_openedWindows.TryGetValue(windowName, out BaseWindow window))
            {
                UILayer layer = window.WindowInfo.Layer;
                
                // 从当前层级窗口中移除
                if (_currentLayerWindows.TryGetValue(layer, out BaseWindow currentWindow) && currentWindow == window)
                {
                    _currentLayerWindows.Remove(layer);
                    
                    // 如果栈中有窗口，则弹出并显示
                    if (_windowStacks[layer].Count > 0)
                    {
                        BaseWindow nextWindow = _windowStacks[layer].Pop();
                        _currentLayerWindows[layer] = nextWindow;
                        nextWindow.gameObject.SetActive(true);
                        Debug.Log($"从栈中弹出窗口 {nextWindow.WindowName} 并显示");
                    }
                }
                else
                {
                    // 如果不是当前层级窗口，则从栈中移除
                    Stack<BaseWindow> tempStack = new Stack<BaseWindow>();
                    while (_windowStacks[layer].Count > 0)
                    {
                        BaseWindow stackWindow = _windowStacks[layer].Pop();
                        if (stackWindow != window)
                        {
                            tempStack.Push(stackWindow);
                        }
                    }
                    
                    // 恢复栈
                    while (tempStack.Count > 0)
                    {
                        _windowStacks[layer].Push(tempStack.Pop());
                    }
                }
                
                // 关闭窗口
                window.OnClose();
                _openedWindows.Remove(windowName);
                Destroy(window.gameObject);
                
                // 刷新窗口排序
                RefreshWindowOrder();
            }
        }

        /// <summary>
        /// 关闭所有窗口
        /// </summary>
        public void CloseAllWindows()
        {
            // 清空所有栈
            foreach (var layer in _windowStacks.Keys.ToList())
            {
                while (_windowStacks[layer].Count > 0)
                {
                    BaseWindow window = _windowStacks[layer].Pop();
                    window.OnClose();
                    Destroy(window.gameObject);
                }
            }
            
            // 清空当前层级窗口
            foreach (var layer in _currentLayerWindows.Keys.ToList())
            {
                BaseWindow window = _currentLayerWindows[layer];
                window.OnClose();
                Destroy(window.gameObject);
            }
            
            // 清空窗口字典
            foreach (var window in _openedWindows.Values.ToList())
            {
                if (window != null && window.gameObject != null)
                {
                    Destroy(window.gameObject);
                }
            }
            
            _openedWindows.Clear();
            _currentLayerWindows.Clear();
        }

        /// <summary>
        /// 获取已打开的窗口
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <returns>窗口实例</returns>
        public BaseWindow GetWindow(string windowName)
        {
            _openedWindows.TryGetValue(windowName, out BaseWindow window);
            return window;
        }
        
        /// <summary>
        /// 刷新窗口排序顺序
        /// </summary>
        private void RefreshWindowOrder()
        {
            // 按层级分组
            var groupedWindows = _openedWindows.Values
                .GroupBy(w => w.WindowInfo.Layer)
                .ToDictionary(g => g.Key, g => g.ToList());
                
            // 对每个层级内的窗口进行排序
            foreach (var layer in groupedWindows.Keys)
            {
                int baseOrder = _baseSortingOrders[layer];
                List<BaseWindow> windows = groupedWindows[layer];
                
                // 按SortingOrder排序
                windows.Sort((a, b) => a.WindowInfo.SortingOrder.CompareTo(b.WindowInfo.SortingOrder));
                
                // 应用排序顺序
                for (int i = 0; i < windows.Count; i++)
                {
                    windows[i].SetSortingOrder(baseOrder + i);
                }
            }
        }
        
        /// <summary>
        /// 将窗口移到最前面
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        public void BringWindowToFront(string windowName)
        {
            if (!_openedWindows.TryGetValue(windowName, out BaseWindow window))
                return;
                
            // 设置为当前层级窗口
            SetCurrentLayerWindow(window);
                
            // 获取同层级的所有窗口
            var sameLayerWindows = _openedWindows.Values
                .Where(w => w.WindowInfo.Layer == window.WindowInfo.Layer)
                .ToList();
                
            // 找到当前层级最高的排序值
            int maxSortingOrder = 0;
            foreach (var w in sameLayerWindows)
            {
                if (w.WindowInfo.SortingOrder > maxSortingOrder)
                    maxSortingOrder = w.WindowInfo.SortingOrder;
            }
            
            // 将当前窗口的SortingOrder设为最高值+1
            window.WindowInfo.SortingOrder = maxSortingOrder + 1;
            
            // 刷新窗口排序
            RefreshWindowOrder();
        }
        
        /// <summary>
        /// 返回上一个窗口
        /// </summary>
        /// <param name="layer">UI层级</param>
        /// <returns>是否成功返回</returns>
        public bool GoToPreviousWindow(UILayer layer)
        {
            // 检查当前层级是否有窗口
            if (!_currentLayerWindows.TryGetValue(layer, out BaseWindow currentWindow))
                return false;
                
            // 检查栈中是否有窗口
            if (_windowStacks[layer].Count == 0)
                return false;
                
            // 关闭当前窗口
            CloseWindow(currentWindow.WindowName);
            return true;
        }
        
        /// <summary>
        /// 获取当前层级显示的窗口
        /// </summary>
        /// <param name="layer">UI层级</param>
        /// <returns>当前窗口</returns>
        public BaseWindow GetCurrentLayerWindow(UILayer layer)
        {
            _currentLayerWindows.TryGetValue(layer, out BaseWindow window);
            return window;
        }
        
        /// <summary>
        /// 获取窗口栈深度
        /// </summary>
        /// <param name="layer">UI层级</param>
        /// <returns>栈深度</returns>
        public int GetWindowStackCount(UILayer layer)
        {
            return _windowStacks[layer].Count;
        }
    }
} 