using System.Collections.Generic;
using System;
[System.Serializable]
public class Tool
{
    public string toolName; // 工具名称（如“木镐”“石斧”）
    public int damage; // 每次攻击对木块的伤害
    public List<BlockType> breakableBlocks; // 可破坏的方块类型（如斧头→木材，镐子→石头）
}