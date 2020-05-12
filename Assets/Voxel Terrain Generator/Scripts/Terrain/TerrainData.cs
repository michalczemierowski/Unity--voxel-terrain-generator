using Unity.Collections;
using UnityEngine;

public class TerrainData
{
    public const int possibleBiomes = 2;
    #region // === Static === \\

    #region // === Blocks === \\

    public static readonly Block AIR = new Block(BlockType.AIR, BlockState.TRANSPARENT);
    public static readonly Block GRASS_BLOCK = new Block(BlockType.GRASS_BLOCK, Tile.GRASS_BLOCK_TOP, Tile.GRASS_BLOCK_SIDE, Tile.DIRT, BlockShape.CUBE, BlockState.SOLID);
    public static readonly Block DIRT = new Block(BlockType.DIRT, Tile.DIRT, BlockShape.CUBE, BlockState.SOLID);
    public static readonly Block STONE = new Block(BlockType.STONE, Tile.STONE, BlockShape.CUBE, BlockState.SOLID);
    public static readonly Block COBBLESTONE = new Block(BlockType.COBBLESTONE, Tile.COBBLESTONE, BlockShape.CUBE, BlockState.SOLID);
    public static readonly Block OBSIDIAN = new Block(BlockType.OBSIDIAN, Tile.OBSIDIAN, BlockShape.CUBE, BlockState.SOLID);
    public static readonly Block OAK_LOG = new Block(BlockType.OAK_LOG, Tile.OAK_LOG_TOP, Tile.OAK_LOG_SIDE, Tile.OAK_LOG_TOP, BlockShape.CUBE, BlockState.SOLID);
    public static readonly Block OAK_LEAVES = new Block(BlockType.OAK_LEAVES, Tile.OAK_LEAVES, BlockShape.CUBE, BlockState.TRANSPARENT);
    public static readonly Block HALF_SLAB_BLOCK = new Block(BlockType.HALF_SLAB, Tile.OAK_LOG_SIDE, BlockShape.HALF_BLOCK, BlockState.TRANSPARENT);

    // liquids
    public static readonly Block WATER_BLOCK = new Block(BlockType.WATER, Tile.WATER, BlockShape.LIQUID, BlockState.LIQUID);

    // plants
    public static readonly Block GRASS = new Block(BlockType.GRASS, Tile.GRASS_0, Tile.GRASS_1, Tile.GRASS_2, BlockShape.GRASS, BlockState.PLANTS);

    #endregion

    public static Block GetBlock(BlockType type)
    {
        switch (type)
        {
            case BlockType.DIRT:
                return DIRT;
            case BlockType.GRASS_BLOCK:
                return GRASS_BLOCK;
            case BlockType.STONE:
                return STONE;
            case BlockType.COBBLESTONE:
                return COBBLESTONE;
            case BlockType.OBSIDIAN:
                return OBSIDIAN;
            case BlockType.OAK_LOG:
                return OAK_LOG;
            case BlockType.OAK_LEAVES:
                return OAK_LEAVES;
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
                return BlockState.TRANSPARENT;
            case BlockType.GRASS:
                return BlockState.PLANTS;
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
public enum BlockType : byte { AIR, DIRT, GRASS_BLOCK, STONE, COBBLESTONE, OBSIDIAN, OAK_LOG, OAK_LEAVES, WATER, HALF_SLAB, GRASS }
public enum BlockState : byte { LIQUID, SOLID, TRANSPARENT, PLANTS }
public enum BlockFace : byte { TOP, BOTTOM, FRONT, BACK, LEFT, RIGHT }
public enum BlockShape : byte { CUBE, LIQUID, GRASS, HALF_BLOCK }

public enum BiomeType { FOREST, STONE_TEST, HIGH_TEST }