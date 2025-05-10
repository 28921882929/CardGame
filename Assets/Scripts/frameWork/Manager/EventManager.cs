using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.frameWork.Manager
{
    /// <summary>
    /// 事件管理器 - 用于注册和分发事件
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        private static EventManager instance;

        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("EventManager");
                    instance = go.AddComponent<EventManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // 无参数事件字典
        private Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();
        
        // 单参数事件字典
        private Dictionary<string, Dictionary<Type, object>> paramEventDictionary = new Dictionary<string, Dictionary<Type, object>>();

        #region 无参数事件

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void AddListener(string eventName, Action listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogError("事件名称不能为空");
                return;
            }

            if (listener == null)
            {
                Debug.LogError("事件监听器不能为空");
                return;
            }

            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary.Add(eventName, listener);
            }
            else
            {
                eventDictionary[eventName] += listener;
            }
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void RemoveListener(string eventName, Action listener)
        {
            if (string.IsNullOrEmpty(eventName) || listener == null)
            {
                return;
            }

            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= listener;

                // 如果没有监听器了，则移除该事件
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void TriggerEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName]?.Invoke();
            }
        }

        #endregion

        #region 带参数事件

        /// <summary>
        /// 添加带参数的事件监听
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void AddListener<T>(string eventName, Action<T> listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogError("事件名称不能为空");
                return;
            }

            if (listener == null)
            {
                Debug.LogError("事件监听器不能为空");
                return;
            }

            if (!paramEventDictionary.ContainsKey(eventName))
            {
                paramEventDictionary.Add(eventName, new Dictionary<Type, object>());
            }

            Type paramType = typeof(T);
            Dictionary<Type, object> typeDic = paramEventDictionary[eventName];

            if (!typeDic.ContainsKey(paramType))
            {
                typeDic.Add(paramType, listener);
            }
            else
            {
                typeDic[paramType] = (Action<T>)typeDic[paramType] + listener;
            }
        }

        /// <summary>
        /// 移除带参数的事件监听
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void RemoveListener<T>(string eventName, Action<T> listener)
        {
            if (string.IsNullOrEmpty(eventName) || listener == null)
            {
                return;
            }

            if (paramEventDictionary.ContainsKey(eventName))
            {
                Type paramType = typeof(T);
                Dictionary<Type, object> typeDic = paramEventDictionary[eventName];

                if (typeDic.ContainsKey(paramType))
                {
                    Action<T> action = (Action<T>)typeDic[paramType];
                    action -= listener;

                    if (action == null)
                    {
                        typeDic.Remove(paramType);

                        // 如果没有任何参数类型的监听器了，则移除该事件
                        if (typeDic.Count == 0)
                        {
                            paramEventDictionary.Remove(eventName);
                        }
                    }
                    else
                    {
                        typeDic[paramType] = action;
                    }
                }
            }
        }

        /// <summary>
        /// 触发带参数的事件
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="param">参数</param>
        public void TriggerEvent<T>(string eventName, T param)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (paramEventDictionary.ContainsKey(eventName))
            {
                Type paramType = typeof(T);
                Dictionary<Type, object> typeDic = paramEventDictionary[eventName];

                if (typeDic.ContainsKey(paramType))
                {
                    Action<T> action = (Action<T>)typeDic[paramType];
                    action?.Invoke(param);
                }
            }
        }

        #endregion

        /// <summary>
        /// 清除指定事件的所有监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void ClearEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary.Remove(eventName);
            }

            if (paramEventDictionary.ContainsKey(eventName))
            {
                paramEventDictionary.Remove(eventName);
            }
        }

        /// <summary>
        /// 清除所有事件
        /// </summary>
        public void ClearAllEvents()
        {
            eventDictionary.Clear();
            paramEventDictionary.Clear();
        }

        private void OnDestroy()
        {
            ClearAllEvents();
        }
    }
} 