using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemWeapon_0", menuName = "Scriptable Objects/Inventory/Weapon")]
    public class InventoryItemWeapon : InventoryItemBase
    {
        [Header("Inventory item weapon settings")]
        /// <summary>
        /// Type of item
        /// </summary>
        public ItemType itemType = ItemType.NONE;

        /// <summary>
        /// Damage that will be dealed to living entity on hit
        /// </summary>
        public float damage;

        [Tooltip("Bullets per second")]
        public float fireRate = 5f;

        public bool isAutomaticRifle;

        /// <summary>
        /// Damage that will be dealed to block on hit
        /// </summary>
        public float blockDamage = 1f;
        /// <summary>
        /// Magazine capacity
        /// </summary>
        public int maxAmmo = 30;

        /// <summary>
        /// Path to in-hand model
        /// </summary>
        public string addressablePathToModel;

        public override bool IsWeapon()
        {
            return true;
        }

        public override bool IsSameType<T>(T other)
        {
            if (other is InventoryItemWeapon otherWeapon)
            {
                return otherWeapon.itemType == itemType;
            }

            return false;
        }
    }
}