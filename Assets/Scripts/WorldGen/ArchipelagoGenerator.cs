using UnityEngine;
using Mirror;

public class ArchipelagoGenerator : NetworkBehaviour
{
    [Header("World")]
    public int chunkSize = 64;
    public int chunks = 4;
    public Material terrainMaterial;

    [Header("Noise")]
    public float scale = 40f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Height")]
    public float heightMultiplier = 10f;

    [SyncVar]
    public int seed;

    public override void OnStartServer()
    {
        seed = Random.Range(0, int.MaxValue);
        Generate(seed);
    }

    public override void OnStartClient()
    {
        Generate(seed);
    }

    void Generate(int usedSeed)
    {
        Clear();

        int worldSize = chunkSize * chunks;

        float[,] noise = NoiseGenerator.GenerateNoise(
            worldSize, scale, usedSeed, octaves, persistence, lacunarity
        );

        float[,] falloff = FalloffGenerator.GenerateFalloffMap(worldSize);

        for (int y = 0; y < worldSize; y++)
            for (int x = 0; x < worldSize; x++)
                noise[x, y] = Mathf.Clamp01(noise[x, y] - falloff[x, y]);

        for (int cy = 0; cy < chunks; cy++)
        {
            for (int cx = 0; cx < chunks; cx++)
            {
                float[,] chunkMap = ExtractChunk(noise, cx, cy);

                GameObject chunk = new GameObject($"Chunk_{cx}_{cy}");
                chunk.transform.parent = transform;
                chunk.transform.localPosition = new Vector3(
                    cx * chunkSize, 0, cy * chunkSize
                );

                MeshFilter mf = chunk.AddComponent<MeshFilter>();
                MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
                MeshCollider mc = chunk.AddComponent<MeshCollider>();

                mr.sharedMaterial = terrainMaterial;

                Mesh mesh = MeshGenerator.GenerateTerrainMesh(chunkMap, heightMultiplier);

                mf.sharedMesh = mesh;
                mc.sharedMesh = mesh;
            }
        }
    }

    void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    float[,] ExtractChunk(float[,] map, int cx, int cy)
    {
        int size = chunkSize + 2;
        float[,] chunk = new float[size, size];

        int startX = cx * chunkSize - 1;
        int startY = cy * chunkSize - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int mx = Mathf.Clamp(startX + x, 0, map.GetLength(0) - 1);
                int my = Mathf.Clamp(startY + y, 0, map.GetLength(1) - 1);
                chunk[x, y] = map[mx, my];
            }
        }

        return chunk;
    }
}
