using UnityEngine;

[System.Serializable] // 允许在Inspector中显示
public class BlockData
{
    public BlockType type; // 方块类型
    public Vector2Int gridPosition; // 网格坐标（x,y），比世界坐标更精准
    public float health; // 方块生命值（0=被破坏）
    public bool isActive; // 用于功能方块（如门的开关状态）

    // 构造函数：创建方块时初始化属性
    public BlockData(BlockType type, Vector2Int gridPos)
    {
        this.type = type;
        this.gridPosition = gridPos;
        this.health = GetMaxHealthByType(type); // 根据类型设置初始生命值
        this.isActive = true;
    }

    // 根据方块类型获取最大生命值（硬度）
    private int GetMaxHealthByType(BlockType type)
    {
        switch (type)
        {
            case BlockType.Dirt: return 10; // 软土，易破坏
            case BlockType.Stone: return 30; // 石头，较硬
            case BlockType.Wood: return 20; // 木材，中等硬度
            case BlockType.Workbench: return 50; // 功能方块更坚固
            default: return 10;
        }
    }
}