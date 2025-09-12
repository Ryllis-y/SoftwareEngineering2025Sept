using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Paused,
    GameOver,
    Menu
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;

    private string saveFilePath;
    private bool isLoadingFromSave = false; // 标记是否正在从存档加载

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        }
        else
        {
            Destroy(gameObject);
        }

        CurrentState = GameState.Menu;
    }

    void Start()
    {
        // 注册场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 场景加载完成后的处理
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game" && isLoadingFromSave)
        {
            // 延迟加载，确保所有组件都已初始化
            Invoke("LoadGameProgressDelayed", 0.1f);
            isLoadingFromSave = false;
        }
        else if (scene.name == "Game")
        {
            // 新游戏，设置游戏状态为Playing
            SetState(GameState.Playing);
        }
    }

    // 延迟加载存档
    private void LoadGameProgressDelayed()
    {
        GameData data = LoadGameProgress();
        if (data != null)
        {
            ApplyGameData(data);
            SetState(GameState.Playing);
            Debug.Log("存档加载成功");
        }
        else
        {
            Debug.LogError("存档加载失败，开始新游戏");
            SetState(GameState.Playing);
        }
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        GameState previousState = CurrentState;
        CurrentState = newState;
        HandleStateTransition(previousState, newState);
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"游戏状态变化: {previousState} → {newState}");
    }

    private void HandleStateTransition(GameState fromState, GameState toState)
    {
        switch (toState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                AudioListener.pause = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                AudioListener.pause = true;
                Cursor.lockState = CursorLockMode.None;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                AudioListener.pause = true;
                Cursor.lockState = CursorLockMode.None;
                break;

            case GameState.Menu:
                Time.timeScale = 0f;
                AudioListener.pause = false;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    // 保存游戏进度
    public void SaveGameProgress()
    {
        try
        {
            GameData data = CollectGameData();
            string jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, jsonData);
            Debug.Log($"游戏进度已保存到: {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存失败: {e.Message}");
        }
    }

    // 加载游戏进度
    public GameData LoadGameProgress()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string jsonData = File.ReadAllText(saveFilePath);
                GameData data = JsonUtility.FromJson<GameData>(jsonData);
                Debug.Log("存档数据加载成功");
                return data;
            }
            Debug.LogWarning("没有找到保存文件");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载失败: {e.Message}");
            return null;
        }
    }

    // 检查是否有存档
    public bool HasSaveData()
    {
        return File.Exists(saveFilePath);
    }

    // 清除存档数据
    public void ClearSaveData()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("存档已清除");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"清除存档失败: {e.Message}");
        }
    }

    // 设置加载存档标记
    public void SetLoadingFromSave(bool loading)
    {
        isLoadingFromSave = loading;
    }

    // 应用存档数据
    private void ApplyGameData(GameData data)
    {
        // 恢复玩家数据
        PlayerScriptTest player = FindObjectOfType<PlayerScriptTest>();
        if (player != null)
        {
            player.RestorePlayerData(data);
        }

        // 恢复物品数据
        Inventory inventory = FindObjectOfType<Inventory>();
        // if (inventory != null)
        // {
        //     inventory.SetItemAmount(ItemType.DirtClump, data.dirtCount);
        //     inventory.SetItemAmount(ItemType.StoneChunk, data.stoneCount);
        //     inventory.SetItemAmount(ItemType.WoodLog, data.woodCount);
        //     inventory.SetItemAmount(ItemType.SandGrain, data.sandCount);
        //     inventory.SetItemAmount(ItemType.GlassPane, data.glassCount);
        // }

        // 恢复方块数据
        WorldData worldData = FindObjectOfType<WorldData>();
        if (worldData != null && data.blocks != null)
        {
            // 清除现有方块
            worldData.ClearAllBlocks();

            // 重建方块
            foreach (SerializableBlock blockData in data.blocks)
            {
                if (Enum.TryParse<BlockType>(blockData.type, out BlockType blockType))
                {
                    Vector2Int pos = new Vector2Int(blockData.x, blockData.y);
                    worldData.AddBlock(blockType, pos);

                    // 恢复方块生命值
                    BlockData block = worldData.GetBlockAt(pos);
                    if (block != null)
                    {
                        block.health = blockData.health;
                    }
                }
            }
        }
    }

    // 收集游戏数据
    private GameData CollectGameData()
    {
        GameData data = new GameData();

        // 玩家数据
        PlayerScriptTest player = FindObjectOfType<PlayerScriptTest>();
        if (player != null && player.player != null)
        {
            Vector3 pos = player.player.transform.position;
            data.playerX = pos.x;
            data.playerY = pos.y;
            data.playerHealth = player.currentHealth;
        }

        // 场景数据
        data.currentScene = SceneManager.GetActiveScene().name;
        data.gameTime = Time.realtimeSinceStartup;

        // 跳过物品数据收集，因为物品是无限的
        Debug.Log("跳过物品数据收集 - 物品为无限资源");
        data.dirtCount = 0;
        data.stoneCount = 0;
        data.woodCount = 0;
        data.sandCount = 0;
        data.glassCount = 0;

        // 方块数据
        WorldData worldData = FindObjectOfType<WorldData>();
        if (worldData != null)
        {
            var allBlocks = worldData.GetAllBlocks();
            data.blocks = new SerializableBlock[allBlocks.Count];

            int index = 0;
            foreach (var block in allBlocks)
            {
                data.blocks[index] = new SerializableBlock
                {
                    x = block.Key.x,
                    y = block.Key.y,
                    type = block.Value.type.ToString(),
                    health = block.Value.health
                };
                index++;
            }
        }

        return data;
    }

    // 状态切换方法
    public void TogglePause() => SetState(CurrentState == GameState.Playing ? GameState.Paused : GameState.Playing);
    public void PauseGame() => SetState(GameState.Paused);
    public void ResumeGame() => SetState(GameState.Playing);
    public void GameOver() => SetState(GameState.GameOver);

    // 修改OpenMenu方法，在切换到菜单前保存游戏
    public void OpenMenu()
    {
        SaveGameProgress();
        SetState(GameState.Menu);
    }
}