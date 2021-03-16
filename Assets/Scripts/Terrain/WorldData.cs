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
        public static readonly BlockStructure AIR
            = new BlockStructure(BlockType.AIR, BlockState.TRANSPARENT);
        public static readonly BlockStructure GRASS_BLOCK
            = new BlockStructure(BlockType.GRASS_BLOCK, TextureTile.GRASS_BLOCK_TOP, TextureTile.GRASS_BLOCK_SIDE, TextureTile.DIRT, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure DIRT
            = new BlockStructure(BlockType.DIRT, TextureTile.DIRT, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure SAND
            = new BlockStructure(BlockType.SAND, TextureTile.SAND, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure STONE
            = new BlockStructure(BlockType.STONE, TextureTile.STONE, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure COBBLESTONE
            = new BlockStructure(BlockType.COBBLESTONE, TextureTile.COBBLESTONE, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure OBSIDIAN
            = new BlockStructure(BlockType.OBSIDIAN, TextureTile.OBSIDIAN, BlockShape.CUBE, BlockState.SOLID);

        public static readonly BlockStructure OAK_LOG
            = new BlockStructure(BlockType.OAK_LOG, TextureTile.OAK_LOG_TOP, TextureTile.OAK_LOG_SIDE, TextureTile.OAK_LOG_TOP, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure OAK_LEAVES
            = new BlockStructure(BlockType.OAK_LEAVES, TextureTile.OAK_LEAVES, BlockShape.CUBE, BlockState.TRANSPARENT);
        public static readonly BlockStructure OAK_PLANKS
            = new BlockStructure(BlockType.OAK_PLANKS, TextureTile.OAK_PLANKS, BlockShape.CUBE, BlockState.SOLID);

        public static readonly BlockStructure SPRUCE_LOG
            = new BlockStructure(BlockType.SPRUCE_LOG, TextureTile.SPRUCE_LOG_TOP, TextureTile.SPRUCE_LOG_SIDE, TextureTile.SPRUCE_LOG_TOP, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure SPRUCE_LEAVES
            = new BlockStructure(BlockType.SPRUCE_LEAVES, TextureTile.SPRUCE_LEAVES, BlockShape.CUBE, BlockState.TRANSPARENT);
        public static readonly BlockStructure SPRUCE_PLANKS
            = new BlockStructure(BlockType.SPRUCE_PLANKS, TextureTile.SPRUCE_PLANKS, BlockShape.CUBE, BlockState.SOLID);

        public static readonly BlockStructure BIRCH_LOG
            = new BlockStructure(BlockType.BIRCH_LOG, TextureTile.BIRCH_LOG_TOP, TextureTile.BIRCH_LOG_SIDE, TextureTile.BIRCH_LOG_TOP, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure BIRCH_LEAVES
            = new BlockStructure(BlockType.BIRCH_LEAVES, TextureTile.BIRCH_LEAVES, BlockShape.CUBE, BlockState.TRANSPARENT);
        public static readonly BlockStructure BIRCH_PLANKS
            = new BlockStructure(BlockType.BIRCH_PLANKS, TextureTile.BIRCH_PLANKS, BlockShape.CUBE, BlockState.SOLID);

        public static readonly BlockStructure JUNGLE_LOG
            = new BlockStructure(BlockType.JUNGLE_LOG, TextureTile.JUNGLE_LOG_TOP, TextureTile.JUNGLE_LOG_SIDE, TextureTile.JUNGLE_LOG_TOP, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure JUNGLE_LEAVES
            = new BlockStructure(BlockType.JUNGLE_LEAVES, TextureTile.JUNGLE_LEAVES, BlockShape.CUBE, BlockState.TRANSPARENT);
        public static readonly BlockStructure JUNGLE_PLANKS
            = new BlockStructure(BlockType.JUNGLE_PLANKS, TextureTile.JUNGLE_PLANKS, BlockShape.CUBE, BlockState.SOLID);

        public static readonly BlockStructure ICE
            = new BlockStructure(BlockType.ICE, TextureTile.ICE, BlockShape.CUBE, BlockState.SOLID);
        public static readonly BlockStructure SNOW
            = new BlockStructure(BlockType.SNOW, TextureTile.SNOW, BlockShape.CUBE, BlockState.SOLID);

        public static readonly BlockStructure HALF_SLAB_BLOCK
            = new BlockStructure(BlockType.HALF_SLAB, TextureTile.OAK_LOG_SIDE, BlockShape.HALF_BLOCK, BlockState.TRANSPARENT);

        // liquids
        public static readonly BlockStructure WATER_BLOCK
            = new BlockStructure(BlockType.WATER, TextureTile.WATER, BlockShape.LIQUID, BlockState.LIQUID);

        // plants
        public static readonly BlockStructure GRASS
            = new BlockStructure(BlockType.GRASS, TextureTile.GRASS_0, TextureTile.GRASS_1, TextureTile.GRASS_2, BlockShape.GRASS, BlockState.LIQUID_DESTROYABLE);

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

        public static BlockStructure GetBlockData(BlockType type)
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


                case BlockType.OAK_LOG:
                    return OAK_LOG;
                case BlockType.SPRUCE_LOG:
                    return SPRUCE_LOG;
                case BlockType.BIRCH_LOG:
                    return BIRCH_LOG;
                case BlockType.JUNGLE_LOG:
                    return JUNGLE_LOG;

                case BlockType.OAK_LEAVES:
                    return OAK_LEAVES;
                case BlockType.SPRUCE_LEAVES:
                    return SPRUCE_LEAVES;
                case BlockType.BIRCH_LEAVES:
                    return BIRCH_LEAVES;
                case BlockType.JUNGLE_LEAVES:
                    return JUNGLE_LEAVES;

                case BlockType.OAK_PLANKS:
                    return OAK_PLANKS;
                case BlockType.SPRUCE_PLANKS:
                    return SPRUCE_PLANKS;
                case BlockType.BIRCH_PLANKS:
                    return BIRCH_PLANKS;
                case BlockType.JUNGLE_PLANKS:
                    return JUNGLE_PLANKS;

                case BlockType.ICE:
                    return ICE;
                case BlockType.SNOW:
                    return SNOW;

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
                case BlockType.BIRCH_LEAVES:
                case BlockType.JUNGLE_LEAVES:
                    return BlockState.TRANSPARENT;
                case BlockType.GRASS:
                    return BlockState.LIQUID_DESTROYABLE;
                case BlockType.WATER:
                    return BlockState.LIQUID;
                default:
                    return BlockState.SOLID;
            }
        }

        public static BlockState GetBlockState(BlockType type, BlockFace face)
        {
            switch (type)
            {
                case BlockType.HALF_SLAB:
                    if (face == BlockFace.BOTTOM)
                        return BlockState.SOLID;
                    return BlockState.TRANSPARENT;
                default:
                    return GetBlockState(type);
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