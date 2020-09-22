/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
using VoxelTG.UI;

namespace VoxelTG.Entities.Items
{
    public class DroppedItem : MonoBehaviour
    {
        public InventoryItemData inventoryItemData;

        public void Pickup()
        {
            if (UIManager.Instance.inventoryUI.AddItemToInventory(inventoryItemData))
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // TODO: settings for this one (enable/disable)
            if (collision.transform.CompareTag("Player"))
                Pickup();
        }
    }
}