using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI 按钮")]
    public Button playButton;
    public Button continueButton;
    public Button quitButton;

    void Start()
    {
        // 检查是否有存档，控制Continue按钮显示
        UpdateContinueButton();
    }

    // 开始新游戏
    public void StartNewGame()
    {
        Debug.Log("开始新游戏");
        PauseMenu.GameIsPaused = false;
        Time.timeScale = 1f;

        // 清除现有存档（可选）
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ClearSaveData();
        }

        SceneManager.LoadScene("Game");
    }

    // 继续游戏（读档）
    public void ContinueGame()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager 不存在！");
            return;
        }

        if (!GameStateManager.Instance.HasSaveData())
        {
            Debug.LogWarning("没有存档数据");
            return;
        }

        Debug.Log("继续游戏 - 加载存档");
        PauseMenu.GameIsPaused = false;
        Time.timeScale = 1f;

        // 标记为加载存档模式
        GameStateManager.Instance.SetLoadingFromSave(true);
        SceneManager.LoadScene("Game");
    }

    // 退出游戏
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }

    // 更新Continue按钮的显示状态
    private void UpdateContinueButton()
    {
        if (continueButton != null && GameStateManager.Instance != null)
        {
            bool hasSave = GameStateManager.Instance.HasSaveData();
            continueButton.interactable = hasSave;

            // 可选：如果没有存档，让按钮变灰或隐藏
            if (!hasSave)
            {
                continueButton.GetComponent<Image>().color = Color.gray;
            }
            else
            {
                continueButton.GetComponent<Image>().color = Color.white;
            }
        }
    }
}