using System;
using System.Collections;
using UnityEngine;
using VoxelTG.Player.Inventory;
using static VoxelTG.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Extensions
{
    public static class Extension
    {
        public static int ChunkIndexUp(this int index, int up)
        {
            return index + up * FixedChunkSizeXZ;
        }

        public static bool IsNullOrEmpty(this InventorySlot slot)
        {
            return slot == null || slot.IsEmpty();
        }

        /// <summary>
        /// Call action with 1 frame delay
        /// </summary>
        public static void OneFrameDelay(this MonoBehaviour mono, Action action)
        {
            if (action == null)
                return;

            mono.StartCoroutine(waitOneFrame(action));
        }

        private static IEnumerator waitOneFrame(Action action)
        {
            yield return null;
            action.Invoke();
        }
    }
}

