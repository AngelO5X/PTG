using UnityEngine;

[ExecuteAlways]   // ← TO JEST KLUCZ
public class ArchipelagoGenerator : MonoBehaviour
{
    [Header("World")]
    public int chunkSize = 64;
    public int chunks = 4;
    public Material terrainMaterial;


    [Header("Noise")]
    public float scale = 40f;
    public int seed = 12345;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Height")]
    public float heightMultiplier = 10f;

    [Header("Auto")]
    public bool autoUpdate = true;

    void OnEnable()
    {
        Generate();
    }

    void OnValidate()
    {
        if (!autoUpdate) return;
        Generate();
    }

    void Start()
    {
        Generate(); // Play Mode
    }

    public void Generate()
    {
        Clear();

        int worldSize = chunkSize * chunks;

        float[,] noise = NoiseGenerator.GenerateNoise(
            worldSize, scale, seed, octaves, persistence, lacunarity
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

                mr.sharedMaterial = terrainMaterial;

                mf.sharedMesh = MeshGenerator.GenerateTerrainMesh(chunkMap, heightMultiplier);
            }
        }
    }

    void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    float[,] ExtractChunk(float[,] map, int cx, int cy)
    {
        float[,] chunk = new float[chunkSize, chunkSize];
        int startX = cx * chunkSize;
        int startY = cy * chunkSize;

        for (int y = 0; y < chunkSize; y++)
            for (int x = 0; x < chunkSize; x++)
                chunk[x, y] = map[startX + x, startY + y];

        return chunk;
    }
}
