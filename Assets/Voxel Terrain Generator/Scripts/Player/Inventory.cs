using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
public class Inventory : MonoBehaviour
{
    [SerializeField] private Text materialListText;

    [SerializeField] private BlockType[] blockTypes;
    int currentBlockType;

    private void Start()
    {
        string text = "Materials:\n";
        for (int i = 0; i < blockTypes.Length; i++)
        {
            text += $"{i+1} : {blockTypes[i].ToString()}\n";
        }

        materialListText.text = text;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            currentBlockType = -1;
        for (int i = 0; i < blockTypes.Length; i++)
        {
            if (Input.GetKeyDown((KeyCode)49 + i))
                currentBlockType = i;
        }
    }

    public BlockType GetCurrentBlock()
    {
        if (currentBlockType < 0)
            return BlockType.AIR;
        return blockTypes[currentBlockType];
    }
}
