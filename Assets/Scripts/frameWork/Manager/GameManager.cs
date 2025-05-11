using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.frameWork.Manager
{
    /// <summary>
    /// 游戏管理器 - 用于管理游戏生命周期和状态
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        /// <summary>
        /// 游戏状态枚举
        /// </summary>
        public enum GameState
        {
            None,           // 初始状态
            Initializing,   // 初始化中
            MainMenu,       // 主菜单
            Loading,        // 加载中
            Playing,        // 游戏中
            Paused,         // 暂停
            GameOver,       // 游戏结束
            Victory         // 胜利
        }

        // 当前游戏状态
        private GameState currentState = GameState.None;

        // 上一个游戏状态
        private GameState previousState = GameState.None;

        // 游戏数据
        private Dictionary<string, object> gameData = new Dictionary<string, object>();

        // 游戏是否已初始化
        private bool isInitialized = false;

        // 游戏设置
        [SerializeField] private float gameSpeed = 1.0f;
        [SerializeField] private bool enableSound = true;
        [SerializeField] private bool enableMusic = true;

        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        public GameState CurrentState => currentState;

        /// <summary>
        /// 获取上一个游戏状态
        /// </summary>
        public GameState PreviousState => previousState;

        /// <summary>
        /// 游戏是否暂停
        /// </summary>
        public bool IsPaused => currentState == GameState.Paused;

        /// <summary>
        /// 游戏是否结束
        /// </summary>
        public bool IsGameOver => currentState == GameState.GameOver || currentState == GameState.Victory;

        /// <summary>
        /// 游戏是否正在进行
        /// </summary>
        public bool IsPlaying => currentState == GameState.Playing;

        /// <summary>
        /// 游戏速度
        /// </summary>
        public float GameSpeed
        {
            get => gameSpeed;
            set
            {
                gameSpeed = Mathf.Clamp(value, 0.1f, 2.0f);
                Time.timeScale = gameSpeed;
            }
        }

        /// <summary>
        /// 是否启用音效
        /// </summary>
        public bool EnableSound
        {
            get => enableSound;
            set
            {
                enableSound = value;
                // 可以在这里设置音效组件的静音状态
            }
        }

        /// <summary>
        /// 是否启用音乐
        /// </summary>
        public bool EnableMusic
        {
            get => enableMusic;
            set
            {
                enableMusic = value;
                // 可以在这里设置音乐组件的静音状态
            }
        }

        private void Awake()
        {
            // 确保只有一个GameManager实例
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 初始化游戏
            Initialize();
        }

        #region 游戏生命周期

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            ChangeState(GameState.Initializing);

            // 初始化其他管理器
            InitializeManagers();

            // 加载游戏配置
            LoadGameSettings();

            isInitialized = true;
            Debug.Log("游戏初始化完成");

            // 初始化完成后进入主菜单
            ChangeState(GameState.MainMenu);

            EventManager.Instance.TriggerEvent("DemoTest");
        }

        /// <summary>
        /// 初始化所有管理器
        /// </summary>
        private void InitializeManagers()
        {
            // 确保所有管理器都被初始化
            try
            {
                // 获取所有管理器的实例，确保它们被创建
                var resourceManager = ResourceManager.Instance;
                var eventManager = EventManager.Instance;
                var sceneManager = SceneManager.Instance;
                var objectPoolManager = ObjectPoolManager.Instance;
                var uiLayerManager = UILayerManager.Instance;
                // 注册游戏全局事件
                RegisterGlobalEvents();
            }
            catch (Exception e)
            {
                Debug.LogError($"初始化管理器时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 注册全局事件
        /// </summary>
        private void RegisterGlobalEvents()
        {
            // 注册全局事件监听
            EventManager.Instance.AddListener("GameStart", OnGameStart);
            EventManager.Instance.AddListener("GamePause", PauseGame);
            EventManager.Instance.AddListener("GameResume", ResumeGame);
            EventManager.Instance.AddListener("GameOver", OnGameOver);
            EventManager.Instance.AddListener("GameVictory", OnGameVictory);
        }

        /// <summary>
        /// 加载游戏设置
        /// </summary>
        private void LoadGameSettings()
        {
            // 从PlayerPrefs加载游戏设置
            gameSpeed = PlayerPrefs.GetFloat("GameSpeed", 1.0f);
            enableSound = PlayerPrefs.GetInt("EnableSound", 1) == 1;
            enableMusic = PlayerPrefs.GetInt("EnableMusic", 1) == 1;

            // 应用设置
            Time.timeScale = gameSpeed;
        }

        /// <summary>
        /// 保存游戏设置
        /// </summary>
        public void SaveGameSettings()
        {
            PlayerPrefs.SetFloat("GameSpeed", gameSpeed);
            PlayerPrefs.SetInt("EnableSound", enableSound ? 1 : 0);
            PlayerPrefs.SetInt("EnableMusic", enableMusic ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            if (currentState == GameState.MainMenu || currentState == GameState.GameOver || currentState == GameState.Victory)
            {
                Debug.Log("开始游戏");
                // 触发游戏开始事件
                EventManager.Instance.TriggerEvent("GameStart");
            }
        }

        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        private void OnGameStart()
        {
            // 切换状态到游戏中
            ChangeState(GameState.Playing);

            // 加载游戏场景
            SceneManager.Instance.LoadSceneAsync("GameLevel1", UnityEngine.SceneManagement.LoadSceneMode.Single,
                onComplete: () =>
                {
                    // 游戏场景加载完成后的逻辑
                    Debug.Log("游戏场景加载完成");
                });
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0;

                // 触发暂停事件
                EventManager.Instance.TriggerEvent("GamePaused");
            }
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                Time.timeScale = gameSpeed;

                // 触发恢复事件
                EventManager.Instance.TriggerEvent("GameResumed");
            }
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        public void OnGameOver()
        {
            ChangeState(GameState.GameOver);

            // 触发游戏结束事件
            EventManager.Instance.TriggerEvent("GameOvered");
        }

        /// <summary>
        /// 游戏胜利
        /// </summary>
        public void OnGameVictory()
        {
            ChangeState(GameState.Victory);

            // 触发游戏胜利事件
            EventManager.Instance.TriggerEvent("GameVictoried");
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            // 加载主菜单场景
            SceneManager.Instance.LoadSceneAsync("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single,
                onComplete: () =>
                {
                    ChangeState(GameState.MainMenu);
                });
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            // 保存游戏设置
            SaveGameSettings();

            // 触发退出游戏事件
            EventManager.Instance.TriggerEvent("GameQuit");

            // 退出游戏
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region 游戏状态管理

        /// <summary>
        /// 改变游戏状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public void ChangeState(GameState newState)
        {
            // 如果状态没有变化，则不处理
            if (currentState == newState)
            {
                return;
            }

            // 保存上一个状态
            previousState = currentState;

            // 离开当前状态
            OnExitState(currentState);

            // 更新当前状态
            currentState = newState;

            // 进入新状态
            OnEnterState(newState);

            Debug.Log("状态改变: " + currentState);
            // 触发状态变化事件
            EventManager.Instance.TriggerEvent<GameState>("GameStateChanged", newState);
        }

        /// <summary>
        /// 进入状态时的处理
        /// </summary>
        /// <param name="state">状态</param>
        private void OnEnterState(GameState state)
        {
            switch (state)
            {
                case GameState.Initializing:
                    // 初始化状态处理
                    break;
                case GameState.MainMenu:
                    // 主菜单状态处理
                    Time.timeScale = 1;
                    break;
                case GameState.Loading:
                    // 加载状态处理
                    break;
                case GameState.Playing:
                    // 游戏中状态处理
                    Time.timeScale = gameSpeed;
                    break;
                case GameState.Paused:
                    // 暂停状态处理
                    Time.timeScale = 0;
                    break;
                case GameState.GameOver:
                    // 游戏结束状态处理
                    break;
                case GameState.Victory:
                    // 胜利状态处理
                    break;
            }
        }

        /// <summary>
        /// 离开状态时的处理
        /// </summary>
        /// <param name="state">状态</param>
        private void OnExitState(GameState state)
        {
            switch (state)
            {
                case GameState.Initializing:
                    // 离开初始化状态处理
                    break;
                case GameState.MainMenu:
                    // 离开主菜单状态处理
                    break;
                case GameState.Loading:
                    // 离开加载状态处理
                    break;
                case GameState.Playing:
                    // 离开游戏中状态处理
                    break;
                case GameState.Paused:
                    // 离开暂停状态处理
                    break;
                case GameState.GameOver:
                    // 离开游戏结束状态处理
                    break;
                case GameState.Victory:
                    // 离开胜利状态处理
                    break;
            }
        }

        #endregion

        #region 游戏数据管理

        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SaveData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("数据键不能为空");
                return;
            }

            if (gameData.ContainsKey(key))
            {
                gameData[key] = value;
            }
            else
            {
                gameData.Add(key, value);
            }
        }

        /// <summary>
        /// 获取游戏数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数据值</returns>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key) || !gameData.ContainsKey(key))
            {
                return defaultValue;
            }

            if (gameData[key] is T value)
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// 清除游戏数据
        /// </summary>
        /// <param name="key">键，为空则清除所有</param>
        public void ClearData(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                gameData.Clear();
            }
            else if (gameData.ContainsKey(key))
            {
                gameData.Remove(key);
            }
        }

        #endregion

        private void OnDestroy()
        {
            // 保存游戏设置
            SaveGameSettings();

            // 清理事件监听
            if (EventManager.Instance != null)
            {
                EventManager.Instance.RemoveListener("GameStart", OnGameStart);
                EventManager.Instance.RemoveListener("GamePause", PauseGame);
                EventManager.Instance.RemoveListener("GameResume", ResumeGame);
                EventManager.Instance.RemoveListener("GameOver", OnGameOver);
                EventManager.Instance.RemoveListener("GameVictory", OnGameVictory);
            }
        }
    }
}