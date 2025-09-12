using UnityEngine;
using System.Collections.Generic;

public class WorldData : MonoBehaviour
{
    // 世界尺寸（网格范围），例如1000x500格
    public Vector2Int worldSize = new Vector2Int(10000, 5000);
    // 存储所有方块：key=网格坐标，value=方块数据
    private Dictionary<Vector2Int, BlockData> allBlocks = new Dictionary<Vector2Int, BlockData>();

    [Header("方块预制体设置")]
    public GameObject[] blockPrefabs; // 方块预制体数组，需要在Inspector中设置

    // 初始化世界（生成初始地形，如地面、山脉）
    private void Start()
    {
        // 检查预制体是否已设置
        if (blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogError("方块预制体未设置！请在Inspector中设置blockPrefabs数组");
            return;
        }

        GenerateInitialTerrain();
    }

    // 生成简单初始地形（示例：底部生成泥土，上方叠石头）
    private void GenerateInitialTerrain()
    {
        for (int x = -50; x < Mathf.Min(worldSize.x, 100); x++) // 限制初始生成范围避免卡顿
        {
            // 底部3格为泥土
            for (int y = -6; y < -3; y++)
            {
                AddBlock(BlockType.Dirt, new Vector2Int(x, y));
            }
            // 泥土上方2格为石头
            // for (int y = -3; y < -2; y++)
            // {
            //     AddBlock(BlockType.Stone, new Vector2Int(x, y));
            // }
        }
        Debug.Log("初始地形生成完成，方块总数：" + allBlocks.Count);
    }

    // 核心方法：添加方块到世界
    public void AddBlock(BlockType type, Vector2Int gridPos)
    {
        // 检查位置是否在世界范围内
        if (!IsPositionInWorld(gridPos))
        {
            Debug.LogWarning($"位置 {gridPos} 超出世界范围");
            return;
        }

        // 检查位置是否已有方块
        if (allBlocks.ContainsKey(gridPos))
        {
            Debug.LogWarning($"位置 {gridPos} 已存在方块");
            return;
        }

        // 创建方块数据并添加到字典
        BlockData newBlock = new BlockData(type, gridPos);
        allBlocks.Add(gridPos, newBlock);

        // 生成方块的可视化模型
        SpawnBlockVisual(type, gridPos);

        Debug.Log($"成功添加方块 {type} 到位置 {gridPos}");
    }

    // 核心方法：从世界移除方块
    public void RemoveBlock(Vector2Int gridPos)
    {
        if (allBlocks.TryGetValue(gridPos, out BlockData block))
        {
            allBlocks.Remove(gridPos);
            // 销毁方块的可视化模型
            DestroyBlockVisual(gridPos, block.type);
            Debug.Log($"移除方块 {block.type} 从位置 {gridPos}");
        }
        else
        {
            Debug.LogWarning($"位置 {gridPos} 没有方块可移除");
        }
    }

    // 辅助方法：检查位置是否在世界范围内
    public bool IsPositionInWorld(Vector2Int gridPos)
    {
        return true;
        // return gridPos.x >= 0 && gridPos.x < worldSize.x && gridPos.y >= 0 && gridPos.y < worldSize.y;
    }

    // 检查指定位置是否有方块
    public bool HasBlockAt(Vector2Int gridPos)
    {
        return allBlocks.ContainsKey(gridPos);
    }

    // 获取指定位置的方块数据
    public BlockData GetBlockAt(Vector2Int gridPos)
    {
        return allBlocks.TryGetValue(gridPos, out BlockData block) ? block : null;
    }

    // 世界坐标转网格坐标
    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    // 网格坐标转世界坐标
    public static Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0);
    }

    // 生成方块模型
    private void SpawnBlockVisual(BlockType type, Vector2Int gridPos)
    {
        // 网格坐标转世界坐标（每个方块1x1单位，中心点偏移0.5）
        Vector3 worldPos = GridToWorld(gridPos);

        // 根据方块类型选择对应的预制体
        GameObject blockPrefab = GetPrefabForBlockType(type);

        if (blockPrefab != null)
        {
            GameObject blockObj = Instantiate(blockPrefab, worldPos, Quaternion.identity, transform);
            blockObj.name = $"{type}_{gridPos.x}_{gridPos.y}"; // 命名格式：类型_x_y

            // 确保方块在Block层上
            blockObj.layer = LayerMask.NameToLayer("Block");
            if (blockObj.layer == -1)
            {
                Debug.LogWarning("Block层不存在，请在Unity中创建Block层");
            }
        }
        else
        {
            Debug.LogError($"未找到方块类型 {type} 的预制体");
        }
    }

    // 根据方块类型获取对应的预制体
    private GameObject GetPrefabForBlockType(BlockType type)
    {
        // 这里可以根据需要实现更复杂的逻辑
        // 现在简单地根据枚举索引来匹配预制体数组
        int typeIndex = (int)type;
        if (typeIndex >= 0 && typeIndex < blockPrefabs.Length && blockPrefabs[typeIndex] != null)
        {
            return blockPrefabs[typeIndex];
        }

        // 如果没有找到对应的预制体，尝试从Resources文件夹加载
        GameObject prefab = Resources.Load<GameObject>($"Blocks/{type}");
        if (prefab != null)
        {
            return prefab;
        }

        Debug.LogError($"未找到方块类型 {type} 的预制体，请在blockPrefabs数组中设置或放置在Resources/Blocks/文件夹中");
        return null;
    }

    // 销毁方块模型
    private void DestroyBlockVisual(Vector2Int gridPos, BlockType type)
    {
        // 查找并销毁对应位置的方块模型
        string blockName = $"{type}_{gridPos.x}_{gridPos.y}";
        Transform blockTransform = transform.Find(blockName);

        if (blockTransform != null)
        {
            Destroy(blockTransform.gameObject);
        }
        else
        {
            // 如果在当前WorldData下没找到，尝试全局搜索
            GameObject blockObj = GameObject.Find(blockName);
            if (blockObj != null)
            {
                Destroy(blockObj);
            }
            else
            {
                Debug.LogWarning($"未找到要销毁的方块模型: {blockName}");
            }
        }
    }

    // 对方块造成伤害
    public void DamageBlock(Vector2Int gridPos, Tool tool)
    {
        if (!allBlocks.TryGetValue(gridPos, out BlockData block))
        {
            Debug.LogWarning($"位置 {gridPos} 没有方块可破坏");
            return;
        }

        if (tool == null)
        {
            Debug.LogWarning("工具为空，无法破坏方块");
            return;
        }

        // 检查工具是否能破坏该方块
        if (tool.breakableBlocks != null && tool.breakableBlocks.Contains(block.type))
        {
            block.health -= tool.damage;
            Debug.Log($"{block.type} 受到 {tool.damage} 伤害，剩余生命值：{block.health}");

            // 生命值≤0时，破坏方块并掉落资源
            if (block.health <= 0)
            {
                DropResource(block.type, gridPos);
                RemoveBlock(gridPos);
            }
        }
        else
        {
            Debug.Log($"{tool.toolName} 无法破坏 {block.type}");
        }
    }

    // 掉落资源
    private void DropResource(BlockType blockType, Vector2Int gridPos)
    {
        // 方块类型→资源类型的映射
        ItemType dropItem = blockType switch
        {
            BlockType.Dirt => ItemType.DirtClump,
            BlockType.Stone => ItemType.StoneChunk,
            BlockType.Wood => ItemType.WoodLog,
            BlockType.Sand => ItemType.SandGrain,
            BlockType.Glass => ItemType.GlassPane,
            BlockType.Torch => ItemType.TorchItem,
            BlockType.Painting => ItemType.PaintingItem,
            _ => ItemType.DirtClump
        };

        // 通知玩家背包添加资源
        PlayerScriptTest player = FindObjectOfType<PlayerScriptTest>();
        if (player != null)
        {
            Inventory inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                inventory.AddItem(dropItem, 1);
                Debug.Log($"破坏 {blockType}，获得 {dropItem}");
            }
            else
            {
                Debug.LogError("玩家没有Inventory组件");
            }
        }
        else
        {
            Debug.LogError("未找到玩家对象");
        }

        // （可选）生成资源拾取动画（在世界中显示漂浮的资源图标）
        // Vector3 worldPos = GridToWorld(gridPos);
        // Instantiate(resourceDropEffectPrefab, worldPos, Quaternion.identity);
    }

    // 调试用：在Scene视图中显示世界边界
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(worldSize.x * 0.5f, worldSize.y * 0.5f, 0);
        Vector3 size = new Vector3(worldSize.x, worldSize.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }

    public Dictionary<Vector2Int, BlockData> GetAllBlocks()
    {
        return allBlocks; // 返回存储所有方块的字典
    }
    // 清除所有方块（用于加载存档前清理）
    public void ClearAllBlocks()
    {
        // 销毁所有方块的可视化模型
        foreach (var block in allBlocks)
        {
            DestroyBlockVisual(block.Key, block.Value.type);
        }

        // 清空方块数据
        allBlocks.Clear();
        Debug.Log("所有方块已清除");
    }

}