using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Jobs
{
    [BurstCompile]
    public struct BakePhysicsXMesh : IJob
    {
        [ReadOnly]
        public int meshID;

        public void Execute()
        {
            UnityEngine.Physics.BakeMesh(meshID, false);
        }
    }
}