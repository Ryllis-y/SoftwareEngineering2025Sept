// Inventory.cs：玩家背包
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
public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    // 添加资源
    public void AddItem(ItemType type, int amount)
    {
        // 查找现有资源，叠加数量
        Item existingItem = items.Find(i => i.type == type);
        if (existingItem != null)
        {
            existingItem.amount += amount;
        }
        else
        {
            items.Add(new Item { type = type, amount = amount });
        }
        Debug.Log($"获得 {amount} 个 {type}，当前总数：{GetItemAmount(type)}");
    }

    // 检查资源数量
    public int GetItemAmount(ItemType type)
    {
        Item item = items.Find(i => i.type == type);
        return item != null ? item.amount : 0;
    }
    public void RemoveItem(ItemType type, int amount)
    {
        Item existingItem = items.Find(i => i.type == type);
        if (existingItem != null)
        {
            existingItem.amount -= amount;
            if (existingItem.amount <= 0)
            {
                items.Remove(existingItem); // 数量为0时移除
            }
        }
    }
}