using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Entities;
using VoxelTG.Extensions;
using VoxelTG.Player.Inventory;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Interactions
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerWeaponController : MonoBehaviour
    {
        /// <summary>
        /// Is player using weapon
        /// </summary>
        public bool IsWeaponEquipped { get; private set; } = false;

        [Tooltip("Layers on which interactions are possible")]
        [SerializeField] private LayerMask interactionLayers;
        /// <summary>
        /// Layers on which interactions are possible
        /// </summary>
        public LayerMask InteractionLayers => interactionLayers;

        [Tooltip("Max distance at which weapon can deal damage")]
        [SerializeField] private float maxDistance = 100;
        /// <summary>
        /// Max distance at which weapon can deal damage
        /// </summary>
        public float MaxDistance => maxDistance;

        [Tooltip("Max size of damaged blocks cache (cache containing health state of damaged blocks)")]
        [SerializeField] private int maxDamagedBlocksCount;

        private Transform cameraTransform;
        private InventoryItemWeapon weaponInHand;

        private WeaponEffectsController currentWeaponFX;
        private float timeToNextShoot = 0;

        /// <summary>
        /// Cache containing health state of damaged blocks
        /// </summary>
        private Dictionary<int3, float> damagedBlocksDict = new Dictionary<int3, float>();

        private void Start()
        {
            cameraTransform = Camera.main.transform;

            PlayerController.InventorySystem.OnMainHandUpdate += OnMainHandUpdate;
            PlayerController.Instance.OnHandObjectLoaded += OnHandObjectLoaded;
        }

        private void OnMainHandUpdate(InventorySlot oldContent, InventorySlot newContent)
        {
            // if old slot is same as new - return
            if (!oldContent.IsNullOrEmpty() && !newContent.IsNullOrEmpty() && oldContent.Item.IsSameType(newContent.Item))
                return;

            if (!newContent.IsNullOrEmpty() && newContent.Item.IsWeapon)
            {
                IsWeaponEquipped = true;
                weaponInHand = (InventoryItemWeapon)newContent.Item;
                // TODO: read settings etc.
            }
            else
                IsWeaponEquipped = false;
        }

        private void OnHandObjectLoaded(GameObject handObject, ItemType itemType)
        {
            if (weaponInHand == null)
                return;

            if (weaponInHand.Type == itemType)
            {
                currentWeaponFX = handObject.GetComponent<WeaponEffectsController>();
                if (currentWeaponFX == null)
                    Debug.LogError($"{handObject} doesn't have {nameof(WeaponEffectsController)} component", this);
            }
            else
                Debug.LogWarning("Current weapon item type is different than current hand object item type", this);
        }

        private void Update()
        {
            if (!UIManager.IsUIModeActive)
            {
                if (!IsWeaponEquipped || weaponInHand == null) return;

                HandleInput();
            }
        }

        private void HandleInput()
        {
            if (timeToNextShoot <= 0)
            {
                bool shootButtonDown = weaponInHand.IsAutomaticRifle ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
                if (shootButtonDown)
                {
                    timeToNextShoot = 1f / weaponInHand.FireRate;
                    currentWeaponFX?.OnShoot();

                    if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxDistance, interactionLayers))
                    {
                        if (hitInfo.transform.CompareTag("Terrain"))
                        {
                            Vector3 pointInTargetBlock;
                            // move towards block position
                            pointInTargetBlock = hitInfo.point + cameraTransform.forward * .01f;

                            // get block & chunk
                            int3 globalBlockPosition = new int3(
                                Mathf.FloorToInt(pointInTargetBlock.x),
                                Mathf.FloorToInt(pointInTargetBlock.y),
                                Mathf.FloorToInt(pointInTargetBlock.z)
                            );

                            BlockPosition blockPosition = new BlockPosition(globalBlockPosition);
                            Chunk chunk = World.GetChunk(globalBlockPosition.x, globalBlockPosition.z);
                            BlockType blockType = chunk.GetBlock(blockPosition);

                            currentWeaponFX?.OnBulletHitTerrain(hitInfo, chunk, blockPosition, blockType);

                            // cannot destroy base layer (at y == 0)
                            if (globalBlockPosition.y > 0)
                            {
                                bool shouldBeDestroyed = DamageBlock(globalBlockPosition, blockType, weaponInHand.BlockDamage);
                                if (shouldBeDestroyed)
                                {
                                    chunk.SetBlock(blockPosition, BlockType.AIR, new SetBlockSettings(true, false, false, 10));
                                }
                            }
                        }
                        if (hitInfo.transform.tag.EndsWith("entity"))
                        {
                            Entity entity = hitInfo.transform.GetComponent<Entity>();
                            entity.Damage(weaponInHand.Damage);
                        }
                    }
                }
            }
            else
            {
                timeToNextShoot -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Damage block at position
        /// </summary>
        /// <param name="globalBlockPosition">global block position (in world space)</param>
        /// <param name="blockType">type of block</param>
        /// <param name="damage">weapon's block damage</param>
        /// <returns>true if block durability is < 0</returns>
        private bool DamageBlock(int3 globalBlockPosition, BlockType blockType, float damage)
        {
            float durability = GetBlockDurability(globalBlockPosition, blockType);
            durability -= damage;

            if (durability > 0)
                SetBlockDurability(globalBlockPosition, durability);
            else
                damagedBlocksDict.Remove(globalBlockPosition);

            return durability <= 0;
        }


        /// <summary>
        /// Get block durability and add block to damagedBlocksDict
        /// </summary>
        /// <param name="globalBlockPosition">global block position (in world space)</param>
        /// <param name="blockType">type of block</param>
        /// <returns>block durability</returns>
        private float GetBlockDurability(int3 globalBlockPosition, BlockType blockType)
        {
            if (damagedBlocksDict.ContainsKey(globalBlockPosition))
                return damagedBlocksDict[globalBlockPosition];
            else
            {
                float durability = WorldData.GetBlockDurability(blockType);
                damagedBlocksDict.Add(globalBlockPosition, durability);

                // remove first objects if count is too big
                if (damagedBlocksDict.Count > maxDamagedBlocksCount)
                {
                    for (int i = 0; i < damagedBlocksDict.Count - maxDamagedBlocksCount + 1; i++)
                    {
                        damagedBlocksDict.Remove(damagedBlocksDict.Keys.First());
                    }
                }

                return durability;
            }
        }

        /// <summary>
        /// Set block durability
        /// </summary>
        /// <param name="globalBlockPosition">global block position (in world space)</param>
        /// <param name="durability">new durability</param>
        private void SetBlockDurability(int3 globalBlockPosition, float durability)
        {
            if (damagedBlocksDict.ContainsKey(globalBlockPosition))
                damagedBlocksDict[globalBlockPosition] = durability;
            else
                damagedBlocksDict.Add(globalBlockPosition, durability);

        }
    }
}
