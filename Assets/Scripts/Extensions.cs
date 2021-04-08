using System;
using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Player.Inventory;
using VoxelTG.Terrain.Blocks;
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

        public static byte GetParameterValue(this NativeHashMap<int, BlockParameter> map, int3 position, ParameterType parameterType)
        {
            if (map.TryGetValue(Utils.GetParameterIndex(position, parameterType), out BlockParameter result))
                return result.Value;

            return 0;
        }

        public static void SetParameterValue(this NativeHashMap<int, BlockParameter> map, int3 position, ParameterType parameterType, byte value)
        {
            map[Utils.GetParameterIndex(position, parameterType)] = new BlockParameter(parameterType, value);
        }

        public static bool TryGetParameterValue(this NativeHashMap<int, BlockParameter> map, int3 position, ParameterType parameterType, out byte value)
        {
            if (map.TryGetValue(Utils.GetParameterIndex(position, parameterType), out BlockParameter result))
            {
                value = result.Value;
                return true;
            }

            value = 0;
            return false;
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

