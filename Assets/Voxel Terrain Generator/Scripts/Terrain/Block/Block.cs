using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct Block
{
    #region // === Variables === \\

    public BlockType type;
    public BlockShape shape;
    public BlockState state;
    public Tile top, side, bottom;
    public TilePos topPos, sidePos, bottomPos;

    #endregion

    public Block(BlockType type, BlockState state)
    {
        this.type = type;
        this.state = state;
        this.shape = BlockShape.CUBE;
        topPos = sidePos = bottomPos = TilePos.AIR_UV;
        top = side = bottom = Tile.AIR;
    }
    public Block(BlockType type, Tile tile, BlockShape shape, BlockState state)
    {
        this.type = type;
        this.shape = shape;
        this.state = state;
        top = side = bottom = tile;
        topPos = TilePos.GetTilePos(tile);
        sidePos = TilePos.GetTilePos(tile);
        bottomPos = TilePos.GetTilePos(tile);
    }
    public Block(BlockType type, Tile top, Tile side, Tile bottom, BlockShape shape, BlockState state)
    {
        this.type = type;
        this.shape = shape;
        this.state = state;
        this.top = top;
        this.side = side;
        this.bottom = bottom;
        topPos = TilePos.GetTilePos(top);
        sidePos = TilePos.GetTilePos(side);
        bottomPos = TilePos.GetTilePos(bottom);
    }

    #region // === Shape === \\

    public bool GetBlockShape(BlockFace face, int3 blockPos, NativeArray<float3> verts, NativeArray<float2> uv, short param)
    {
        switch (shape)
        {
            case BlockShape.CUBE:
                switch (face)
                {
                    case BlockFace.TOP:
                        verts[0] = new float3(blockPos.x, blockPos.y + 1, blockPos.z);
                        verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z);

                        uv[0] = topPos.uv0;
                        uv[1] = topPos.uv1;
                        uv[2] = topPos.uv2;
                        uv[3] = topPos.uv3;
                        return false;
                    case BlockFace.BOTTOM:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                        uv[0] = bottomPos.uv0;
                        uv[1] = bottomPos.uv1;
                        uv[2] = bottomPos.uv2;
                        uv[3] = bottomPos.uv3;
                        return false;
                    case BlockFace.FRONT:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);

                        uv[0] = sidePos.uv0;
                        uv[1] = sidePos.uv1;
                        uv[2] = sidePos.uv2;
                        uv[3] = sidePos.uv3;
                        return false;
                    case BlockFace.BACK:
                        verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                        uv[0] = sidePos.uv0;
                        uv[1] = sidePos.uv1;
                        uv[2] = sidePos.uv2;
                        uv[3] = sidePos.uv3;
                        return false;
                    case BlockFace.RIGHT:
                        verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                        uv[0] = sidePos.uv0;
                        uv[1] = sidePos.uv1;
                        uv[2] = sidePos.uv2;
                        uv[3] = sidePos.uv3;
                        return false;
                    case BlockFace.LEFT:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);
                        verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z);

                        uv[0] = sidePos.uv0;
                        uv[1] = sidePos.uv1;
                        uv[2] = sidePos.uv2;
                        uv[3] = sidePos.uv3;
                        return false;
                }
                break;
            case BlockShape.GRASS:
                switch (face)
                {
                    case BlockFace.FRONT:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                        GetGrassUVS(uv, param);
                        return false;
                    case BlockFace.BACK:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                        GetGrassUVS(uv, param);
                        return true;
                    case BlockFace.RIGHT:
                        verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z);
                        verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                        GetGrassUVS(uv, param);
                        return false;
                    case BlockFace.LEFT:
                        verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z);
                        verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                        GetGrassUVS(uv, param);
                        return true;
                }
                break;
            case BlockShape.HALF_BLOCK:
                switch (face)
                {
                    case BlockFace.TOP:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);

                        if (param != 0)
                            Rotate(verts, blockPos, param);

                        uv[0] = topPos.uv0;
                        uv[1] = topPos.uv1;
                        uv[2] = topPos.uv2;
                        uv[3] = topPos.uv3;
                        return false;
                    case BlockFace.BOTTOM:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                        if (param != 0)
                            Rotate(verts, blockPos, param);

                        uv[0] = bottomPos.uv0;
                        uv[1] = bottomPos.uv1;
                        uv[2] = bottomPos.uv2;
                        uv[3] = bottomPos.uv3;
                        return false;
                    case BlockFace.FRONT:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);

                        if (param != 0)
                            Rotate(verts, blockPos, param);

                        uv[0] = topPos.uv0;
                        uv[1] = topPos.uv1;
                        uv[2] = topPos.uv2;
                        uv[3] = topPos.uv3;
                        return false;
                    case BlockFace.BACK:
                        verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                        if (param != 0)
                            Rotate(verts, blockPos, param);

                        uv[0] = sidePos.uv0;
                        uv[1] = sidePos.uv1;
                        uv[2] = sidePos.uv2;
                        uv[3] = sidePos.uv3;
                        return false;
                    case BlockFace.RIGHT:
                        verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                        verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                        if (param != 0)
                            Rotate(verts, blockPos, param);

                        uv[0] = sidePos.uv0;
                        uv[1] = sidePos.uv1;
                        uv[2] = sidePos.uv2;
                        uv[3] = sidePos.uv3;
                        return false;
                    case BlockFace.LEFT:
                        verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[1] = new float3(blockPos.x, blockPos.y, blockPos.z);
                        verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                        verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                        if (param != 0)
                            Rotate(verts, blockPos, param);

                        uv[0] = sidePos.uv0;
                        uv[1] = sidePos.uv1;
                        uv[2] = sidePos.uv2;
                        uv[3] = sidePos.uv3;
                        return true;
                }
                break;
        }

        return false;
    }
    public void GetWaterShape(BlockFace face, int3 blockPos, NativeArray<float3> verts, NativeArray<float2> uv, short sourceDistance)
    {
        float height = (float)sourceDistance / 8;
        switch (face)
        {
            case BlockFace.TOP:
                verts[0] = new float3(blockPos.x, blockPos.y + height, blockPos.z);
                verts[1] = new float3(blockPos.x, blockPos.y + height, blockPos.z + 1);
                verts[2] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z + 1);
                verts[3] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z);

                uv[0] = topPos.uv0;
                uv[1] = topPos.uv1;
                uv[2] = topPos.uv2;
                uv[3] = topPos.uv3;
                return;
            case BlockFace.BOTTOM:
                verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                verts[2] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                uv[0] = bottomPos.uv0;
                uv[1] = bottomPos.uv1;
                uv[2] = bottomPos.uv2;
                uv[3] = bottomPos.uv3;
                return;
            case BlockFace.FRONT:
                verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                verts[1] = new float3(blockPos.x, blockPos.y + height, blockPos.z);
                verts[2] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z);
                verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);

                uv[0] = sidePos.uv0;
                uv[1] = sidePos.uv1;
                uv[2] = sidePos.uv2;
                uv[3] = sidePos.uv3;
                return;
            case BlockFace.BACK:
                verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                verts[1] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z + 1);
                verts[2] = new float3(blockPos.x, blockPos.y + height, blockPos.z + 1);
                verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                uv[0] = sidePos.uv0;
                uv[1] = sidePos.uv1;
                uv[2] = sidePos.uv2;
                uv[3] = sidePos.uv3;
                return;
            case BlockFace.RIGHT:
                verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                verts[1] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z);
                verts[2] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z + 1);
                verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                uv[0] = sidePos.uv0;
                uv[1] = sidePos.uv1;
                uv[2] = sidePos.uv2;
                uv[3] = sidePos.uv3;
                return;
            case BlockFace.LEFT:
                verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);
                verts[1] = new float3(blockPos.x, blockPos.y + height, blockPos.z + 1);
                verts[2] = new float3(blockPos.x, blockPos.y + height, blockPos.z);
                verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z);

                uv[0] = sidePos.uv0;
                uv[1] = sidePos.uv1;
                uv[2] = sidePos.uv2;
                uv[3] = sidePos.uv3;
                return;
        }
    }

    #endregion

    private void GetGrassUVS(NativeArray<float2> uv, short param)
    {
        switch (param)
        {
            case 0:
                uv[0] = topPos.uv0;
                uv[1] = topPos.uv1;
                uv[2] = topPos.uv2;
                uv[3] = topPos.uv3;
                break;
            case 1:
                uv[0] = sidePos.uv0;
                uv[1] = sidePos.uv1;
                uv[2] = sidePos.uv2;
                uv[3] = sidePos.uv3;
                break;
            case 2:
                uv[0] = bottomPos.uv0;
                uv[1] = bottomPos.uv1;
                uv[2] = bottomPos.uv2;
                uv[3] = bottomPos.uv3;
                break;
        }
    }

    private void Rotate(NativeArray<float3> verts, int3 blockpos, short deg)
    {
        float3 center = blockpos + new float3(0.5f, 0.5f, 0.5f);

        Quaternion newRotation = new Quaternion();
        newRotation.eulerAngles = new Vector3(0, deg, 0);

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = newRotation * (verts[i] - center) + new Vector3(center.x, center.y, center.z);
        }
    }
}