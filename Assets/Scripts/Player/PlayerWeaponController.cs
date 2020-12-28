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
        [System.NonSerialized]
        public bool isWeaponEquipped = false;

        [SerializeField] private LayerMask groundLayer;
        public float bulletDistance = 100;

        private Transform cameraTransform;
        private InventoryItemWeapon weaponInHand;

        private WeaponEffectsController currentWeaponFX;
        private float timeToNextShoot = 0;

        [SerializeField] private int maxDamagedBlocksCount;
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

            if (!newContent.IsNullOrEmpty() && newContent.Item.IsWeapon())
            {
                isWeaponEquipped = true;
                weaponInHand = (InventoryItemWeapon)newContent.Item;
                // TODO: read settings etc.
            }
            else
                isWeaponEquipped = false;
        }

        private void OnHandObjectLoaded(GameObject handObject, ItemType itemType)
        {
            if (weaponInHand.Type == itemType)
            {
                currentWeaponFX = handObject.GetComponent<WeaponEffectsController>();
            }
            else
            {
                Debug.LogWarning("Current weapon item type is different than current hand object item type", this);
            }
        }

        private void Update()
        {
            if (!UIManager.IsUiModeActive)
            {
                if (!isWeaponEquipped || weaponInHand == null) return;

                HandleInput();
            }
        }

        private void HandleInput()
        {
            if (timeToNextShoot <= 0)
            {
                bool shootButtonDown = weaponInHand.isAutomaticRifle ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
                if (shootButtonDown)
                {
                    timeToNextShoot = 1f / weaponInHand.fireRate;
                    currentWeaponFX.OnShoot();
                    //SoundManager.Instance.PlaySound(SoundType.RIFLE_AK74_SHOOT, transform.position, SoundSettings.DEFAULT);

                    if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, bulletDistance, groundLayer))
                    {
                        if (hitInfo.transform.CompareTag("Terrain"))
                        {
                            Vector3 pointInTargetBlock;

                            // move towards block position
                            pointInTargetBlock = hitInfo.point + cameraTransform.forward * .01f;

                            //SoundManager.Instance.PlaySound(SoundType.DESTROY_WOOD, pointInTargetBlock, SoundSettings.DEFAULT);

                            // get block & chunk
                            int3 globalBlockPosition = new int3(
                                Mathf.FloorToInt(pointInTargetBlock.x),
                                Mathf.FloorToInt(pointInTargetBlock.y),
                                Mathf.FloorToInt(pointInTargetBlock.z)
                            );

                            BlockPosition blockPosition = new BlockPosition(globalBlockPosition);
                            Chunk chunk = World.GetChunk(globalBlockPosition.x, globalBlockPosition.z);
                            BlockType blockType = chunk.GetBlock(blockPosition);

                            currentWeaponFX.OnBulletHitTerrain(hitInfo, chunk, blockPosition, blockType);

                            // cannot destroy base layer (at y == 0)
                            if (globalBlockPosition.y > 0)
                            {
                                bool shouldBeDestroyed = DamageBlock(globalBlockPosition, blockType, weaponInHand.blockDamage);
                                if (shouldBeDestroyed)
                                {
                                    chunk.SetBlock(blockPosition, BlockType.AIR, new SetBlockSettings(true, false, false, 10));
                                }
                            }
                        }
                        if (hitInfo.transform.tag.EndsWith("entity"))
                        {
                            Entity entity = hitInfo.transform.GetComponent<Entity>();
                            entity.Damage(weaponInHand.damage);
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
