using UnityEngine;
using Mirror;
using MicahW.PointGrass;

public class ArchipelagoGenerator : NetworkBehaviour
{
    [Header("World Settings")]
    public int chunkSize = 64;
    public int chunks = 4;
    public Material terrainMaterial;

    [Header("Noise Settings")]
    public float scale = 40f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Height Settings")]
    public float heightMultiplier = 10f;
    private float waterOffset = 0.2f;

    [Header("Grass Rules (Wysokoœæ)")]
    [Tooltip("Wysokoœæ, od której trawa ZACZYNA siê pojawiaæ (koniec pla¿y).")]
    public float grassMinHeight = 1.2f;

    [Tooltip("D³ugoœæ pasa przejœciowego (w metrach). Tyle metrów w górê trawa roœnie od ma³ej do du¿ej.")]
    public float grassFadeRange = 2.0f;

    [Header("Grass Rules (Klify)")]
    [Tooltip("Jak p³aski musi byæ teren? 1.0 = idealnie p³asko, 0.5 = œrednio stromo. Ustaw ok. 0.5-0.6.")]
    [Range(0f, 1f)]
    public float slopeLimit = 0.55f;

    [Tooltip("Zmiêkczenie granicy na klifach.")]
    public float slopeBlur = 0.1f;

    [Header("Grass Visuals")]
    [Tooltip("Wielkoœæ trawy na samym dole (przy pla¿y).")]
    public float minGrassScale = 0.4f;
    public float maxGrassScale = 1.0f;

    [Tooltip("Gêstoœæ na œrodku l¹du.")]
    public float grassDensityBase = 2.5f;
    [Tooltip("Gêstoœæ przy pla¿y (musi byæ gêsto, ¿eby zakryæ braki).")]
    public float grassDensityEdge = 8.0f;

    [Header("Mesh Settings")]
    public Mesh grassClumpMesh;
    public Material grassMaterial;

    [SyncVar]
    public int seed;

    private void Start()
    {
        if (Application.isPlaying)
        {
            if (!NetworkClient.active && !NetworkServer.active)
            {
                Generate(seed != 0 ? seed : Random.Range(0, 10000));
            }
        }
    }

    public override void OnStartServer()
    {
        seed = Random.Range(0, int.MaxValue);
        Generate(seed);
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active && NetworkClient.active) return;
        Generate(seed);
    }

    void Generate(int usedSeed)
    {
        Clear();

        int totalVerts = (chunkSize * chunks) + 1;
        int worldMetricSize = chunkSize * chunks;

        float[,] noise = NoiseGenerator.GenerateNoise(totalVerts, scale, usedSeed, octaves, persistence, lacunarity);
        float[,] falloff = FalloffGenerator.GenerateFalloffMap(totalVerts);

        for (int y = 0; y < totalVerts; y++)
            for (int x = 0; x < totalVerts; x++)
                noise[x, y] = Mathf.Clamp01(noise[x, y] - falloff[x, y]);

        for (int cy = 0; cy < chunks; cy++)
        {
            for (int cx = 0; cx < chunks; cx++)
            {
                GenerateSingleChunk(noise, cx, cy, worldMetricSize);
            }
        }
    }

    void GenerateSingleChunk(float[,] noiseMap, int cx, int cy, int worldMetricSize)
    {
        float[,] borderedMap = ExtractChunkWithBorder(noiseMap, cx, cy);
        int width = borderedMap.GetLength(0);
        int height = borderedMap.GetLength(1);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float h = borderedMap[x, y];
                if (h <= 0.001f) borderedMap[x, y] -= (waterOffset / heightMultiplier);
            }

        GameObject chunk = new GameObject($"Chunk_{cx}_{cy}");
        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(cx * chunkSize, 0, cy * chunkSize);
        chunk.layer = gameObject.layer;

        MeshFilter mf = chunk.AddComponent<MeshFilter>();
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        MeshCollider mc = chunk.AddComponent<MeshCollider>();

        mr.material = terrainMaterial;

        int uvOffsetX = cx * chunkSize;
        int uvOffsetY = cy * chunkSize;
        Mesh mesh = MeshGenerator.GenerateTerrainMesh(borderedMap, heightMultiplier, uvOffsetX, uvOffsetY, worldMetricSize);
        mesh.RecalculateBounds();

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Color[] colors = new Color[vertices.Length];
        System.Random prng = new System.Random(seed + cx * 100 + cy);

        float maxDensitySetting = Mathf.Max(grassDensityBase, grassDensityEdge);

        for (int i = 0; i < vertices.Length; i++)
        {
            float h = vertices[i].y;
            Vector3 normal = normals[i];

            // --- 1. WYSOKOŒÆ (HEIGHT) ---
            // Obliczamy gdzie jesteœmy wzglêdem pla¿y.
            // 0.0 = Dok³adnie na granicy (grassMinHeight)
            // 1.0 = W pe³ni na l¹dzie (grassMinHeight + grassFadeRange)
            float heightFactor = Mathf.InverseLerp(grassMinHeight, grassMinHeight + grassFadeRange, h);

            // --- 2. NACHYLENIE (SLOPE) ---
            // Czy teren jest p³aski?
            float slopeDot = Vector3.Dot(normal, Vector3.up);
            // Maska: 1 = p³asko, 0 = stromo
            float slopeMask = Mathf.InverseLerp(slopeLimit - slopeBlur, slopeLimit + slopeBlur, slopeDot);

            // --- 3. DECYZJA (PRESENCE) ---
            // Trawa jest tam, gdzie jest wysokoœæ I gdzie jest w miarê p³asko.
            // ¯adnego losowego szumu!
            float grassPresence = heightFactor * slopeMask;

            // --- 4. OBLICZENIA PARAMETRÓW ---

            // Gêstoœæ: Na dole (beach) gêsto, na górze (land) normalnie.
            float wantedDensity = Mathf.Lerp(grassDensityEdge, grassDensityBase, grassPresence);

            // Twarde odciêcie poni¿ej pewnego progu, ¿eby nie by³o "kurzu"
            float finalDensity = (grassPresence < 0.01f) ? 0.0f : (wantedDensity / maxDensitySetting);

            // Skala: £agodny wzrost od min do max
            // SmoothStep sprawia, ¿e trawa nie roœnie liniowo, tylko bardziej naturalnie
            float smoothPresence = Mathf.SmoothStep(0f, 1f, grassPresence);
            float targetScale = Mathf.Lerp(minGrassScale, maxGrassScale, smoothPresence);

            float randomVar = (float)prng.NextDouble() * 0.4f + 0.8f;
            float finalScale = targetScale * randomVar;

            // Jeœli jest pod wod¹/pla¿¹ -> ca³kowite zero
            if (h < grassMinHeight)
            {
                finalDensity = 0f;
            }

            colors[i] = new Color(finalDensity, finalScale, 0f, 1.0f);
        }
        mesh.colors = colors;

        mf.sharedMesh = mesh;
        mc.sharedMesh = mesh;

        if (grassMaterial != null)
        {
            PointGrassRenderer pgr = chunk.AddComponent<PointGrassRenderer>();
            pgr.distSource = PointGrassCommon.DistributionSource.MeshFilter;

            if (grassClumpMesh != null)
            {
                pgr.bladeType = PointGrassCommon.BladeType.Mesh;
                pgr.SetBladeMesh(grassClumpMesh);
            }
            else
            {
                pgr.bladeType = PointGrassCommon.BladeType.Flat;
            }

            pgr.SetMaterial(grassMaterial);
            pgr.shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            pgr.multiplyByArea = true;
            pgr.pointCount = maxDensitySetting;

            pgr.useDensity = true;
            pgr.densityCutoff = 0.1f;

            pgr.useLength = true;
            pgr.lengthMapping = new Vector2(0.0f, 1.0f);

            pgr.randomiseSeed = true;
            pgr.seed = (cx * 100) + cy + seed;
            pgr.overwriteNormalDirection = true;
            pgr.forcedNormal = Vector3.up;

            Vector3 boundsCenter = new Vector3(chunkSize / 2f, heightMultiplier / 2f, chunkSize / 2f);
            Vector3 boundsSize = new Vector3(chunkSize, heightMultiplier * 2f, chunkSize);
            pgr.boundingBoxOffset = new Bounds(boundsCenter, boundsSize);

            pgr.BuildPoints();
        }
    }

    float[,] ExtractChunkWithBorder(float[,] map, int cx, int cy)
    {
        int borderedSize = chunkSize + 3;
        float[,] chunk = new float[borderedSize, borderedSize];
        int startX = (cx * chunkSize) - 1;
        int startY = (cy * chunkSize) - 1;

        for (int y = 0; y < borderedSize; y++)
            for (int x = 0; x < borderedSize; x++)
            {
                int mx = Mathf.Clamp(startX + x, 0, map.GetLength(0) - 1);
                int my = Mathf.Clamp(startY + y, 0, map.GetLength(1) - 1);
                chunk[x, y] = map[mx, my];
            }
        return chunk;
    }

    void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}