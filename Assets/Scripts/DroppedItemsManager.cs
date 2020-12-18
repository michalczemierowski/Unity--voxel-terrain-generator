using UnityEngine;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Entities.Items
{
    public class DroppedItemsManager : MonoBehaviour
    {
        // TODO: move to World.DroppedItemsManager
        public static DroppedItemsManager Instance;
        [SerializeField] private GameObject materialItemPrefab;
        [SerializeField] private GameObject toolItemPrefab;

        [SerializeField] private bool pickupItemOnCollision;
        /// <summary>
        /// If true player will pickup dopped items on collision
        /// </summary>
        public bool PickupItemOnCollision => pickupItemOnCollision;

        private void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;
        }

        /// <summary>
        /// Drop tool/weapon item (axe, gun etc.)
        /// </summary>
        /// <param name="itemType">type of item</param>
        /// <param name="position">dropped item position (in world space)</param>
        /// <param name="count">dropped item count</param>
        /// <param name="velocity">velocity multipler (camera.forward * velocity)</param>
        /// <param name="handObjectToCopy">GameObject that will be displayed in dropped item</param>
        public void DropItemTool(ItemType itemType, Vector3 position, int count = 1, float velocity = 0f, GameObject handObjectToCopy = null)
        {
            if (itemType == ItemType.NONE)
                return;

            Chunk chunk = World.GetChunk(position.x, position.z);
            DroppedItem item = Instantiate(toolItemPrefab, position, Quaternion.identity, chunk.transform).GetComponent<DroppedItem>();

            if(handObjectToCopy != null)
            {
                GameObject tool = Instantiate(handObjectToCopy, item.transform);
                tool.transform.localPosition = Vector3.zero;
                tool.transform.localRotation = Quaternion.identity;
            }
            else
            {
                GameObject primitive = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), item.transform);
                primitive.transform.localPosition = Vector3.zero;
                primitive.transform.localRotation = Quaternion.identity;
            }

            item.inventoryItemData = new InventoryItemData(itemType, count);

            if(velocity != 0 && item.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(MouseLook.cameraTransform.forward * velocity, ForceMode.Impulse);
                item.transform.forward = MouseLook.cameraTransform.forward;
            }
        }

        /// <summary>
        /// Drop material item (dirt, stone etc.)
        /// </summary>
        /// <param name="blockType">type of material</param>
        /// <param name="position">doppped item position (in world space)</param>
        /// <param name="count">dropped item count</param>
        /// <param name="velocity">velocity multipler (camera.forward * velocity)</param>
        /// <param name="rotate">should object be rotated same as camera</param>
        public void DropItemMaterial(BlockType blockType, Vector3 position, int count = 1, float velocity = 0f, bool rotate = false)
        {
            if (blockType == BlockType.AIR)
                return;

            Chunk chunk = World.GetChunk(position.x, position.z);
            DroppedItem item = Instantiate(materialItemPrefab, position, Quaternion.identity, chunk.transform).GetComponent<DroppedItem>();

            item.inventoryItemData = new InventoryItemData(blockType, count);

            if (velocity != 0 && item.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(MouseLook.cameraTransform.forward * velocity, ForceMode.Impulse);
                if(rotate)
                    item.transform.forward = MouseLook.cameraTransform.forward;
            }

             Mesh mesh = item.GetComponent<MeshFilter>().mesh;
             CreateDoppedMaterialMesh(mesh, blockType, 0.75f);
        }


        // TODO: add pooling
        public void CreateDoppedMaterialMesh(Mesh mesh, BlockType blockType, float cubeSize = 1, float pivotX = 0.5f, float pivotY = 0.5f, float pivotZ = 0.5f)
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