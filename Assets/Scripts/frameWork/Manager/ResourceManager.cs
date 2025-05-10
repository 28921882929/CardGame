using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace Assets.Scripts.frameWork.Manager
{
    /// <summary>
    /// 资源管理器 - 用于加载和管理游戏资源
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager instance;

        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("ResourceManager");
                    instance = go.AddComponent<ResourceManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // 缓存已加载的资源
        private Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();

        #region 同步加载

        /// <summary>
        /// 加载资源（同步）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="useCache">是否使用缓存</param>
        /// <returns>加载的资源</returns>
        public T Load<T>(string path, bool useCache = true) where T : UnityEngine.Object
        {
            // 检查缓存
            if (useCache && resourceCache.ContainsKey(path))
            {
                return resourceCache[path] as T;
            }

            // 加载资源
            T resource = Resources.Load<T>(path);

            if (resource == null)
            {
                Debug.LogError($"加载资源失败: {path}");
                return null;
            }

            // 添加到缓存
            if (useCache)
            {
                resourceCache[path] = resource;
            }

            return resource;
        }

        /// <summary>
        /// 加载游戏对象（同步）
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="parent">父节点</param>
        /// <param name="useCache">是否使用缓存</param>
        /// <returns>加载的游戏对象</returns>
        public GameObject LoadGameObject(string path, Transform parent = null, bool useCache = true)
        {
            GameObject prefab = Load<GameObject>(path, useCache);
            if (prefab == null)
            {
                return null;
            }

            GameObject go = Instantiate(prefab, parent);
            go.name = prefab.name;
            return go;
        }

        #endregion

        #region 异步加载

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="useCache">是否使用缓存</param>
        public void LoadAsync<T>(string path, Action<T> callback, bool useCache = true) where T : UnityEngine.Object
        {
            // 检查缓存
            if (useCache && resourceCache.ContainsKey(path))
            {
                callback?.Invoke(resourceCache[path] as T);
                return;
            }

            // 开始异步加载
            StartCoroutine(LoadAsyncCoroutine(path, callback, useCache));
        }

        private IEnumerator LoadAsyncCoroutine<T>(string path, Action<T> callback, bool useCache) where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            yield return request;

            if (request.asset == null)
            {
                Debug.LogError($"异步加载资源失败: {path}");
                callback?.Invoke(null);
                yield break;
            }

            T resource = request.asset as T;

            // 添加到缓存
            if (useCache)
            {
                resourceCache[path] = resource;
            }

            callback?.Invoke(resource);
        }

        /// <summary>
        /// 异步加载游戏对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="parent">父节点</param>
        /// <param name="useCache">是否使用缓存</param>
        public void LoadGameObjectAsync(string path, Action<GameObject> callback, Transform parent = null, bool useCache = true)
        {
            LoadAsync<GameObject>(path, (prefab) =>
            {
                if (prefab == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                GameObject go = Instantiate(prefab, parent);
                go.name = prefab.name;
                callback?.Invoke(go);
            }, useCache);
        }

        #endregion

        #region 资源管理

        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="paths">资源路径列表</param>
        public void Preload<T>(List<string> paths) where T : UnityEngine.Object
        {
            foreach (string path in paths)
            {
                Load<T>(path, true);
            }
        }

        /// <summary>
        /// 预加载资源（异步）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="paths">资源路径列表</param>
        /// <param name="onComplete">全部加载完成的回调</param>
        /// <param name="onProgress">加载进度回调</param>
        public void PreloadAsync<T>(List<string> paths, Action onComplete = null, Action<float> onProgress = null) where T : UnityEngine.Object
        {
            StartCoroutine(PreloadAsyncCoroutine<T>(paths, onComplete, onProgress));
        }

        private IEnumerator PreloadAsyncCoroutine<T>(List<string> paths, Action onComplete, Action<float> onProgress) where T : UnityEngine.Object
        {
            int totalCount = paths.Count;
            int loadedCount = 0;

            foreach (string path in paths)
            {
                ResourceRequest request = Resources.LoadAsync<T>(path);
                yield return request;

                if (request.asset != null)
                {
                    resourceCache[path] = request.asset;
                }

                loadedCount++;
                onProgress?.Invoke((float)loadedCount / totalCount);
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// 清除资源缓存
        /// </summary>
        /// <param name="path">指定资源路径，为空则清除所有</param>
        public void ClearCache(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                resourceCache.Clear();
                Resources.UnloadUnusedAssets();
            }
            else if (resourceCache.ContainsKey(path))
            {
                resourceCache.Remove(path);
            }
        }

        /// <summary>
        /// 卸载未使用的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region 场景资源管理

        /// <summary>
        /// 加载场景资源
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="resourcePaths">需要预加载的资源路径列表</param>
        /// <param name="onComplete">加载完成回调</param>
        /// <param name="onProgress">进度回调</param>
        public void LoadSceneResources(string sceneName, List<string> resourcePaths, Action onComplete = null, Action<float> onProgress = null)
        {
            StartCoroutine(LoadSceneResourcesCoroutine(sceneName, resourcePaths, onComplete, onProgress));
        }

        private IEnumerator LoadSceneResourcesCoroutine(string sceneName, List<string> resourcePaths, Action onComplete, Action<float> onProgress)
        {
            // 记录加载开始时间
            float startTime = Time.realtimeSinceStartup;

            // 预加载资源
            int totalCount = resourcePaths.Count;
            int loadedCount = 0;

            foreach (string path in resourcePaths)
            {
                ResourceRequest request = Resources.LoadAsync(path);
                yield return request;

                if (request.asset != null)
                {
                    resourceCache[path] = request.asset;
                }

                loadedCount++;
                onProgress?.Invoke((float)loadedCount / totalCount);
            }

            // 记录加载结束时间
            float endTime = Time.realtimeSinceStartup;
            Debug.Log($"场景资源加载完成: {sceneName}, 耗时: {endTime - startTime}秒");

            onComplete?.Invoke();
        }

        #endregion

        private void OnDestroy()
        {
            ClearCache();
        }
    }
} 