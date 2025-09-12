// Item.cs：资源数据
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using System;
[System.Serializable]
public enum ItemType
{
    // 基础资源
    DirtClump, StoneChunk, WoodLog, SandGrain,
    // 工具
    WoodenPickaxe, StonePickaxe, IronPickaxe,
    WoodenAxe, StoneAxe, IronAxe,
    WoodenShovel, StoneShovel, IronShovel,
    // 其他物品
    GlassPane, TorchItem, PaintingItem
}

[System.Serializable]
public class Item
{
    public ItemType type;
    public int amount;
}

