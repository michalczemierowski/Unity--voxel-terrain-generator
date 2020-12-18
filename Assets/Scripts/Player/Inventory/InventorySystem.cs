using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        #region // === Variables === \\

        [SerializeField] private Vector2Int inventorySize;
        [SerializeField] private int toolbarSlotsCount;

        public int SelectedToolbarSlot { get; private set; }

        private InventorySlot[,] inventorySlots;
        /// <summary>
        /// Array containing all inventory slots
        /// </summary>
        public InventorySlot[,] InventorySlots => inventorySlots;

        #endregion

        #region // === Events === \\

        public delegate void ToolbarUpdate(int slot, InventorySlot oldItem, InventorySlot newItem);
        public ToolbarUpdate OnToolbarUpdate;

        public delegate void ToolbarSlotSelected(int slot);
        public ToolbarSlotSelected OnToolbarSlotSelected;

        public delegate void ActiveToolbarSlotUpdate(InventorySlot oldItem, InventorySlot newItem);
        public ActiveToolbarSlotUpdate OnActiveToolbarSlotUpdate;

        #endregion

        private void Awake()
        {
            inventorySlots = new InventorySlot[inventorySize.x, inventorySize.y];
            for(int x = 0; x < inventorySize.x; x++)
            {
                for (int y = 0; y < inventorySize.y; y++)
                {
                    inventorySlots[x,y] = new InventorySlot(x, y);
                }
            }
        }
    }
}
