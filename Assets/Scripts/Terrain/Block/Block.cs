using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Effects.SFX;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public struct Block
    {
        #region // === Variables === \\

        public BlockType type;
        public BlockShape shape;
        public BlockState state;

        public TextureTile topTexture, sideTexture, botTexture;
        public BlockUVs topUVs, sideUVs, botUvs;

        public SoundType soundType;

        #endregion

        public Block(BlockType type, BlockState state, SoundType soundType = SoundType.DESTROY_STONE)
        {
            this.type = type;
            this.state = state;
            this.shape = BlockShape.CUBE;

            topUVs = sideUVs = botUvs = BlockUVs.AIR_UV;
            topTexture = sideTexture = botTexture = TextureTile.AIR;

            this.soundType = soundType;
        }
        public Block(BlockType type, TextureTile tile, BlockShape shape, BlockState state, SoundType soundType = SoundType.DESTROY_STONE)
        {
            this.type = type;
            this.shape = shape;
            this.state = state;
            
            topTexture = sideTexture = botTexture = tile;
            topUVs = BlockUVs.GetTileUVs(tile);
            sideUVs = BlockUVs.GetTileUVs(tile);
            botUvs = BlockUVs.GetTileUVs(tile);

            this.soundType = soundType;
        }
        public Block(BlockType type, TextureTile top, TextureTile side, TextureTile bottom, BlockShape shape, BlockState state, SoundType soundType = SoundType.DESTROY_STONE)
        {
            this.type = type;
            this.shape = shape;
            this.state = state;

            this.topTexture = top;
            this.sideTexture = side;
            this.botTexture = bottom;
            topUVs = BlockUVs.GetTileUVs(top);
            sideUVs = BlockUVs.GetTileUVs(side);
            botUvs = BlockUVs.GetTileUVs(bottom);

            this.soundType = soundType;
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

                            uv[0] = topUVs.uv0;
                            uv[1] = topUVs.uv1;
                            uv[2] = topUVs.uv2;
                            uv[3] = topUVs.uv3;
                            return false;
                        case BlockFace.BOTTOM:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                            uv[0] = botUvs.uv0;
                            uv[1] = botUvs.uv1;
                            uv[2] = botUvs.uv2;
                            uv[3] = botUvs.uv3;
                            return false;
                        case BlockFace.FRONT:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z);
                            verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);

                            uv[0] = sideUVs.uv0;
                            uv[1] = sideUVs.uv1;
                            uv[2] = sideUVs.uv2;
                            uv[3] = sideUVs.uv3;
                            return false;
                        case BlockFace.BACK:
                            verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                            verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                            uv[0] = sideUVs.uv0;
                            uv[1] = sideUVs.uv1;
                            uv[2] = sideUVs.uv2;
                            uv[3] = sideUVs.uv3;
                            return false;
                        case BlockFace.RIGHT:
                            verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                            uv[0] = sideUVs.uv0;
                            uv[1] = sideUVs.uv1;
                            uv[2] = sideUVs.uv2;
                            uv[3] = sideUVs.uv3;
                            return false;
                        case BlockFace.LEFT:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);
                            verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                            verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z);
                            verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z);

                            uv[0] = sideUVs.uv0;
                            uv[1] = sideUVs.uv1;
                            uv[2] = sideUVs.uv2;
                            uv[3] = sideUVs.uv3;
                            return false;
                    }
                    break;
                case BlockShape.GRASS:
                    switch (face)
                    {
                        case BlockFace.FRONT:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x, blockPos.y + 0.9f, blockPos.z);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y + 0.9f, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                            GetGrassUVS(uv, param);
                            return false;
                        case BlockFace.BACK:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x, blockPos.y + 0.9f, blockPos.z);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y + 0.9f, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                            GetGrassUVS(uv, param);
                            return true;
                        case BlockFace.RIGHT:
                            verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y + 0.9f, blockPos.z);
                            verts[2] = new float3(blockPos.x, blockPos.y + 0.9f, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                            GetGrassUVS(uv, param);
                            return false;
                        case BlockFace.LEFT:
                            verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y + 0.9f, blockPos.z);
                            verts[2] = new float3(blockPos.x, blockPos.y + 0.9f, blockPos.z + 1);
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

                            uv[0] = topUVs.uv0;
                            uv[1] = topUVs.uv1;
                            uv[2] = topUVs.uv2;
                            uv[3] = topUVs.uv3;
                            return false;
                        case BlockFace.BOTTOM:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                            if (param != 0)
                                Rotate(verts, blockPos, param);

                            uv[0] = botUvs.uv0;
                            uv[1] = botUvs.uv1;
                            uv[2] = botUvs.uv2;
                            uv[3] = botUvs.uv3;
                            return false;
                        case BlockFace.FRONT:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);

                            if (param != 0)
                                Rotate(verts, blockPos, param);

                            uv[0] = topUVs.uv0;
                            uv[1] = topUVs.uv1;
                            uv[2] = topUVs.uv2;
                            uv[3] = topUVs.uv3;
                            return false;
                        case BlockFace.BACK:
                            verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                            verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                            if (param != 0)
                                Rotate(verts, blockPos, param);

                            uv[0] = sideUVs.uv0;
                            uv[1] = sideUVs.uv1;
                            uv[2] = sideUVs.uv2;
                            uv[3] = sideUVs.uv3;
                            return false;
                        case BlockFace.RIGHT:
                            verts[0] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                            verts[2] = new float3(blockPos.x + 1, blockPos.y + 1, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);

                            if (param != 0)
                                Rotate(verts, blockPos, param);

                            uv[0] = sideUVs.uv0;
                            uv[1] = sideUVs.uv1;
                            uv[2] = sideUVs.uv2;
                            uv[3] = sideUVs.uv3;
                            return false;
                        case BlockFace.LEFT:
                            verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[1] = new float3(blockPos.x, blockPos.y, blockPos.z);
                            verts[2] = new float3(blockPos.x, blockPos.y + 1, blockPos.z + 1);
                            verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                            if (param != 0)
                                Rotate(verts, blockPos, param);

                            uv[0] = sideUVs.uv0;
                            uv[1] = sideUVs.uv1;
                            uv[2] = sideUVs.uv2;
                            uv[3] = sideUVs.uv3;
                            return true;
                    }
                    break;
            }

            return false;
        }
        public bool GetWaterShape(BlockFace face, int3 blockPos, NativeArray<float3> verts, NativeArray<float2> uv, short sourceDistance, NativeArray<short> nearbyLiquidSourceDistance)
        {
            float height = (float)sourceDistance / 8;
            // R L F B
            float heightRight = (float)nearbyLiquidSourceDistance[0] / 8;
            float heightLeft = (float)nearbyLiquidSourceDistance[1] / 8;
            float heightFront = (float)nearbyLiquidSourceDistance[3] / 8;
            float heightBack = (float)nearbyLiquidSourceDistance[2] / 8;

            switch (face)
            {
                case BlockFace.TOP:
                    verts[0] = new float3(blockPos.x, blockPos.y + height, blockPos.z);
                    verts[1] = new float3(blockPos.x, blockPos.y + height, blockPos.z + 1);
                    verts[2] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z + 1);
                    verts[3] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z);

                    uv[0] = topUVs.uv0;
                    uv[1] = topUVs.uv1;
                    uv[2] = topUVs.uv2;
                    uv[3] = topUVs.uv3;
                    return false;
                case BlockFace.BOTTOM:
                    verts[0] = new float3(blockPos.x, blockPos.y, blockPos.z);
                    verts[1] = new float3(blockPos.x + 1, blockPos.y, blockPos.z);
                    verts[2] = new float3(blockPos.x + 1, blockPos.y, blockPos.z + 1);
                    verts[3] = new float3(blockPos.x, blockPos.y, blockPos.z + 1);

                    uv[0] = botUvs.uv0;
                    uv[1] = botUvs.uv1;
                    uv[2] = botUvs.uv2;
                    uv[3] = botUvs.uv3;
                    return false;
                case BlockFace.FRONT:
                    verts[0] = new float3(blockPos.x, blockPos.y + heightFront, blockPos.z);
                    verts[1] = new float3(blockPos.x, blockPos.y + height, blockPos.z);
                    verts[2] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z);
                    verts[3] = new float3(blockPos.x + 1, blockPos.y + heightFront, blockPos.z);

                    uv[0] = new Vector2(sideUVs.uv0.x, sideUVs.uv0.y + heightFront / BlockUVs.textureSize);
                    uv[1] = sideUVs.uv1;
                    uv[2] = sideUVs.uv2;
                    uv[3] = new Vector2(sideUVs.uv3.x, sideUVs.uv0.y + heightFront / BlockUVs.textureSize);
                    return height < heightFront;
                case BlockFace.BACK:
                    verts[0] = new float3(blockPos.x + 1, blockPos.y + heightBack, blockPos.z + 1);
                    verts[1] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z + 1);
                    verts[2] = new float3(blockPos.x, blockPos.y + height, blockPos.z + 1);
                    verts[3] = new float3(blockPos.x, blockPos.y + heightBack, blockPos.z + 1);

                    uv[0] = new Vector2(sideUVs.uv0.x, sideUVs.uv0.y + heightBack / BlockUVs.textureSize);
                    uv[1] = sideUVs.uv1;
                    uv[2] = sideUVs.uv2;
                    uv[3] = new Vector2(sideUVs.uv3.x, sideUVs.uv0.y + heightBack / BlockUVs.textureSize);
                    return height < heightBack;
                case BlockFace.RIGHT:
                    verts[0] = new float3(blockPos.x + 1, blockPos.y + heightRight, blockPos.z);
                    verts[1] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z);
                    verts[2] = new float3(blockPos.x + 1, blockPos.y + height, blockPos.z + 1);
                    verts[3] = new float3(blockPos.x + 1, blockPos.y + heightRight, blockPos.z + 1);

                    uv[0] = new Vector2(sideUVs.uv0.x, sideUVs.uv0.y + heightRight / BlockUVs.textureSize);
                    uv[1] = sideUVs.uv1;
                    uv[2] = sideUVs.uv2;
                    uv[3] = new Vector2(sideUVs.uv3.x, sideUVs.uv0.y + heightRight / BlockUVs.textureSize);
                    return height < heightRight;
                case BlockFace.LEFT:
                    verts[0] = new float3(blockPos.x, blockPos.y + heightLeft, blockPos.z + 1);
                    verts[1] = new float3(blockPos.x, blockPos.y + height, blockPos.z + 1);
                    verts[2] = new float3(blockPos.x, blockPos.y + height, blockPos.z);
                    verts[3] = new float3(blockPos.x, blockPos.y + heightLeft, blockPos.z);

                    uv[0] = new Vector2(sideUVs.uv0.x, sideUVs.uv0.y + heightLeft / BlockUVs.textureSize);
                    uv[1] = sideUVs.uv1;
                    uv[2] = sideUVs.uv2;
                    uv[3] = new Vector2(sideUVs.uv3.x, sideUVs.uv0.y + heightLeft / BlockUVs.textureSize);
                    return height < heightLeft;
            }

            return false;
        }

        #endregion
        private void GetGrassUVS(NativeArray<float2> uv, short param)
        {
            switch (param)
            {
                case 0:
                    uv[0] = topUVs.uv0;
                    uv[1] = topUVs.uv1;
                    uv[2] = topUVs.uv2;
                    uv[3] = topUVs.uv3;
                    break;
                case 1:
                    uv[0] = sideUVs.uv0;
                    uv[1] = sideUVs.uv1;
                    uv[2] = sideUVs.uv2;
                    uv[3] = sideUVs.uv3;
                    break;
                case 2:
                    uv[0] = botUvs.uv0;
                    uv[1] = botUvs.uv1;
                    uv[2] = botUvs.uv2;
                    uv[3] = botUvs.uv3;
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
}