using UnityEngine;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory.Tools
{
    public static class ToolUtils
    {
        public static float GetBlockDurability(BlockType blockType, ToolType toolType)
        {
            switch (toolType)
            {
                case ToolType.SWORD:
                    switch (blockType)
                    {
                        case BlockType.OAK_LEAVES:
                            return 0.2f;
                    }
                    break;
                case ToolType.PICKAXE:
                    switch (blockType)
                    {
                        case BlockType.STONE:
                        case BlockType.COBBLESTONE:
                            return 0.3f;
                        default:
                            return 0.3f;
                    }
                    break;
                case ToolType.AXE:
                    switch (blockType)
                    {
                        case BlockType.OAK_LOG:
                            return 0.3f;
                    }
                    break;
                case ToolType.MATERIAL:
                    break;
            }
            return 1;
        }
    }
}
