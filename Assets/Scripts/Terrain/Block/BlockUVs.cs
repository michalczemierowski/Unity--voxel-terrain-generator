using Unity.Mathematics;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    /// <summary>
    /// Struct used to generate terrain mesh
    /// </summary>
    public struct BlockUVs
    {
        public const float textureSize = 16f;
        public float2 uv0, uv1, uv2, uv3;

        public static readonly BlockUVs AIR_UV = new BlockUVs
        {
            uv0 = float2.zero,
            uv1 = float2.zero,
            uv2 = float2.zero,
            uv3 = float2.zero
        };

        #region blocks uvs

        public static readonly BlockUVs DIRT_UV = CreateUVs(0, 0);
        public static readonly BlockUVs SAND_UV = CreateUVs(2, 0);
        public static readonly BlockUVs GRASS_BLOCK_TOP_UV = CreateUVs(1, 0);
        public static readonly BlockUVs GRASS_BLOCK_SIDE_UV = CreateUVs(0, 1);
        public static readonly BlockUVs STONE_UV = CreateUVs(0, 2);
        public static readonly BlockUVs COBBLESTONE_UV = CreateUVs(1, 2);
        public static readonly BlockUVs OBSIDIAN_UV = CreateUVs(2, 2);
        public static readonly BlockUVs OAK_LOG_TOP_UV = CreateUVs(0, 4);
        public static readonly BlockUVs SPRUCE_LOG_TOP_UV = CreateUVs(1, 4);
        public static readonly BlockUVs OAK_LOG_SIDE_UV = CreateUVs(0, 3);
        public static readonly BlockUVs SPRUCE_LOG_SIDE_UV = CreateUVs(1, 3);
        public static readonly BlockUVs OAK_LEAVES_UV = CreateUVs(0, 5);
        public static readonly BlockUVs SPRUCE_LEAVES_UV = CreateUVs(1, 5);

        #endregion

        #region liquids uvs

        // (liquids have different texture so textureSizeX = 1)
        public static readonly BlockUVs WATER_UV = CreateUVs(0, 0, textureSizeX: 1);

        #endregion

        #region plants uvs

        public static readonly BlockUVs GRASS_UV_0 = CreateUVs(3, 0);

        public static readonly BlockUVs GRASS_UV_1 = CreateUVs(3, 2);

        public static readonly BlockUVs GRASS_UV_2 = CreateUVs(3, 3);

        #endregion

        public static BlockUVs GetTileUVs(TextureTile tile)
        {
            switch (tile)
            {
                case TextureTile.DIRT:
                    return DIRT_UV;
                case TextureTile.SAND:
                    return SAND_UV;
                case TextureTile.GRASS_BLOCK_TOP:
                    return GRASS_BLOCK_TOP_UV;
                case TextureTile.GRASS_BLOCK_SIDE:
                    return GRASS_BLOCK_SIDE_UV;

                case TextureTile.STONE:
                    return STONE_UV;
                case TextureTile.COBBLESTONE:
                    return COBBLESTONE_UV;
                case TextureTile.OBSIDIAN:
                    return OBSIDIAN_UV;

                case TextureTile.OAK_LOG_SIDE:
                    return OAK_LOG_SIDE_UV;
                case TextureTile.OAK_LOG_TOP:
                    return OAK_LOG_TOP_UV;
                case TextureTile.OAK_LEAVES:
                    return OAK_LEAVES_UV;

                case TextureTile.SPRUCE_LOG_SIDE:
                    return SPRUCE_LOG_SIDE_UV;
                case TextureTile.SPRUCE_LOG_TOP:
                    return SPRUCE_LOG_TOP_UV;
                case TextureTile.SPRUCE_LEAVES:
                    return SPRUCE_LEAVES_UV;

                case TextureTile.WATER:
                    return WATER_UV;

                case TextureTile.GRASS_0:
                    return GRASS_UV_0;
                case TextureTile.GRASS_1:
                    return GRASS_UV_1;
                case TextureTile.GRASS_2:
                    return GRASS_UV_2;
                default:
                    return DIRT_UV;
            }
        }

        private static BlockUVs CreateUVs(int x, int y, float textureSizeX = textureSize, float textureSizeY = textureSize)
        {
            return new BlockUVs
            {
                uv0 = new float2(
                 x / textureSizeX + .001f,
                 y / textureSizeY + .001f),
                uv1 = new float2(
                 x / textureSizeX + .001f,
                 (y + 1) / textureSizeY - .001f),
                uv2 = new float2(
                 (x + 1) / textureSizeX - .001f,
                 (y + 1) / textureSizeY - .001f),
                uv3 = new float2(
                 (x + 1) / textureSizeX - .001f,
                 y / textureSizeY + .001f),
            };
        }
    }
}
