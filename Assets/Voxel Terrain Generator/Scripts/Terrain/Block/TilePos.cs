using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct TilePos
{
    public float2 uv0, uv1, uv2, uv3;

    private static readonly float textureSize = 16f;

    public static readonly TilePos AIR_UV = new TilePos
    {
        uv0 = float2.zero,
        uv1 = float2.zero,
        uv2 = float2.zero,
        uv3 = float2.zero
    };

    public static readonly TilePos DIRT_UV = new TilePos
    {
        uv0 = new float2(
            0 / textureSize + .001f,
            0 / textureSize + .001f),
        uv1 = new float2(
            0 / textureSize + .001f,
            (0 + 1) / textureSize - .001f),
        uv2 = new float2(
            (0 + 1) / textureSize - .001f,
            (0 + 1) / textureSize - .001f),
        uv3 = new float2(
            (0 + 1) / textureSize - .001f,
            0 / textureSize + .001f),
    };
    public static readonly TilePos GRASS_BLOCK_TOP_UV = new TilePos
    {
        uv0 = new float2(
            1 / textureSize + .001f,
            0 / textureSize + .001f),
        uv1 = new float2(
            1 / textureSize + .001f,
            (0 + 1) / textureSize - .001f),
        uv2 = new float2(
            (1 + 1) / textureSize - .001f,
            (0 + 1) / textureSize - .001f),
        uv3 = new float2(
            (1 + 1) / textureSize - .001f,
            0 / textureSize + .001f),
    };
    public static readonly TilePos GRASS_BLOCK_SIDE_UV = new TilePos
    {
        uv0 = new float2(
            0 / textureSize + .001f,
            1 / textureSize + .001f),
        uv1 = new float2(
            0 / textureSize + .001f,
            (1 + 1) / textureSize - .001f),
        uv2 = new float2(
            (0 + 1) / textureSize - .001f,
            (1 + 1) / textureSize - .001f),
        uv3 = new float2(
            (0 + 1) / textureSize - .001f,
            1 / textureSize + .001f),
    };
    public static readonly TilePos STONE_UV = new TilePos
    {
        uv0 = new float2(
            0 / textureSize + .001f,
            2 / textureSize + .001f),
        uv1 = new float2(
            0 / textureSize + .001f,
            (2 + 1) / textureSize - .001f),
        uv2 = new float2(
            (0 + 1) / textureSize - .001f,
            (2 + 1) / textureSize - .001f),
        uv3 = new float2(
            (0 + 1) / textureSize - .001f,
            2 / textureSize + .001f),
    };
    public static readonly TilePos COBBLESTONE_UV = new TilePos
    {
        uv0 = new float2(
            1 / textureSize + .001f,
            2 / textureSize + .001f),
        uv1 = new float2(
            1 / textureSize + .001f,
            (2 + 1) / textureSize - .001f),
        uv2 = new float2(
            (1 + 1) / textureSize - .001f,
            (2 + 1) / textureSize - .001f),
        uv3 = new float2(
            (1 + 1) / textureSize - .001f,
            2 / textureSize + .001f),
    };
    public static readonly TilePos OBSIDIAN_UV = new TilePos
    {
        uv0 = new float2(
            2 / textureSize + .001f,
            2 / textureSize + .001f),
        uv1 = new float2(
            2 / textureSize + .001f,
            (2 + 1) / textureSize - .001f),
        uv2 = new float2(
            (2 + 1) / textureSize - .001f,
            (2 + 1) / textureSize - .001f),
        uv3 = new float2(
            (2 + 1) / textureSize - .001f,
            2 / textureSize + .001f),
    };
    public static readonly TilePos OAK_LOG_TOP_UV = new TilePos
    {
        uv0 = new float2(
            0 / textureSize + .001f,
            4 / textureSize + .001f),
        uv1 = new float2(
            0 / textureSize + .001f,
            (4 + 1) / textureSize - .001f),
        uv2 = new float2(
            (0 + 1) / textureSize - .001f,
            (4 + 1) / textureSize - .001f),
        uv3 = new float2(
            (0 + 1) / textureSize - .001f,
            4 / textureSize + .001f),
    };
    public static readonly TilePos OAK_LOG_SIDE_UV = new TilePos
    {
        uv0 = new float2(
            0 / textureSize + .001f,
            3 / textureSize + .001f),
        uv1 = new float2(
            0 / textureSize + .001f,
            (3 + 1) / textureSize - .001f),
        uv2 = new float2(
            (0 + 1) / textureSize - .001f,
            (3 + 1) / textureSize - .001f),
        uv3 = new float2(
            (0 + 1) / textureSize - .001f,
            3 / textureSize + .001f),
    };
    public static readonly TilePos LEAVES_UV = new TilePos
    {
        uv0 = new float2(
            0 / textureSize + .001f,
            5 / textureSize + .001f),
        uv1 = new float2(
            0 / textureSize + .001f,
            (5 + 1) / textureSize - .001f),
        uv2 = new float2(
            (0 + 1) / textureSize - .001f,
            (5 + 1) / textureSize - .001f),
        uv3 = new float2(
            (0 + 1) / textureSize - .001f,
            5 / textureSize + .001f),
    };

    #region liquids uvs

    public static readonly TilePos WATER_UV = new TilePos
    {
        uv0 = new float2(
            1 / textureSize + .001f,
            1 / textureSize + .001f),
        uv1 = new float2(
            1 / textureSize + .001f,
            (1 + 1) / textureSize - .001f),
        uv2 = new float2(
            (1 + 1) / textureSize - .001f,
            (1 + 1) / textureSize - .001f),
        uv3 = new float2(
            (1 + 1) / textureSize - .001f,
            1 / textureSize + .001f),
    };

    #endregion

    #region plants uvs

    public static readonly TilePos GRASS_UV_0 = new TilePos
    {
        uv0 = new float2(
            3 / textureSize + .001f,
            0 / textureSize + .001f),
        uv1 = new float2(
            3 / textureSize + .001f,
            (0 + 1) / textureSize - .001f),
        uv2 = new float2(
            (3 + 1) / textureSize - .001f,
            (0 + 1) / textureSize - .001f),
        uv3 = new float2(
            (3 + 1) / textureSize - .001f,
            0 / textureSize + .001f),
    };

    public static readonly TilePos GRASS_UV_1 = new TilePos
    {
        uv0 = new float2(
            3 / textureSize + .001f,
            2 / textureSize + .001f),
        uv1 = new float2(
            3 / textureSize + .001f,
            (2 + 1) / textureSize - .001f),
        uv2 = new float2(
            (3 + 1) / textureSize - .001f,
            (2 + 1) / textureSize - .001f),
        uv3 = new float2(
            (3 + 1) / textureSize - .001f,
            2 / textureSize + .001f),
    };

    public static readonly TilePos GRASS_UV_2 = new TilePos
    {
        uv0 = new float2(
            3 / textureSize + .001f,
            3 / textureSize + .001f),
        uv1 = new float2(
            3 / textureSize + .001f,
            (3 + 1) / textureSize - .001f),
        uv2 = new float2(
            (3 + 1) / textureSize - .001f,
            (3 + 1) / textureSize - .001f),
        uv3 = new float2(
            (3 + 1) / textureSize - .001f,
            3 / textureSize + .001f),
    };

    #endregion

    public static TilePos GetTilePos(Tile tile)
    {
        switch (tile)
        {
            case Tile.DIRT:
                return DIRT_UV;
            case Tile.GRASS_BLOCK_TOP:
                return GRASS_BLOCK_TOP_UV;
            case Tile.GRASS_BLOCK_SIDE:
                return GRASS_BLOCK_SIDE_UV;
            case Tile.STONE:
                return STONE_UV;
            case Tile.COBBLESTONE:
                return COBBLESTONE_UV;
            case Tile.OBSIDIAN:
                return OBSIDIAN_UV;
            case Tile.OAK_LOG_SIDE:
                return OAK_LOG_SIDE_UV;
            case Tile.OAK_LOG_TOP:
                return OAK_LOG_TOP_UV;
            case Tile.OAK_LEAVES:
                return LEAVES_UV;
            case Tile.WATER:
                return WATER_UV;
            case Tile.GRASS_0:
                return GRASS_UV_0;
            case Tile.GRASS_1:
                return GRASS_UV_1;
            case Tile.GRASS_2:
                return GRASS_UV_2;
            default:
                return DIRT_UV;
        }
    }
}

public enum Tile { AIR, DIRT, GRASS_BLOCK_TOP, GRASS_BLOCK_SIDE, STONE, COBBLESTONE, OBSIDIAN, OAK_LOG_SIDE, OAK_LOG_TOP, OAK_LEAVES, WATER, GRASS_0, GRASS_1, GRASS_2 }
