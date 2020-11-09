using VoxelTG.Player.Inventory;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public class WorldData
    {
        #region // === Static === \\

        #region // === Blocks === \\

        // TODO: add sound types
        public static readonly Block AIR
            = new Block(BlockType.AIR, BlockState.TRANSPARENT);
        public static readonly Block GRASS_BLOCK
            = new Block(BlockType.GRASS_BLOCK, TextureTile.GRASS_BLOCK_TOP, TextureTile.GRASS_BLOCK_SIDE, TextureTile.DIRT, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block DIRT
            = new Block(BlockType.DIRT, TextureTile.DIRT, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block SAND
            = new Block(BlockType.SAND, TextureTile.SAND, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block STONE
            = new Block(BlockType.STONE, TextureTile.STONE, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block COBBLESTONE
            = new Block(BlockType.COBBLESTONE, TextureTile.COBBLESTONE, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block OBSIDIAN
            = new Block(BlockType.OBSIDIAN, TextureTile.OBSIDIAN, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block OAK_LOG
            = new Block(BlockType.OAK_LOG, TextureTile.OAK_LOG_TOP, TextureTile.OAK_LOG_SIDE, TextureTile.OAK_LOG_TOP, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block OAK_LEAVES
            = new Block(BlockType.OAK_LEAVES, TextureTile.OAK_LEAVES, BlockShape.CUBE, BlockState.TRANSPARENT);
        public static readonly Block SPRUCE_LOG
            = new Block(BlockType.SPRUCE_LOG, TextureTile.SPRUCE_LOG_TOP, TextureTile.SPRUCE_LOG_SIDE, TextureTile.SPRUCE_LOG_TOP, BlockShape.CUBE, BlockState.SOLID);
        public static readonly Block SPRUCE_LEAVES
            = new Block(BlockType.SPRUCE_LEAVES, TextureTile.SPRUCE_LEAVES, BlockShape.CUBE, BlockState.TRANSPARENT);
        public static readonly Block HALF_SLAB_BLOCK
            = new Block(BlockType.HALF_SLAB, TextureTile.OAK_LOG_SIDE, BlockShape.HALF_BLOCK, BlockState.TRANSPARENT);

        // liquids
        public static readonly Block WATER_BLOCK
            = new Block(BlockType.WATER, TextureTile.WATER, BlockShape.LIQUID, BlockState.LIQUID);

        // plants
        public static readonly Block GRASS
            = new Block(BlockType.GRASS, TextureTile.GRASS_0, TextureTile.GRASS_1, TextureTile.GRASS_2, BlockShape.GRASS, BlockState.PLANTS);

        #endregion

        public static float GetBlockDurability(BlockType type)
        {
            switch (type)
            {
                case BlockType.OBSIDIAN:
                    return 2;
                default:
                    return 1;
            }
        }

        public static Block GetBlockData(BlockType type)
        {
            switch (type)
            {
                case BlockType.DIRT:
                    return DIRT;
                case BlockType.GRASS_BLOCK:
                    return GRASS_BLOCK;
                case BlockType.SAND:
                    return SAND;

                case BlockType.STONE:
                    return STONE;
                case BlockType.COBBLESTONE:
                    return COBBLESTONE;
                case BlockType.OBSIDIAN:
                    return OBSIDIAN;

                // oak
                case BlockType.OAK_LOG:
                    return OAK_LOG;
                case BlockType.OAK_LEAVES:
                    return OAK_LEAVES;
                // spruce
                case BlockType.SPRUCE_LOG:
                    return SPRUCE_LOG;
                case BlockType.SPRUCE_LEAVES:
                    return SPRUCE_LEAVES;

                case BlockType.HALF_SLAB:
                    return HALF_SLAB_BLOCK;
                // liquids
                case BlockType.WATER:
                    return WATER_BLOCK;
                // plants
                case BlockType.GRASS:
                    return GRASS;
                default:
                    return AIR;
            }
        }

        public static BlockState GetBlockState(BlockType type)
        {
            switch (type)
            {
                case BlockType.AIR:
                case BlockType.OAK_LEAVES:
                case BlockType.SPRUCE_LEAVES:
                    return BlockState.TRANSPARENT;
                case BlockType.GRASS:
                    return BlockState.PLANTS;
                case BlockType.WATER:
                    return BlockState.LIQUID;
                default:
                    return BlockState.SOLID;
            }
        }

        public static void GetCustomBlockDrops(BlockType block, ref ItemType itemType, ref BlockType blockType, ref int count)
        {
            switch (block)
            {
                case BlockType.GRASS_BLOCK:
                    itemType = ItemType.MATERIAL;
                    blockType = BlockType.DIRT;
                    break;
            }
        }

        public static BlockState GetBlockState(BlockType type, BlockFace face)
        {
            switch (type)
            {
                case BlockType.AIR:
                case BlockType.OAK_LEAVES:
                    return BlockState.TRANSPARENT;
                case BlockType.GRASS:
                    return BlockState.PLANTS;
                case BlockType.HALF_SLAB:
                    if (face == BlockFace.BOTTOM)
                        return BlockState.SOLID;
                    return BlockState.TRANSPARENT;
                case BlockType.WATER:
                    return BlockState.LIQUID;
                default:
                    return BlockState.SOLID;
            }
        }

        #region // === Booleans === \\

        /// <summary>
        /// Can tree be placed on provided BlockType
        /// </summary>
        public static bool CanPlaceTree(BlockType type)
        {
            switch (type)
            {
                case BlockType.DIRT:
                case BlockType.GRASS_BLOCK:
                case BlockType.GRASS:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Can grass be placed on provided BlockType
        /// </summary>
        public static bool CanPlaceGrass(BlockType type)
        {
            switch (type)
            {
                case BlockType.DIRT:
                case BlockType.GRASS_BLOCK:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #endregion
    }
}