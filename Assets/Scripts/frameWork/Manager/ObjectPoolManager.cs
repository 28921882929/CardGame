using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.frameWork.Manager
{
    /// <summary>
    /// 对象池管理器 - 用于管理和复用游戏对象
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        private static ObjectPoolManager instance;

        public static ObjectPoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("ObjectPoolManager");
                    instance = go.AddComponent<ObjectPoolManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // 对象池字典，键为对象池名称，值为对象池中的对象列表
        private Dictionary<string, List<GameObject>> poolDictionary = new Dictionary<string, List<GameObject>>();
        
        // 预制体字典，键为对象池名称，值为对应的预制体
        private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="prefab">预制体</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="container">容器（可选）</param>
        public void CreatePool(string poolName, GameObject prefab, int initialSize = 0, Transform container = null)
        {
            if (poolDictionary.ContainsKey(poolName))
            {
                Debug.LogWarning($"对象池 '{poolName}' 已经存在！");
                return;
            }

            prefabDictionary[poolName] = prefab;
            List<GameObject> objectPool = new List<GameObject>();
            poolDictionary[poolName] = objectPool;

            // 预先创建对象
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = CreateNewObject(poolName, container);
                obj.SetActive(false);
                objectPool.Add(obj);
            }
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>池中的游戏对象</returns>
        public GameObject GetObjectFromPool(string poolName, Vector3 position = default, Quaternion rotation = default)
        {
            // 检查对象池是否存在
            if (!poolDictionary.ContainsKey(poolName))
            {
                Debug.LogError($"对象池 '{poolName}' 不存在！请先创建对象池。");
                return null;
            }

            List<GameObject> objectPool = poolDictionary[poolName];

            // 查找可用对象
            for (int i = 0; i < objectPool.Count; i++)
            {
                if (objectPool[i] != null && !objectPool[i].activeInHierarchy)
                {
                    GameObject obj = objectPool[i];
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.SetActive(true);
                    return obj;
                }
            }

            // 如果没有可用对象，创建新对象
            GameObject newObj = CreateNewObject(poolName);
            newObj.transform.position = position;
            newObj.transform.rotation = rotation;
            objectPool.Add(newObj);
            return newObj;
        }

        /// <summary>
        /// 返回对象到对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="obj">要返回的对象</param>
        public void ReturnObjectToPool(string poolName, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(poolName))
            {
                Debug.LogWarning($"对象池 '{poolName}' 不存在！无法返回对象。");
                return;
            }

            obj.SetActive(false);
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        public void ClearPool(string poolName)
        {
            if (!poolDictionary.ContainsKey(poolName))
            {
                Debug.LogWarning($"对象池 '{poolName}' 不存在！无法清空。");
                return;
            }

            List<GameObject> objectPool = poolDictionary[poolName];
            foreach (GameObject obj in objectPool)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            objectPool.Clear();
        }

        /// <summary>
        /// 创建新对象
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="container">父对象</param>
        /// <returns>新创建的游戏对象</returns>
        private GameObject CreateNewObject(string poolName, Transform container = null)
        {
            GameObject prefab = prefabDictionary[poolName];
            GameObject obj = Instantiate(prefab);
            obj.name = $"{poolName}_{obj.GetInstanceID()}";

            if (container != null)
            {
                obj.transform.SetParent(container);
            }

            return obj;
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var poolName in poolDictionary.Keys)
            {
                ClearPool(poolName);
            }
            poolDictionary.Clear();
            prefabDictionary.Clear();
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
} 