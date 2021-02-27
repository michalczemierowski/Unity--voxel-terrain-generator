using UnityEngine;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
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
        /// Drop pickupable prefab on ground.
        /// </summary>
        /// <param name="itemType">type of item</param>
        /// <param name="position">dropped item position (in world space)</param>
        /// <param name="amount">dropped item count</param>
        /// <param name="velocity">velocity multipler (camera.forward * velocity)</param>
        /// <param name="handObjectToCopy">GameObject that will be displayed in dropped item</param>
        public void DropItem(ItemType itemType, Vector3 position, int amount = 1, float velocity = 0f, GameObject handObjectToCopy = null)
        {
            if (itemType == ItemType.NONE)
                return;

            Chunk chunk = World.GetChunk(position.x, position.z);
            DroppedItem droppedItem = Instantiate(toolItemPrefab, position, Quaternion.identity, chunk.transform).GetComponent<DroppedItem>();

            if(handObjectToCopy != null)
            {
                GameObject tool = Instantiate(handObjectToCopy, droppedItem.transform);
                tool.transform.localPosition = Vector3.zero;
                tool.transform.localRotation = Quaternion.identity;
            }
            else
            {
                GameObject primitive = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), droppedItem.transform);
                primitive.transform.localPosition = Vector3.zero;
                primitive.transform.localRotation = Quaternion.identity;
            }

            PlayerController.InventorySystem.GetItemData(itemType, (item) => droppedItem.SetInventoryItem(item));
            droppedItem.Amount = amount;

            if(velocity != 0 && droppedItem.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(MouseLook.cameraTransform.forward * velocity, ForceMode.Impulse);
                droppedItem.transform.forward = MouseLook.cameraTransform.forward;
            }
        }

        /// <summary>
        /// Drop pickupable prefab on ground.
        /// </summary>
        /// <param name="blockType">type of material</param>
        /// <param name="position">doppped item position (in world space)</param>
        /// <param name="amount">dropped item count</param>
        /// <param name="velocity">velocity multipler (camera.forward * velocity)</param>
        /// <param name="rotate">should object be rotated same as camera</param>
        public void DropItem(BlockType blockType, Vector3 position, int amount = 1, float velocity = 0f, bool rotate = false)
        {
            if (blockType == BlockType.AIR)
                return;

            Chunk chunk = World.GetChunk(position.x, position.z);
            DroppedItem droppedItem = Instantiate(materialItemPrefab, position, Quaternion.identity, chunk.transform).GetComponent<DroppedItem>();
            
            PlayerController.InventorySystem.GetItemData(blockType, (item) => droppedItem.SetInventoryItem(item));
            droppedItem.Amount = amount;

            if (velocity != 0 && droppedItem.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(MouseLook.cameraTransform.forward * velocity, ForceMode.Impulse);
                if(rotate)
                    droppedItem.transform.forward = MouseLook.cameraTransform.forward;
            }

             Mesh mesh = droppedItem.GetComponent<MeshFilter>().mesh;
             MeshUtils.CreateBlockCube(mesh, blockType, 0.75f);
        }
    }
}