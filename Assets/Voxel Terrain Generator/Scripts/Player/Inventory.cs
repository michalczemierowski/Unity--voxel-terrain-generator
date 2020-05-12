using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        for (int i = 0; i < blockTypes.Length; i++)
        {
            if (Input.GetKeyDown((KeyCode)49 + i))
                currentBlockType = i;
        }
    }

    public BlockType GetCurrentBlock()
    {
        return blockTypes[currentBlockType];
    }
}
