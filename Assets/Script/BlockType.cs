// 方块类型（自然/功能/装饰）
public enum BlockType
{
    // 自然方块（可破坏，掉落资源）
    Dirt, Stone, Wood, Sand,
    // 功能方块（可交互，如制作/合成）
    Workbench, Furnace, Door,
    // 装饰方块（纯美观，不可破坏或低硬度）
    Glass, Torch, Painting
}