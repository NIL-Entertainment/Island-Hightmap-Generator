using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public int seed;

    public Transform Player;
    public Vector3 SpawnPosition;
    public Material material;
    public Image img;
    //public BlockDataSingle[] blockdata;
    //public List<BlockDataSingle> bds = new List<BlockDataSingle>();

    public BlockDataSingle[] blockdata;
    public List<BlockDataSingle> bds = new List<BlockDataSingle>();

    public TextAsset jsonFile;
    public TextAsset jsonFile2;

    Chunk[,] Chunks = new Chunk[VoxelData.WorldSizeChunks, VoxelData.WorldSizeChunks];

    public Camera snapCam1;
    public Camera snapCam2;

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    List<ChunkCoord> ChunksToCreate = new List<ChunkCoord>();
    List<Chunk> ChunksToUpdate = new List<Chunk>();
    public ChunkCoord LastChunk;
    public ChunkCoord CurrentChunk;

    public float[,] biomedata = new float[9,3];
    public float[,] structuredata = new float[9,6];

    public int[,] heightmap = new int[VoxelData.WorldSizeBlocks, VoxelData.WorldSizeBlocks];
    public float[,] IslandMap = new float[VoxelData.WorldSizeBlocks, VoxelData.WorldSizeBlocks];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    bool isCreatingChunks;
    bool isUpdatingChunks;

    public Snap snap;

    private void Start()
    {
        // Load BlockData from Json
        BlockData BlockInJson = JsonUtility.FromJson<BlockData>(jsonFile.text);
        foreach (BlockDataSingle bd in BlockInJson.blockdata)
        {
            bds.Add(bd);
            string TIDList = bd.GetTextureID(0).ToString();
            for (int i = 1; i < 6; i++)
            {
                TIDList = TIDList + ", " + bd.GetTextureID(i);
            }
            Debug.Log("Found Block: " + bd.Name + " Solid: " + bd.Render + " TID: " + TIDList + " EBD: " + bd.ExtraBlockData.Length);
        }
        blockdata = bds.ToArray();

        // Load BiomeData from Json
        BiomeData BiomeInJson = JsonUtility.FromJson<BiomeData>(jsonFile2.text);
        int index = 0;
        foreach (BiomeDataSingle bd in BiomeInJson.biomedata)
        {
            Debug.Log("Found Biome: " + bd.Name + " SGH: " + bd.SolidGroundHeight + " TH: " + bd.TerrainHeight + " TS: " + bd.TerrainScale);
            biomedata[index, 0] = bd.SolidGroundHeight;
            biomedata[index, 1] = bd.TerrainHeight;
            biomedata[index, 2] = bd.TerrainScale;
            structuredata[index, 0] = bd.StructureScale;
            structuredata[index, 1] = bd.StructureThreshold;
            structuredata[index, 2] = bd.StructurePlacementScale;
            structuredata[index, 3] = bd.StructurePlacementThreshold;
            structuredata[index, 4] = bd.maxStructureHeight;
            structuredata[index, 5] = bd.minStructureHeight;
            index++;
        }
        blockdata = bds.ToArray();

        // Actual Game-Stuff

        LoadHeightmap();
        LoadIslandMap();
        heightmap = ArrayMultiply(heightmap, IslandMap);

        Random.InitState(seed);

        SpawnPosition = new Vector3(VoxelData.WorldSizeBlocks / 2, VoxelData.ChunkHeight - 50, VoxelData.WorldSizeBlocks / 2);
        GenerateWorld();
        LastChunk = getChunkCoord(Player.position);

        //snap.setupsnap(this);
        //StartCoroutine(Iconset());
    }

    void LoadHeightmap()
    {
        for (int x = 0; x < VoxelData.WorldSizeBlocks; x++)
        {
            for (int y = 0; y < VoxelData.WorldSizeBlocks; y++)
            {
                heightmap[x,y] = (int)((Noise.GetTerrainPerlin(new Vector2(x, y), 500, biomedata[0, 2]) * biomedata[0, 1]) + biomedata[0, 0]);
            }
        }
    }

    void LoadIslandMap()
    {
        Texture2D t2d = new Texture2D(VoxelData.WorldSizeBlocks, VoxelData.WorldSizeBlocks);
        for (int x = -(VoxelData.WorldSizeBlocks / 2); x < VoxelData.WorldSizeBlocks / 2; x++)
        {
            for (int y = -(VoxelData.WorldSizeBlocks / 2); y < VoxelData.WorldSizeBlocks / 2; y++)
            {
                float valx = 0;
                float valy = 0;

                if(x <= 0)
                {
                    valx = (x + VoxelData.WorldSizeBlocks / 2f) / (VoxelData.WorldSizeBlocks / 2f);
                }
                else
                {
                    valx = (VoxelData.WorldSizeBlocks / 2f - x) / (VoxelData.WorldSizeBlocks / 2f);
                }

                if (y <= 0)
                {
                    valy = (y + VoxelData.WorldSizeBlocks / 2f) / (VoxelData.WorldSizeBlocks / 2f);
                }
                else
                {
                    valy = (VoxelData.WorldSizeBlocks / 2f - y) / (VoxelData.WorldSizeBlocks / 2f);
                }

                float outpt = (valx + valy) / 2 * 1.5f;

                t2d.SetPixel(x + VoxelData.WorldSizeBlocks / 2, y + VoxelData.WorldSizeBlocks / 2, new Color(0, 0, 0));

                IslandMap[x + VoxelData.WorldSizeBlocks / 2, y + VoxelData.WorldSizeBlocks / 2] = outpt;
            }
        }

        img.sprite = Sprite.Create(t2d, new Rect(Vector2.zero, new Vector2(t2d.width, t2d.height)),Vector2.zero);
    }

    int[,] ArrayMultiply(int[,] hm, float[,] mm)
    {
        int[,] arr = hm;
        for (int x = 0; x < VoxelData.WorldSizeBlocks; x++)
        {
            for (int y = 0; y < VoxelData.WorldSizeBlocks; y++)
            {
                arr[x, y] = Mathf.FloorToInt(hm[x,y] * mm[x,y]);
            }
        }
        return arr;
    }

    private void Update()
    {
        CurrentChunk = getChunkCoord(Player.position);

        if (!CurrentChunk.Equals(LastChunk))
            CheckViewDistance();

        LastChunk = CurrentChunk;

        if (ChunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine(CreateChunks());
        }
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeChunks / 2) - VoxelData.ViewDistance; x < (VoxelData.WorldSizeChunks / 2) + VoxelData.ViewDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeChunks / 2) - VoxelData.ViewDistance; z < (VoxelData.WorldSizeChunks / 2) + VoxelData.ViewDistance; z++)
            {
                Chunks[x, z] = new Chunk(this, new ChunkCoord(x,z), true);
                activeChunks.Add(new ChunkCoord(x,z));
            }
        }
        
        Chunks[VoxelData.WorldSizeChunks / 2, VoxelData.WorldSizeChunks / 2] = new Chunk(this, new ChunkCoord(VoxelData.WorldSizeChunks / 2, VoxelData.WorldSizeChunks / 2), true);
        if (modifications.Count > 0 && !isUpdatingChunks)
            StartCoroutine(UpdateChunk());

        Player.position = SpawnPosition;
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (ChunksToCreate.Count > 0)
        {
            Chunks[ChunksToCreate[0].x, ChunksToCreate[0].z].init();
            ChunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    IEnumerator UpdateChunk()
    {
        isUpdatingChunks = true;

        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();

            ChunkCoord c = getChunkCoord(v.position);
            if (Chunks[c.x, c.z] == null)
            {
                Chunks[c.x, c.z] = new Chunk(this, c, true);
                activeChunks.Add(c);
            }
            Chunks[c.x, c.z].modifications.Enqueue(v);

            if (!ChunksToUpdate.Contains(Chunks[c.x, c.z]))
            {
                ChunksToUpdate.Add(Chunks[c.x, c.z]);
            }
        }

        for (int i = 0; i < ChunksToUpdate.Count; i++)
        {
            ChunksToUpdate[0].UpdateChunk();
            ChunksToUpdate.RemoveAt(0);
            yield return null;
        }

        isUpdatingChunks = false;
    }

    ChunkCoord getChunkCoord(Vector3 pos)
    {
        Vector3Int v3i = new Vector3Int((int)pos.x / VoxelData.ChunkWidth, (int)pos.y, (int)pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(v3i.x,v3i.z);
    }

    public Chunk ChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return Chunks[x,z];
    }

    public bool CheckSolid(Vector3 pos)
    {
        Vector3Int v3i = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        Vector2Int v2i = new Vector2Int(v3i.x / VoxelData.ChunkWidth, v3i.z / VoxelData.ChunkWidth);

        if (!IsVoxelInWorld(pos))
        {
            return false;
        }
        else
        {
            if (Chunks[v2i.x,v2i.y] != null && Chunks[v2i.x,v2i.y].populated) {
                v3i.x -= (v2i.x * VoxelData.ChunkWidth);
                v3i.z -= (v2i.y * VoxelData.ChunkWidth);

                return blockdata[Chunks[v2i.x, v2i.y].voxelMap[v3i.x, v3i.y, v3i.z]].Solid;
            }
            return blockdata[GetVoxel(pos)].Solid;
        }
    }

    public bool CheckRender(Vector3 pos)
    {
        Vector3Int v3i = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        Vector2Int v2i = new Vector2Int(v3i.x / VoxelData.ChunkWidth, v3i.z / VoxelData.ChunkWidth);

        if (!IsVoxelInWorld(pos))
        {
            return false;
        }
        else
        {
            if (Chunks[v2i.x, v2i.y] != null && Chunks[v2i.x, v2i.y].populated)
            {
                v3i.x -= (v2i.x * VoxelData.ChunkWidth);
                v3i.z -= (v2i.y * VoxelData.ChunkWidth);

                return blockdata[Chunks[v2i.x, v2i.y].voxelMap[v3i.x, v3i.y, v3i.z]].Render;
            }
            return blockdata[GetVoxel(pos)].Render;
        }
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = getChunkCoord(Player.position);

        List<ChunkCoord> prevChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.ViewDistance; x < coord.x + VoxelData.ViewDistance; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistance; z < coord.z + VoxelData.ViewDistance; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    if (Chunks[x,z] == null)
                    {
                        Chunks[x, z] = new Chunk(this, new ChunkCoord(x, z), false);
                        ChunksToCreate.Add(new ChunkCoord(x,z));
                    }
                    else if (!Chunks[x,z].IsActive)
                    {
                        Chunks[x, z].IsActive = true;
                        
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                }

                for (int i = 0; i < prevChunks.Count; i++)
                {
                    if (prevChunks[i].Equals(new ChunkCoord(x,z)))
                    {
                        prevChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (ChunkCoord c in prevChunks)
        {
            Chunks[c.x, c.z].IsActive = false;
            activeChunks.Remove(c);
        }

        if (modifications.Count > 0 && !isUpdatingChunks)
            StartCoroutine(UpdateChunk());

    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = (int)pos.y;

        if (!IsVoxelInWorld(pos))
            return 0;

         //Global Terrain
        
        if (yPos < 2)
        {
            return 4;
        }

        // Basic Terrain
        int Py = heightmap[(int)pos.x, (int)pos.z];

        byte VoxelValue = 0;

        if (yPos > Py)
        {
            if (yPos > 50)
                VoxelValue = 0;
            else
                VoxelValue = 6;
        }
        if (yPos == Py)
        {
            if (yPos > 50)
            {
                VoxelValue = 1;
            }
            else
                VoxelValue = 7;
        }
        if (yPos < Py && yPos >= Py - 5)
            VoxelValue = 2;
        if (yPos < Py - 5)
            VoxelValue = 3;

        // Structures

        //   TREE
        if (yPos == Py && Py > 50)
        {
            if (Noise.TreePerlin(new Vector2(pos.x + seed, pos.z), 100, 100))
            {
                    //VoxelValue = 5;
                    Structure.MakeTree(pos, modifications, (int)structuredata[0,5], (int)structuredata[0,4]);
            }
        }
        
        return VoxelValue;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeChunks - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x > 0 && pos.x < VoxelData.WorldSizeBlocks && pos.z > 0 && pos.z < VoxelData.WorldSizeBlocks && pos.y > 0 && pos.y < VoxelData.ChunkHeight)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 pos, byte ID)
    {
        position = pos;
        id = ID;
    }
}

[System.Serializable]
public class BlockDataSingle
{
    public string Name;
    public bool Render;
    public bool Solid;
    public int TIDBack;
    public int TIDFront;
    public int TIDTop;
    public int TIDBottom;
    public int TIDLeft;
    public int TIDRight;

    public ExtraData[] ExtraBlockData;

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return TIDBack;
            case 1:
                return TIDFront;
            case 2:
                return TIDTop;
            case 3:
                return TIDBottom;
            case 4:
                return TIDLeft;
            case 5:
                return TIDRight;
            default:
                Debug.Log("TID Error");
                return 0;
        }
    }
}
[System.Serializable]
public class BlockData
{
    public BlockDataSingle[] blockdata;
}
[System.Serializable]
public class ExtraData
{
    public string key;
    public int value;
}
[System.Serializable]
public class BiomeDataSingle
{
    public string Name;
    public float SolidGroundHeight;
    public float TerrainHeight;
    public float TerrainScale;

    public float StructureScale;
    public float StructureThreshold;
    public float StructurePlacementScale;
    public float StructurePlacementThreshold;
    public float maxStructureHeight;
    public float minStructureHeight;
}
[System.Serializable]
public class BiomeData
{
    public BiomeDataSingle[] biomedata;
}

/*[System.Serializable]
public class BlockData
{
    public string name;
    public bool render;
    public bool transparent;
    public float transparency;

    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public string[] EBT;

    public Dictionary<string, string> EBTData;

    public int[,] BlockStateTextures;

    public string Name
    {
        get { return name; }
    }
    public bool Render
    {
        get { return render; }
    }
    public bool Solid
    {
        get { return render; }
    }

    public BlockData()
    {
        name = "";
        render = false;
        transparency = 0;
        transparent = false;

        backFaceTexture = 0;
        frontFaceTexture = 0;
        topFaceTexture = 0;
        bottomFaceTexture = 0;
        leftFaceTexture = 0;
        rightFaceTexture = 0;
    }

    public int GetTextureID(int faceIndex, int blockstate)
    {
        if (blockstate == 0)
        {
            switch (faceIndex)
            {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Debug.Log("TID Error");
                    return 0;
            }
        }
        else
        {
            return BlockStateTextures[blockstate - 1, faceIndex];
        }
    }
    public object GetDataTag(string key, string DataType)
    {
        if (DataType == "string")
        {
            if (EBTData.ContainsKey(key))
                return EBTData[key];
            else
                return "";
        }
        if (DataType == "int")
        {
            if (EBTData.ContainsKey(key))
                return int.Parse(EBTData[key]);
            else
                return 0;
        }
        if (DataType == "float")
        {
            if (EBTData.ContainsKey(key))
                return float.Parse(EBTData[key]);
            else
                return 0;
        }
        if (DataType == "bool")
        {
            if (EBTData.ContainsKey(key))
                return bool.Parse(EBTData[key]);
            else
                return false;
        }
        if (DataType == "bx")
        {
            return EBTData.ContainsKey(key);
        }
        return null;
    }
    public void LoadDataTags()
    {
        EBTData = new Dictionary<string, string>();
        for (int i = 0; i < EBT.Length; i++)
        {
            string[] data;

            if (EBT[i].Contains(":"))
                data = EBT[i].Split(':');
            else
            {
                data = new string[1];
                data[0] = EBT[i];
            }

            if (data.Length > 1)
            {
                EBTData.Add(data[0], data[1]);
            }
            else
            {
                EBTData.Add(data[0], "True");
            }
        }

        BlockStateTextures = new int[0, 0];

        if (EBTData.ContainsKey("StateTex"))
        {
            string[] overArray;

            if (EBTData["StateTex"].Contains("|"))
                overArray = EBTData["StateTex"].Split('|');
            else
            {
                overArray = new string[1];
                overArray[0] = EBTData["StateTex"];
            }

            int[,] values = new int[overArray.Length, 6];

            for (int i = 0; i < overArray.Length; i++)
            {
                string[] underArray = overArray[i].Split(',');
                for (int j = 0; j < underArray.Length; j++)
                {
                    values[i, j] = int.Parse(underArray[j]);
                }
            }
            BlockStateTextures = values;
        }
    }
}
[System.Serializable]
public class BlockDataHolder
{
    public BlockData[] BlockTypeData;
}*/