using UnityEngine;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Entities.Items
{
    public class DroppedItem : MonoBehaviour
    {
        /// <summary>
        /// reference to item data
        /// </summary>
        public InventoryItemData inventoryItemData;

        /// <summary>
        /// Called when player interacts with dropped item
        /// </summary>
        public void Pickup()
        {
            if (UIManager.Instance.inventoryUI.AddItemToInventory(inventoryItemData))
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool onCollisionEnabled = DroppedItemsManager.Instance.PickupItemOnCollision;
            if (onCollisionEnabled && collision.transform.CompareTag("Player"))
                Pickup();
        }
    }
}