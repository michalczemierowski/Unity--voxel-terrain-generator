using Unity.Burst;
using UnityEngine;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    [BurstCompile]
    public static class MeshUtils
    {
        public static void CreateBlockCube(Mesh mesh, BlockType blockType, float cubeSize = 1, float pivotX = 0.5f, float pivotY = 0.5f, float pivotZ = 0.5f)
        {
            BlockStructure block = WorldData.GetBlockData(blockType);
            Vector3[] verts = new Vector3[24];
            Vector2[] uv = new Vector2[24];

            float startPosX = -pivotX * cubeSize;
            float startPosY = -pivotY * cubeSize;
            float startPosZ = -pivotZ * cubeSize;

            verts[0] = new Vector3(startPosX, startPosY + cubeSize, startPosZ);
            verts[1] = new Vector3(startPosX, startPosY + cubeSize, startPosZ + cubeSize);
            verts[2] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ + cubeSize);
            verts[3] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ);

            uv[0] = block.topUVs.uv0;
            uv[1] = block.topUVs.uv1;
            uv[2] = block.topUVs.uv2;
            uv[3] = block.topUVs.uv3;

            verts[4] = new Vector3(startPosX, startPosY, startPosZ);
            verts[5] = new Vector3(startPosX + cubeSize, startPosY, startPosZ);
            verts[6] = new Vector3(startPosX + cubeSize, startPosY, startPosZ + cubeSize);
            verts[7] = new Vector3(startPosX, startPosY, startPosZ + cubeSize);

            uv[4] = block.botUvs.uv0;
            uv[5] = block.botUvs.uv1;
            uv[6] = block.botUvs.uv2;
            uv[7] = block.botUvs.uv3;

            verts[8] = new Vector3(startPosX, startPosY, startPosZ);
            verts[9] = new Vector3(startPosX, startPosY + cubeSize, startPosZ);
            verts[10] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ);
            verts[11] = new Vector3(startPosX + cubeSize, startPosY, startPosZ);

            uv[8] = block.sideUVs.uv0;
            uv[9] = block.sideUVs.uv1;
            uv[10] = block.sideUVs.uv2;
            uv[11] = block.sideUVs.uv3;

            verts[12] = new Vector3(startPosX + cubeSize, startPosY, startPosZ + cubeSize);
            verts[13] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ + cubeSize);
            verts[14] = new Vector3(startPosX, startPosY + cubeSize, startPosZ + cubeSize);
            verts[15] = new Vector3(startPosX, startPosY, startPosZ + cubeSize);

            uv[12] = block.sideUVs.uv0;
            uv[13] = block.sideUVs.uv1;
            uv[14] = block.sideUVs.uv2;
            uv[15] = block.sideUVs.uv3;

            verts[16] = new Vector3(startPosX + cubeSize, startPosY, startPosZ);
            verts[17] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ);
            verts[18] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ + cubeSize);
            verts[19] = new Vector3(startPosX + cubeSize, startPosY, startPosZ + cubeSize);

            uv[16] = block.sideUVs.uv0;
            uv[17] = block.sideUVs.uv1;
            uv[18] = block.sideUVs.uv2;
            uv[19] = block.sideUVs.uv3;

            verts[20] = new Vector3(startPosX, startPosY, startPosZ + cubeSize);
            verts[21] = new Vector3(startPosX, startPosY + cubeSize, startPosZ + cubeSize);
            verts[22] = new Vector3(startPosX, startPosY + cubeSize, startPosZ);
            verts[23] = new Vector3(startPosX, startPosY, startPosZ);

            uv[20] = block.sideUVs.uv0;
            uv[21] = block.sideUVs.uv1;
            uv[22] = block.sideUVs.uv2;
            uv[23] = block.sideUVs.uv3;

            int[] triangles = new int[36];
            int counter = 0;
            for (int i = 0; i < 6; i++)
            {
                triangles[counter + 0] = i * 4;
                triangles[counter + 1] = i * 4 + 1;
                triangles[counter + 2] = i * 4 + 2;
                triangles[counter + 3] = i * 4;
                triangles[counter + 4] = i * 4 + 2;
                triangles[counter + 5] = i * 4 + 3;
                counter += 6;
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
        }
    }
}
