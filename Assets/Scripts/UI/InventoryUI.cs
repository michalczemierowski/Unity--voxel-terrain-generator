using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using VoxelTG.DebugUtils;
using VoxelTG.Entities.Items;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    // TODO: xml docs
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private ToolbarUI toolbarUI;
        [SerializeField] private TMP_Text currentItemName;

        [Header("Settings")]
        [SerializeField] private Color defaultToolbarBgColor;
        [SerializeField] private Color selectedToolbarBgColor;

        
    }
}
