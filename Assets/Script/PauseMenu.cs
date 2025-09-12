using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("PauseMenu Start - 脚本已启动");
        // 初始隐藏暂停菜单
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("ESC键被按下");
            if (GameIsPaused)
            {
                Debug.Log("调用 Resume 方法");
                Resume();
            }
            else
            {
                Debug.Log("调用 Pause 方法");
                Pause();
            }
        }
    }

    public void Resume()
    {
        // 隐藏暂停菜单UI
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        // 恢复游戏状态
        GameStateManager.Instance.SetState(GameState.Playing);
        GameIsPaused = false;
        Time.timeScale = 1f;
        Debug.Log("游戏继续");
    }

    public void Pause()
    {
        // 显示暂停菜单UI
        if (pauseMenuUI != null)
        {
            Debug.Log("显示暂停菜单UI");
            pauseMenuUI.SetActive(true);
        }
        // 暂停游戏状态
        GameStateManager.Instance.SetState(GameState.Paused);
        GameIsPaused = true;
        Time.timeScale = 0f;
        Debug.Log("游戏暂停");
    }

    public void LoadMenu()
    {
        // 恢复时间流速
        Time.timeScale = 1f;
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
        // 设置菜单状态
        GameStateManager.Instance.SetState(GameState.Menu);

        // 加载菜单场景
        SceneManager.LoadScene("Menu");
        GameStateManager gsm = GameStateManager.Instance;
        gsm.SaveGameProgress();

        Debug.Log("加载菜单");
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏...");

        // 恢复时间流速（确保退出前状态正常）
        Time.timeScale = 1f;
        GameIsPaused = false;

        Application.Quit();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // 停止编辑器中的播放模式
#endif
    }

    // 可选：添加状态变化事件监听
    void OnEnable()
    {
        // 注册状态变化事件，确保UI状态同步
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    void OnDisable()
    {
        // 取消注册事件
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    // 响应游戏状态变化
    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Paused)
        {
            if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
            GameIsPaused = true;
        }
        else if (newState == GameState.Playing)
        {
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            GameIsPaused = false;
        }
    }
}