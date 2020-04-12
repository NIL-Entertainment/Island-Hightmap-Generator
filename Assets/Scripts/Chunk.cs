using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;

    GameObject ChunkObject;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    World world;
    int vertIndex = 0;
    List<Vector3> vertecies = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    bool _isActive;
    public bool populated = false;

    public Chunk(World _world, ChunkCoord _coord, bool generateOnLoad)
    {
        world = _world;
        coord = _coord;
        if (generateOnLoad)
            init();
    }

    public void init()
    {
        ChunkObject = new GameObject();
        meshFilter = ChunkObject.AddComponent<MeshFilter>();
        meshRenderer = ChunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;
        ChunkObject.transform.SetParent(world.transform);
        ChunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0, coord.z * VoxelData.ChunkWidth);
        ChunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        PopulateVoxelMap();
        UpdateChunk();
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x,y,z) + position);
                }
            }
        }
        populated = true;
    }

    public void EditVoxel(Vector3 pos, byte id)
    {
        Vector3Int v3i = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
        v3i.x -= (int)position.x;
        v3i.z -= (int)position.z;

        voxelMap[v3i.x, v3i.y, v3i.z] = id;

        UpdateVoxels(pos);

        UpdateChunk();
    }

    void UpdateVoxels(Vector3 pos)
    {
        Vector3 thisVoxel = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];
            if (!IsVoxelInChunk(currentVoxel))
            {
                //world.ChunkFromVector3(currentVoxel).UpdateChunk();
            }
        }
    }

    public void UpdateChunk()
    {
        while(modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;
        }

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (world.blockdata[voxelMap[x, y, z]].Render)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        CreateMesh();
    }

    void UpdateMeshData(Vector3 pos)
    {
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        for (int p = 0; p < 6; p++)
        {
            if (world.blockdata[blockID].Solid) {
                if (!CheckVoxelS(pos + VoxelData.faceChecks[p]))
                {

                    /*if ((bool)world.blockdata[blockID].GetDataTag("SmallModel", "bx"))
                    {
                        vertecies.Add(pos + VoxelData.voxelVertsPower[VoxelData.voxelTris[p, 0]]);
                        vertecies.Add(pos + VoxelData.voxelVertsPower[VoxelData.voxelTris[p, 1]]);
                        vertecies.Add(pos + VoxelData.voxelVertsPower[VoxelData.voxelTris[p, 2]]);
                        vertecies.Add(pos + VoxelData.voxelVertsPower[VoxelData.voxelTris[p, 3]]);
                    }
                    else*/
                    {
                        vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 0]]);
                        vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 1]]);
                        vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 2]]);
                        vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 3]]);
                    }

                    AddTexture(world.blockdata[blockID].GetTextureID(p));

                    triangles.Add(vertIndex);
                    triangles.Add(vertIndex + 1);
                    triangles.Add(vertIndex + 2);
                    triangles.Add(vertIndex + 2);
                    triangles.Add(vertIndex + 1);
                    triangles.Add(vertIndex + 3);

                    vertIndex += 4;
                }
            }
            else
            {
                if (!CheckVoxelR(pos + VoxelData.faceChecks[p]))
                {

                    vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 0]]);
                    vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 1]]);
                    vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 2]]);
                    vertecies.Add(pos + VoxelData.voxelverts[VoxelData.voxelTris[p, 3]]);

                    AddTexture(world.blockdata[blockID].GetTextureID(p));

                    triangles.Add(vertIndex);
                    triangles.Add(vertIndex + 1);
                    triangles.Add(vertIndex + 2);
                    triangles.Add(vertIndex + 2);
                    triangles.Add(vertIndex + 1);
                    triangles.Add(vertIndex + 3);

                    vertIndex += 4;
                }
            }
        }
    }

    public byte VoxelFromVector3(Vector3 pos)
    {
        Vector3Int v3i = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
        v3i.x -= (int)position.x;
        v3i.z -= (int)position.z;

        return voxelMap[v3i.x, v3i.y, v3i.z];
    }

    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; if (ChunkObject != null) ChunkObject.SetActive(value); }
    }

    public Vector3 position
    {
        get { return ChunkObject.transform.position; }
    }

    bool CheckVoxelR(Vector3 pos)
    {
        if (!IsVoxelInChunk(pos))
            return world.CheckRender(pos + position);

        Vector3Int v3i = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

        return world.blockdata[voxelMap[v3i.x, v3i.y, v3i.z]].Render;
    }

    bool CheckVoxelS(Vector3 pos)
    {
        if (!IsVoxelInChunk(pos))
            return world.CheckSolid(pos + position);

        Vector3Int v3i = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

        return world.blockdata[voxelMap[v3i.x, v3i.y, v3i.z]].Solid;
    }

    bool IsVoxelInChunk(Vector3 pos)
    {
        if (pos.x < 0 || pos.x > VoxelData.ChunkWidth - 1 || pos.y < 0 || pos.y > VoxelData.ChunkHeight - 1 || pos.z < 0 || pos.z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertecies.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        ClearMesh();
    }

    void ClearMesh()
    {
        vertIndex = 0;
        vertecies.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    void AddTexture(int TextureID)
    {
        float y = TextureID / VoxelData.TextureAtlasSize;
        float x = TextureID - (y * VoxelData.TextureAtlasSize);
        x *= VoxelData.NormalizedTextureSize;
        y *= VoxelData.NormalizedTextureSize;
        y = 1f - y - VoxelData.NormalizedTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedTextureSize, y + VoxelData.NormalizedTextureSize));
    }
}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(Vector3 pos)
    {
        Vector2Int v2i = new Vector2Int((int)pos.x,(int)pos.z);
        x = v2i.x / VoxelData.ChunkWidth;
        z = v2i.y / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        }
        else if (other.x == x && other.z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
