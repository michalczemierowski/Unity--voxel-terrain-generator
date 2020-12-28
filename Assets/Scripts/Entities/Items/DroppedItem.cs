using UnityEngine;
using VoxelTG.Extensions;
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
        public InventoryItemBase Item { get; private set; }

        private int amount;
        public int Amount
        {
            get => amount;
            set
            {
                amount = amount > 0 ? amount : 1;
            }
        }

        /// <summary>
        /// Called when player interacts with dropped item
        /// </summary>
        public void Pickup()
        {
            if (Item != null)
            {
                Player.PlayerController.InventorySystem.AddItem(Item, amount);
                Destroy(gameObject);
            }
        }


        public void SetInventoryItem(InventoryItemBase inventoryItem)
        {
            this.Item = inventoryItem;
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool onCollisionEnabled = DroppedItemsManager.Instance.PickupItemOnCollision;
            if (onCollisionEnabled && collision.transform.CompareTag("Player"))
                Pickup();
        }
    }
}