using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static void MakeTree(Vector3 pos, Queue<VoxelMod> q, int minH, int maxH)
    {
        for (int x = -2; x < 3; x++)
        {
            for (int z = -2; z < 3; z++)
            {
                q.Enqueue(new VoxelMod(pos + new Vector3(x, 2, z), 9));
                q.Enqueue(new VoxelMod(pos + new Vector3(x, 3, z), 9));
            }
        }
        for (int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                q.Enqueue(new VoxelMod(pos + new Vector3(x, 4, z), 9));
            }
        }

        q.Enqueue(new VoxelMod(pos + new Vector3(0, 5, 0), 9));
        q.Enqueue(new VoxelMod(pos + new Vector3(1, 5, 0), 9));
        q.Enqueue(new VoxelMod(pos + new Vector3(0, 5, 1), 9));
        q.Enqueue(new VoxelMod(pos + new Vector3(-1, 5, 0), 9));
        q.Enqueue(new VoxelMod(pos + new Vector3(0, 5, -1), 9));

        for (int y = 0; y < 5; y++)
        {
            q.Enqueue(new VoxelMod(pos + new Vector3(0, y, 0), 8));
        }
    }
}
