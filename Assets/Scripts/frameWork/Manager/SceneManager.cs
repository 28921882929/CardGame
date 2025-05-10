using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.frameWork.Manager
{
    /// <summary>
    /// 场景管理器 - 用于场景加载和管理
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        private static SceneManager instance;

        public static SceneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("SceneManager");
                    instance = go.AddComponent<SceneManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // 当前场景名称
        private string currentSceneName;
        
        // 加载进度
        private float loadingProgress;
        
        // 是否正在加载场景
        private bool isLoading;
        
        // 场景数据字典，用于在场景之间传递数据
        private Dictionary<string, object> sceneData = new Dictionary<string, object>();

        /// <summary>
        /// 获取当前场景名称
        /// </summary>
        public string CurrentSceneName => currentSceneName;

        /// <summary>
        /// 获取场景加载进度
        /// </summary>
        public float LoadingProgress => loadingProgress;

        /// <summary>
        /// 是否正在加载场景
        /// </summary>
        public bool IsLoading => isLoading;

        private void Start()
        {
            // 初始化当前场景名称
            currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        #region 场景加载

        /// <summary>
        /// 加载场景（同步）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="loadMode">加载模式</param>
        public void LoadScene(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("场景名称不能为空");
                return;
            }

            // 更新当前场景名称
            if (loadMode == LoadSceneMode.Single)
            {
                currentSceneName = sceneName;
            }

            // 加载场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, loadMode);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="onComplete">加载完成回调</param>
        /// <param name="onProgress">加载进度回调</param>
        /// <param name="showLoadingUI">是否显示加载界面</param>
        public void LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, Action onComplete = null, Action<float> onProgress = null, bool showLoadingUI = true)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("场景名称不能为空");
                return;
            }

            if (isLoading)
            {
                Debug.LogWarning("已经有场景正在加载中，请等待加载完成");
                return;
            }

            StartCoroutine(LoadSceneAsyncCoroutine(sceneName, loadMode, onComplete, onProgress, showLoadingUI));
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, LoadSceneMode loadMode, Action onComplete, Action<float> onProgress, bool showLoadingUI)
        {
            isLoading = true;
            loadingProgress = 0f;

            // 显示加载界面
            if (showLoadingUI)
            {
                // 这里可以触发显示加载界面的事件
                EventManager.Instance.TriggerEvent("ShowLoadingUI");
            }

            // 预加载资源
            if (ResourceManager.Instance != null)
            {
                List<string> resourcePaths = GetSceneResourcePaths(sceneName);
                if (resourcePaths != null && resourcePaths.Count > 0)
                {
                    bool resourcesLoaded = false;

                    ResourceManager.Instance.LoadSceneResources(sceneName, resourcePaths, 
                        onComplete: () => { resourcesLoaded = true; },
                        onProgress: (progress) => 
                        { 
                            loadingProgress = progress * 0.5f; // 资源加载占总进度的50%
                            onProgress?.Invoke(loadingProgress);
                        });

                    // 等待资源加载完成
                    while (!resourcesLoaded)
                    {
                        yield return null;
                    }
                }
            }

            // 异步加载场景
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, loadMode);
            asyncOperation.allowSceneActivation = false; // 先不激活场景

            // 场景加载进度
            while (asyncOperation.progress < 0.9f) // 0.9是Unity场景加载的最大进度
            {
                loadingProgress = 0.5f + asyncOperation.progress * 0.5f; // 场景加载占总进度的50%
                onProgress?.Invoke(loadingProgress);
                yield return null;
            }

            loadingProgress = 1.0f;
            onProgress?.Invoke(loadingProgress);

            // 等待一小段时间，确保加载界面显示完整
            yield return new WaitForSeconds(0.2f);

            // 更新当前场景名称
            if (loadMode == LoadSceneMode.Single)
            {
                currentSceneName = sceneName;
            }

            // 激活场景
            asyncOperation.allowSceneActivation = true;

            // 等待场景完全加载
            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            // 隐藏加载界面
            if (showLoadingUI)
            {
                // 这里可以触发隐藏加载界面的事件
                EventManager.Instance.TriggerEvent("HideLoadingUI");
            }

            isLoading = false;
            
            // 通知场景已加载完成
            EventManager.Instance.TriggerEvent("SceneLoaded");
            EventManager.Instance.TriggerEvent<string>("SceneLoadedWithName", sceneName);
            
            // 完成回调
            onComplete?.Invoke();
        }

        /// <summary>
        /// 获取场景所需的资源路径列表
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>资源路径列表</returns>
        private List<string> GetSceneResourcePaths(string sceneName)
        {
            // 这里可以根据场景名称返回该场景需要预加载的资源列表
            // 实际项目中可以从配置文件或资源清单中获取
            
            // 示例：
            Dictionary<string, List<string>> sceneResourceMap = new Dictionary<string, List<string>>
            {
                // 示例场景资源，实际项目中应该根据需求配置
                {"MainMenu", new List<string>{"UI/MainMenu", "Audio/BGM/MenuBGM"}},
                {"GameLevel1", new List<string>{"Characters/Player", "Enemies/Enemy1", "Audio/BGM/Level1BGM"}}
                // 可以继续添加其他场景的资源列表
            };

            if (sceneResourceMap.ContainsKey(sceneName))
            {
                return sceneResourceMap[sceneName];
            }

            return null;
        }

        #endregion

        #region 场景数据

        /// <summary>
        /// 设置场景数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("数据键不能为空");
                return;
            }

            if (sceneData.ContainsKey(key))
            {
                sceneData[key] = value;
            }
            else
            {
                sceneData.Add(key, value);
            }
        }

        /// <summary>
        /// 获取场景数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数据值</returns>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key) || !sceneData.ContainsKey(key))
            {
                return defaultValue;
            }

            if (sceneData[key] is T value)
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// 清除场景数据
        /// </summary>
        /// <param name="key">键，为空则清除所有</param>
        public void ClearData(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                sceneData.Clear();
            }
            else if (sceneData.ContainsKey(key))
            {
                sceneData.Remove(key);
            }
        }

        #endregion

        /// <summary>
        /// 重新加载当前场景
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(currentSceneName);
        }

        /// <summary>
        /// 异步重新加载当前场景
        /// </summary>
        /// <param name="onComplete">加载完成回调</param>
        /// <param name="onProgress">加载进度回调</param>
        /// <param name="showLoadingUI">是否显示加载界面</param>
        public void ReloadCurrentSceneAsync(Action onComplete = null, Action<float> onProgress = null, bool showLoadingUI = true)
        {
            LoadSceneAsync(currentSceneName, LoadSceneMode.Single, onComplete, onProgress, showLoadingUI);
        }
    }
} 