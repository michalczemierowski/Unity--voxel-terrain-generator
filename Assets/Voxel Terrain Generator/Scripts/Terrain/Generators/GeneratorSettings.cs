using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public struct GeneratorSettings
{
    public float baseLandHeightMultipler;
    public float chanceForGrass;
    public float2 heightMapMultipler;
    public bool2 heighMapAbs;

    public BlockType plantsBlock;
    public BlockType topBlock;
    public BlockType belowBlock;
    // TODO
}

[System.Serializable]
public struct HeightMapStruct
{
    public byte end;
    public BlockType blockType;

    public HeightMapStruct(byte end, BlockType blockType)
    {
        this.end = end;
        this.blockType = blockType;
    }
}