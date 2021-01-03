/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    public interface IMetadataProperty
    {
        string Key { get; }
        string ValueStr { get; }
    }

    public struct MetadataProperty<T> : IMetadataProperty where T : unmanaged
    {
        public string Key { get; }
        public T Value { get; }
        public string ValueStr { get => Value.ToString(); }

        public MetadataProperty(string key, T value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
