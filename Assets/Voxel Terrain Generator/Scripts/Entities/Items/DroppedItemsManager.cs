/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using VoxelTG.Player.Inventory;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;

namespace VoxelTG.Entities.Items
{
    public class DroppedItemsManager : MonoBehaviour
    {
        public static DroppedItemsManager Instance;
        [SerializeField] private GameObject itemPrefab;

        private void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;
        }

        public void DropItem(ToolType toolType, Vector3 position, int count = 1, float velocity = 0f)
        {
            Chunk chunk = World.GetChunk(position.x, position.z);
            DroppedItem item = Instantiate(itemPrefab, position, Quaternion.identity, chunk.transform).GetComponent<DroppedItem>();

            item.inventoryItemData = new InventoryItemData(toolType, count);

            if(velocity != 0 && item.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(MouseLook.cameraTransform.forward * velocity, ForceMode.Impulse);
                item.transform.forward = MouseLook.cameraTransform.forward;
            }
        }

        public void DropMaterial(BlockType blockType, Vector3 position, int count = 1, float velocity = 0f)
        {
            Chunk chunk = World.GetChunk(position.x, position.z);
            DroppedItem item = Instantiate(itemPrefab, position, Quaternion.identity, chunk.transform).GetComponent<DroppedItem>();

            item.inventoryItemData = new InventoryItemData(blockType, count);

            if (velocity != 0 && item.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(MouseLook.cameraTransform.forward * velocity, ForceMode.Impulse);
                item.transform.forward = MouseLook.cameraTransform.forward;
            }

             Mesh mesh = item.GetComponent<MeshFilter>().mesh;
             CreateCube(mesh, blockType, 0.75f);
        }


        // TODO: move to different script and add pooling
        private void CreateCube(Mesh mesh, BlockType blockType, float cubeSize = 1, float pivotX = 0.5f, float pivotY = 0.5f, float pivotZ = 0.5f)
        {
            Block block = WorldData.GetBlockData(blockType);
            Vector3[] verts = new Vector3[24];
            Vector2[] uv = new Vector2[24];

            float startPosX = -pivotX * cubeSize;
            float startPosY = -pivotY * cubeSize;
            float startPosZ = -pivotZ * cubeSize;

            verts[0] = new Vector3(startPosX, startPosY + cubeSize, startPosZ);
            verts[1] = new Vector3(startPosX, startPosY + cubeSize, startPosZ + cubeSize);
            verts[2] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ + cubeSize);
            verts[3] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ);

            uv[0] = block.topPos.uv0;
            uv[1] = block.topPos.uv1;
            uv[2] = block.topPos.uv2;
            uv[3] = block.topPos.uv3;

            verts[4] = new Vector3(startPosX, startPosY, startPosZ);
            verts[5] = new Vector3(startPosX + cubeSize, startPosY, startPosZ);
            verts[6] = new Vector3(startPosX + cubeSize, startPosY, startPosZ + cubeSize);
            verts[7] = new Vector3(startPosX, startPosY, startPosZ + cubeSize);

            uv[4] = block.bottomPos.uv0;
            uv[5] = block.bottomPos.uv1;
            uv[6] = block.bottomPos.uv2;
            uv[7] = block.bottomPos.uv3;

            verts[8] = new Vector3(startPosX, startPosY, startPosZ);
            verts[9] = new Vector3(startPosX, startPosY + cubeSize, startPosZ);
            verts[10] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ);
            verts[11] = new Vector3(startPosX + cubeSize, startPosY, startPosZ);

            uv[8] = block.sidePos.uv0;
            uv[9] = block.sidePos.uv1;
            uv[10] = block.sidePos.uv2;
            uv[11] = block.sidePos.uv3;

            verts[12] = new Vector3(startPosX + cubeSize, startPosY, startPosZ + cubeSize);
            verts[13] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ + cubeSize);
            verts[14] = new Vector3(startPosX, startPosY + cubeSize, startPosZ + cubeSize);
            verts[15] = new Vector3(startPosX, startPosY, startPosZ + cubeSize);

            uv[12] = block.sidePos.uv0;
            uv[13] = block.sidePos.uv1;
            uv[14] = block.sidePos.uv2;
            uv[15] = block.sidePos.uv3;

            verts[16] = new Vector3(startPosX + cubeSize, startPosY, startPosZ);
            verts[17] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ);
            verts[18] = new Vector3(startPosX + cubeSize, startPosY + cubeSize, startPosZ + cubeSize);
            verts[19] = new Vector3(startPosX + cubeSize, startPosY, startPosZ + cubeSize);

            uv[16] = block.sidePos.uv0;
            uv[17] = block.sidePos.uv1;
            uv[18] = block.sidePos.uv2;
            uv[19] = block.sidePos.uv3;

            verts[20] = new Vector3(startPosX, startPosY, startPosZ + cubeSize);
            verts[21] = new Vector3(startPosX, startPosY + cubeSize, startPosZ + cubeSize);
            verts[22] = new Vector3(startPosX, startPosY + cubeSize, startPosZ);
            verts[23] = new Vector3(startPosX, startPosY, startPosZ);

            uv[20] = block.sidePos.uv0;
            uv[21] = block.sidePos.uv1;
            uv[22] = block.sidePos.uv2;
            uv[23] = block.sidePos.uv3;

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