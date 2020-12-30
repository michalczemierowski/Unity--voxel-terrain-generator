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

        [Tooltip("Damage that will be dealed to living entity on hit")]
        [SerializeField] private float damage;
        /// <summary>
        /// Damage that will be dealed to living entity on hit
        /// </summary>
        public float Damage => damage;

        [Tooltip("How many bullets can be fired in a second")]
        [SerializeField] private float fireRate = 5f;
        /// <summary>
        /// How many bullets can be fired in a second
        /// </summary>
        public float FireRate => fireRate;

        [Tooltip("Is continuous firing possible while holding down the shot key")]
        [SerializeField] private bool isAutomaticRifle;
        /// <summary>
        /// Is continuous firing possible while holding down the shot key
        /// </summary>
        public bool IsAutomaticRifle => isAutomaticRifle;

        [Tooltip("Damage that will be dealed to block on hit")]
        [SerializeField] private float blockDamage = 1f;
        /// <summary>
        /// Damage that will be dealed to block on hit
        /// </summary>
        public float BlockDamage => blockDamage;
        
        [Tooltip("Maximum number of bullets in the clip")]
        [SerializeField] private int magazineCapacity = 30;
        /// <summary>
        /// Maximum number of bullets in the clip
        /// </summary>
        public int MagazineCapacity => magazineCapacity;

        [Tooltip("Addressables key to in-hand object")]
        [SerializeField] private string addressablePathToModel;
        /// <summary>
        /// Addressables key to in-hand object
        /// </summary>
        public string AddressablePathToModel => addressablePathToModel;

        public override bool IsWeapon => true;
    }
}