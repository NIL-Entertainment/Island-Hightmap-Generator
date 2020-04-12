using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static void MakeTree(Vector3 pos, Queue<VoxelMod> q, int minH, int maxH)
    {
        int height = (int)(maxH * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 1337f, 3f));
        if (height < minH)
            height = minH;

        for (int x = (int)pos.x - 2; x < (int)pos.x + 2; x++)
        {
            for (int y = height - 1; y < height + 1; y++)
            {
                for (int z = (int)pos.z - 2; z < (int)pos.z + 2; z++)
                {
                    q.Enqueue(new VoxelMod(new Vector3(x, y, z), 9));
                }
            }
        }

        for (int i = 1; i < height; i++)
        {
            q.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), 8));
        }
    }
}
