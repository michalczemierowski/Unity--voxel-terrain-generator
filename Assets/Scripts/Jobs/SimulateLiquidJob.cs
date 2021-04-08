using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VoxelTG.Extensions;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using static VoxelTG.WorldSettings;

/*
 * Micha� Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Jobs
{
    [BurstCompile]
    public struct SimulateLiquidJob : IJob
    {
        public NativeArray<BlockType> blocks;
        public NativeArray<bool> needsRebuild;
        public NativeHashMap<int, BlockParameter> blockParameters;
        public int maxStepsPerFrame;

        public void Execute()
        {
            for (int i = 0; i < needsRebuild.Length; i++)
            {
                needsRebuild[i] = false;
            }

            NativeArray<BlockType> newblocks = new NativeArray<BlockType>(blocks, Allocator.Temp);
            for (int x = 0; x < FixedChunkSizeXZ; x++)
            {
                for (int z = 0; z < FixedChunkSizeXZ; z++)
                {
                    for (int y = ChunkSizeY - 1; y > 1; y--)
                    {
                        if (blocks[Utils.BlockPosition3DtoIndex(x, y, z)] == BlockType.WATER)
                        {
                            byte source = WaterSourceMax;
                            if (blockParameters.TryGetParameterValue(new int3(x, y, z), ParameterType.LIQUID_SOURCE_DISTANCE, out byte value))
                                source = value;

                            ProcessWater(x, y, z, source, 0, ref newblocks);
                        }
                    }
                }
            }
            newblocks.CopyTo(blocks);
            newblocks.Dispose();
        }

        private void ProcessWater(int x, int y, int z, byte source, int step, ref NativeArray<BlockType> newblocks)
        {
            if (++step > maxStepsPerFrame || y == 0 || source < 2)
                return;

            --source;

            int3 belowPos = new int3(x, y - 1, z);
            int belowIndex = Utils.BlockPosition3DtoIndex(belowPos);
            if (CanReplace(belowIndex, belowPos, WaterSourceMax))
            {
                SetWater(belowIndex, belowPos, WaterSourceMax, ref newblocks);
                ProcessWater(belowPos.x, belowPos.y, belowPos.z, WaterSourceMax, step, ref newblocks);
            }
            else if (WorldData.GetBlockState(blocks[belowIndex]) == BlockState.SOLID)
            {
                if (x > 0)
                {
                    int3 leftPos = new int3(x - 1, y, z);
                    int leftIndex = Utils.BlockPosition3DtoIndex(leftPos);
                    if (CanReplace(leftIndex, leftPos, source))
                    {
                        SetWater(leftIndex, leftPos, source, ref newblocks);
                        ProcessWater(leftPos.x, leftPos.y, leftPos.z, source, step, ref newblocks);
                    }
                }

                if (x < FixedChunkSizeXZ - 1)
                {
                    int3 rightPos = new int3(x + 1, y, z);
                    int rightIndex = Utils.BlockPosition3DtoIndex(rightPos);
                    if (CanReplace(rightIndex, rightPos, source))
                    {
                        SetWater(rightIndex, rightPos, source, ref newblocks);
                        ProcessWater(rightPos.x, rightPos.y, rightPos.z, source, step, ref newblocks);
                    }
                }

                if (z > 0)
                {
                    int3 backPos = new int3(x, y, z - 1);
                    int backIndex = Utils.BlockPosition3DtoIndex(backPos);
                    if (CanReplace(backIndex, backPos, source))
                    {
                        SetWater(backIndex, backPos, source, ref newblocks);
                        ProcessWater(backPos.x, backPos.y, backPos.z, source, step, ref newblocks);
                    }
                }

                if (z < FixedChunkSizeXZ - 1)
                {
                    int3 frontPos = new int3(x, y, z + 1);
                    int frontIndex = Utils.BlockPosition3DtoIndex(frontPos);
                    if (CanReplace(frontIndex, frontPos, source))
                    {
                        SetWater(frontIndex, frontPos, source, ref newblocks);
                        ProcessWater(frontPos.x, frontPos.y, frontPos.z, source, step, ref newblocks);
                    }
                }
            }
        }

        private void SetWater(int index, int3 pos, byte source, ref NativeArray<BlockType> newblocks)
        {
            newblocks[index] = BlockType.WATER;
            blockParameters.SetParameterValue(pos, ParameterType.LIQUID_SOURCE_DISTANCE, source);
            NeedsRebuild(pos);
        }

        private bool CanReplace(int index, int3 pos, byte source)
        {
            if (blocks[index] == BlockType.WATER)
            {
                if (blockParameters.TryGetParameterValue(pos, ParameterType.LIQUID_SOURCE_DISTANCE, out byte value))
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