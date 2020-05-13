using System.Collections.Generic;

public interface IBlockUpdateListener
{
    BlockType GetBlockType();
    void OnBlockUpdate(BlockUpdateEventData data, Dictionary<Side, BlockUpdateEventData> neighbours);
}