using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static float GetTerrainPerlin(Vector2 pos, float offset, float scale)
    {
        float noise1 = Mathf.PerlinNoise((pos.x + 0.1f) / VoxelData.ChunkWidth * (scale / 50) + offset, (pos.y + 0.1f) / VoxelData.ChunkWidth * (scale / 50) + offset);
        float noise2 = Mathf.PerlinNoise((pos.x + 0.1f) / VoxelData.ChunkWidth * (scale * 4) + (offset * 3), (pos.y + 0.1f) / VoxelData.ChunkWidth * (scale * 4) + (offset * 3));
        float noise3 = Mathf.PerlinNoise((pos.x + 0.1f) / VoxelData.ChunkWidth * (scale * 4.5f) + (offset * 6), (pos.y + 0.1f) / VoxelData.ChunkWidth * (scale * 4.5f) + (offset * 6));

        return noise2 * noise1 * noise3;
    }

    public static float Get2DPerlin(Vector2 pos, float offset, float scale)
    {
        float noise1 = Mathf.PerlinNoise((pos.x + 0.1f) / VoxelData.ChunkWidth * scale + offset, (pos.y + 0.1f) / VoxelData.ChunkWidth * scale + offset);

        return noise1;
    }
}
