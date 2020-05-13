using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static FastNoise;

[System.Serializable]
public struct GeneratorSettings
{
    public NoiseSettings noiseSettings;
    [Space(15)]
    public float baseLandHeightMultipler;
    public float chanceForGrass;
    public float heightMapMultipler;
    [Space(15)]
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

[System.Serializable]
public struct NoiseSettings
{
    public float frequency;
    public Interp interp;
    public NoiseType noiseType;

    public int octaves;
    public float lancuarity;
    public float gain;
    public FractalType fractalType;
}