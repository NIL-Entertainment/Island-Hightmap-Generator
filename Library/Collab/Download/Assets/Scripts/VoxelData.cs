using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;

    public static readonly int WorldSizeChunks = 100;
    public static int WorldSizeBlocks
    {
        get { return WorldSizeChunks * ChunkWidth; }
    }


    public static readonly int TextureAtlasSize = 16;
    public static float NormalizedTextureSize
    {
        get
        {
            return 1f / (float)TextureAtlasSize;
        }
    }

    public static readonly int ViewDistance = 5;

    public static readonly Vector3[] voxelverts = new Vector3[8] {
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f),
        new Vector3(1.0f,1.0f,0.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(1.0f,0.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f),
        new Vector3(0.0f,1.0f,1.0f)
    };

    public static readonly Vector3[] voxelVertsPower = new Vector3[8] {
        new Vector3 (0.0f, 0.0f, 0.0f),
        new Vector3 (1.0f, 0.0f, 0.0f),
        new Vector3 (1.0f, 0.1f, 0.0f),
        new Vector3 (0.0f, 0.1f, 0.0f),
        new Vector3 (0.0f, 0.0f, 1.0f),
        new Vector3 (1.0f, 0.0f, 1.0f),
        new Vector3 (1.0f, 0.1f, 1.0f),
        new Vector3 (0.0f, 0.1f, 1.0f),
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f,0.0f,-1.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,-1.0f,0.0f),
        new Vector3(-1.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f)
    };

    public static readonly int[,] voxelTris = new int[6, 4]
    {
        {0,3,1,2 }, // Back Face
        {5,6,4,7 }, // Front Face
        {3,7,2,6 }, // Top Face
        {1,5,0,4 }, // Bottom Face
        {4,7,0,3 }, // Left Face
        {1,2,5,6 }  // Right Face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1)
    };
}
