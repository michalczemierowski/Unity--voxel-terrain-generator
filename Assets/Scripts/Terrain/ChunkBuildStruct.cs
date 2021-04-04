using Unity.Collections;
using Unity.Jobs;

namespace VoxelTG.Terrain
{
    public struct ChunkBuildStruct
    {
        public bool playAnimation;
        public bool rebuildPhysics;
        public bool createBiomeTexture;
        public Chunk chunk;
        public JobHandle jobHandle;
        public NativeArray<BlockType> blocksBuffer;

        public ChunkBuildStruct(Chunk chunk, JobHandle jobHandle)
        {
            playAnimation = true;
            rebuildPhysics = true;
            createBiomeTexture = true;
            this.chunk = chunk;
            this.jobHandle = jobHandle;
            blocksBuffer = default;
        }

        public ChunkBuildStruct(bool playAnimation, bool rebuildPhysics, bool createBiomeTexture, Chunk chunk, JobHandle jobHandle, NativeArray<BlockType> blocksBuffer = default)
        {
            this.playAnimation = playAnimation;
            this.rebuildPhysics = rebuildPhysics;
            this.createBiomeTexture = createBiomeTexture;
            this.chunk = chunk;
            this.jobHandle = jobHandle;
            this.blocksBuffer = blocksBuffer;
        }
    }
}
