using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using static VoxelTG.WorldSettings;

/*
 * Micha³ Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Jobs
{
    public struct SimulateWaterJob : IJob
    {
        public NativeArray<BlockType> blocks;
        public NativeArray<bool> needsRebuild;
        public NativeHashMap<BlockParameter, short> blockParameters;
        public int maxStepsPerFrame;

        public void Execute()
        {
            for (int x = 0; x < FixedChunkSizeXZ; x++)
            {
                for (int z = 0; z < FixedChunkSizeXZ; z++)
                {
                    for (int y = ChunkSizeY - 1; y > 1; y--)
                    {
                        if (blocks[Utils.BlockPosition3DtoIndex(x, y, z)] == BlockType.WATER)
                        {
                            short source = WaterSourceMax;
                            var param = new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE);
                            if (blockParameters.TryGetValue(param, out short value))
                                source = value;

                            ProcessWater(x, y, z, source, 0);
                        }
                    }
                }
            }
        }

        private void ProcessWater(int x, int y, int z, short source, int step)
        {
            if (++step > maxStepsPerFrame || y == 0 || source < 2)
                return;

            --source;

            int3 belowPos = new int3(x, y - 1, z);
            int belowIndex = Utils.BlockPosition3DtoIndex(belowPos);
            if (CanReplace(belowIndex, belowPos, WaterSourceMax))
            {
                SetWater(belowIndex, belowPos, WaterSourceMax);
                ProcessWater(belowPos.x, belowPos.y, belowPos.z, WaterSourceMax, step);
            }
            else if (WorldData.GetBlockState(blocks[belowIndex]) == BlockState.SOLID)
            {
                if (x > 0)
                {
                    int3 leftPos = new int3(x - 1, y, z);
                    int leftIndex = Utils.BlockPosition3DtoIndex(leftPos);
                    if (CanReplace(leftIndex, leftPos, source))
                    {
                        SetWater(leftIndex, leftPos, source);
                        ProcessWater(leftPos.x, leftPos.y, leftPos.z, source, step);
                    }
                }

                if (x < FixedChunkSizeXZ - 1)
                {
                    int3 rightPos = new int3(x + 1, y, z);
                    int rightIndex = Utils.BlockPosition3DtoIndex(rightPos);
                    if (CanReplace(rightIndex, rightPos, source))
                    {
                        SetWater(rightIndex, rightPos, source);
                        ProcessWater(rightPos.x, rightPos.y, rightPos.z, source, step);
                    }
                }

                if (z > 0)
                {
                    int3 backPos = new int3(x, y, z - 1);
                    int backIndex = Utils.BlockPosition3DtoIndex(backPos);
                    if (CanReplace(backIndex, backPos, source))
                    {
                        SetWater(backIndex, backPos, source);
                        ProcessWater(backPos.x, backPos.y, backPos.z, source, step);
                    }
                }

                if (z < FixedChunkSizeXZ - 1)
                {
                    int3 frontPos = new int3(x, y, z + 1);
                    int frontIndex = Utils.BlockPosition3DtoIndex(frontPos);
                    if (CanReplace(frontIndex, frontPos, source))
                    {
                        SetWater(frontIndex, frontPos, source);
                        ProcessWater(frontPos.x, frontPos.y, frontPos.z, source, step);
                    }
                }
            }
        }

        private void SetWater(int index, int3 pos, short source)
        {
            blocks[index] = BlockType.WATER;
            var param = new BlockParameter(pos, ParameterType.WATER_SOURCE_DISTANCE);
            if (blockParameters.ContainsKey(param))
                blockParameters[param] = source;
            else
                blockParameters.Add(param, source);

            NeedsRebuild(pos);
        }

        private bool CanReplace(int index, int3 pos, short source)
        {
            if (blocks[index] == BlockType.WATER)
            {
                var param = new BlockParameter(pos, ParameterType.WATER_SOURCE_DISTANCE);
                if (blockParameters.TryGetValue(param, out short value))
                    return source > value;
                else
                    return false;
            }

            return blocks[index] == BlockType.AIR || WorldData.GetBlockState(blocks[index]) == BlockState.LIQUID_DESTROYABLE;
        }

        private void NeedsRebuild(int3 pos)
        {
            bool found = false;
            // +x, -x, +z, -z
            if (pos.x == FixedChunkSizeXZ - 1)
                needsRebuild[0] = found = true;
            else if (pos.x == 0)
                needsRebuild[1] = found = true;
            if (pos.z == FixedChunkSizeXZ - 1)
                needsRebuild[2] = found = true;
            else if (pos.z == 0)
                needsRebuild[3] = found = true;

            if (!needsRebuild[4])
                needsRebuild[4] = !found;
        }
    }
}