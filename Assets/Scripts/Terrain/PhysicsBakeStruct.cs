using Unity.Jobs;
using UnityEngine;

namespace VoxelTG.Terrain
{
    public struct PhysicsBakeStruct
    {
        public JobHandle jobHandle;
        public Mesh mesh;
        public MeshCollider meshCollider;

        public PhysicsBakeStruct(JobHandle jobHandle, Mesh mesh, MeshCollider meshCollider)
        {
            this.jobHandle = jobHandle;
            this.mesh = mesh;
            this.meshCollider = meshCollider;
        }
    }
}