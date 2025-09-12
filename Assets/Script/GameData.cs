using System;
using UnityEngine;

[Serializable]
public class GameData
{
    // 玩家数据
    public float playerX;
    public float playerY;
    public float playerHealth;

    // 游戏状态
    public string currentScene;
    public float gameTime;

    // 物品数据
    public int dirtCount;
    public int stoneCount;
    public int woodCount;
    public int sandCount;
    public int glassCount;

    // 方块数据
    public SerializableBlock[] blocks;
}

[Serializable]
public class SerializableBlock
{
    public int x;
    public int y;
    public string type;
    public float health;
}
