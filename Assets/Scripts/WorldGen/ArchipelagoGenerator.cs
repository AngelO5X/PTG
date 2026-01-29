using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using MicahW.PointGrass;

public class ArchipelagoGenerator : NetworkBehaviour
{
    [Header("Loading Optimization")]
    public float maxFrameTimeMs = 5f;

    [Header("Grass Optimization")]
    public float grassDrawDistance = 80f;
    public float grassCullingInterval = 0.5f;

    [Header("World Settings")]
    public int chunkSize = 64;
    public int chunks = 16;
    public Material terrainMaterial;

    [Header("Water Settings")]
    public GameObject waterPrefab;
    public float waterLevel = 4.0f;

    [Header("Noise Settings")]
    public float scale = 40f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Height Settings")]
    public float heightMultiplier = 10f;
    private float waterOffset = 0.2f;

    [Header("Grass Rules")]
    public float grassMinHeight = 1.2f;
    public float grassFadeRange = 1.5f;
    [Range(0f, 1f)] public float slopeLimit = 0.55f;
    public float slopeBlur = 0.1f;

    [Header("Grass Visuals")]
    public float minGrassScale = 0.4f;
    public float maxGrassScale = 1.0f;
    public float grassDensityBase = 2.5f;
    public float grassDensityEdge = 8.0f;

    [Header("Mesh Settings")]
    public Mesh grassClumpMesh;
    public Material grassMaterial;

    [SyncVar]
    public int seed;
    public bool IsMapReady { get; private set; } = false;

    private List<PointGrassRenderer> allGrassInstances = new List<PointGrassRenderer>();
    private List<PointGrassRenderer> grassQueue = new List<PointGrassRenderer>();
    private float[,] globalNoiseMap;
    private Transform playerTransform;

    // --- ZMIENNE DO OBS£UGI LOADING SCREENA ---
    private GameObject activeLoadingScreen;
    private Slider activeLoadingSlider;

    private void Start()
    {
        if (Application.isPlaying)
        {
            // Próba znalezienia Loading Screena
            FindLoadingScreen();

            if (!NetworkClient.active && !NetworkServer.active)
            {
                StartCoroutine(GenerationSequence(seed != 0 ? seed : Random.Range(0, 10000)));
            }
        }
    }

    public override void OnStartServer()
    {
        seed = Random.Range(0, int.MaxValue);
        StartCoroutine(GenerationSequence(seed));
    }

    public override void OnStartClient()
    {
        // Klient te¿ szuka paska
        FindLoadingScreen();

        if (NetworkServer.active && NetworkClient.active) return;
        StartCoroutine(GenerationSequence(seed));
    }

    void FindLoadingScreen()
    {
        if (activeLoadingScreen == null)
        {
            activeLoadingScreen = GameObject.FindGameObjectWithTag("LoadingScreen");
            if (activeLoadingScreen != null)
                activeLoadingSlider = activeLoadingScreen.GetComponentInChildren<Slider>();
        }
    }

    // --- SYSTEM CULLINGU (OPTYMALIZACJA) ---
    IEnumerator GrassCullingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(grassCullingInterval);
        while (true)
        {
            if (playerTransform == null) FindPlayerRef();

            if (playerTransform != null)
            {
                Vector3 playerPos = playerTransform.position;
                float distSqLimit = grassDrawDistance * grassDrawDistance;
                int processed = 0;

                for (int i = 0; i < allGrassInstances.Count; i++)
                {
                    var veg = allGrassInstances[i];
                    if (veg == null) continue;

                    float distSq = (veg.transform.position - playerPos).sqrMagnitude;
                    bool shouldBeVisible = distSq < distSqLimit;

                    if (veg.enabled != shouldBeVisible)
                        veg.enabled = shouldBeVisible;

                    processed++;
                    if (processed > 50)
                    {
                        processed = 0;
                        yield return null;
                    }
                }
            }
            yield return wait;
        }
    }

    void FindPlayerRef()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
        else if (NetworkClient.localPlayer != null) playerTransform = NetworkClient.localPlayer.transform;
    }

    // --- G£ÓWNA PÊTLA GENEROWANIA ---
    IEnumerator GenerationSequence(int usedSeed)
    {
        Debug.Log("Rozpoczynam generowanie œwiata...");
        IsMapReady = false;

        // UWAGA: Nie blokujemy gracza tutaj! Niech spada z grawitacj¹ w tle.
        // Loading Screen i tak zas³ania widok.

        Clear();
        grassQueue.Clear();
        allGrassInstances.Clear();

        // Ustawiamy pasek na start (10%)
        if (activeLoadingSlider != null) activeLoadingSlider.value = 0.1f;

        int totalVerts = (chunkSize * chunks) + 1;
        int worldMetricSize = chunkSize * chunks;

        // --- GENEROWANIE MAPY SZUMU ---
        float[,] noise = NoiseGenerator.GenerateNoise(totalVerts, scale, usedSeed, octaves, persistence, lacunarity);
        float[,] falloff = FalloffGenerator.GenerateFalloffMap(totalVerts);

        for (int y = 0; y < totalVerts; y++)
            for (int x = 0; x < totalVerts; x++)
                noise[x, y] = Mathf.Clamp01(noise[x, y] - falloff[x, y]);

        globalNoiseMap = noise;
        yield return null;

        float stopwatch = Time.realtimeSinceStartup;

        // --- GENEROWANIE CHUNKÓW TERENU ---
        int totalChunks = chunks * chunks;
        int chunksProcessed = 0;

        for (int cy = 0; cy < chunks; cy++)
        {
            for (int cx = 0; cx < chunks; cx++)
            {
                GenerateSingleChunkTerrain(noise, cx, cy, worldMetricSize);
                chunksProcessed++;

                if ((Time.realtimeSinceStartup - stopwatch) * 1000f > maxFrameTimeMs)
                {
                    // Aktualizacja paska (od 10% do 60%)
                    if (activeLoadingSlider != null)
                    {
                        float progress = 0.1f + ((float)chunksProcessed / totalChunks) * 0.5f;
                        activeLoadingSlider.value = progress;
                    }
                    yield return null;
                    stopwatch = Time.realtimeSinceStartup;
                }
            }
        }

        // --- GENEROWANIE WODY ---
        GenerateGlobalOcean();
        yield return null;

        // --- INICJALIZACJA TRAWY ---
        Vector3 centerPos = new Vector3((chunks * chunkSize) / 2f, 0, (chunks * chunkSize) / 2f);
        grassQueue.Sort((a, b) => Vector3.Distance(a.transform.position, centerPos).CompareTo(Vector3.Distance(b.transform.position, centerPos)));

        stopwatch = Time.realtimeSinceStartup;
        for (int i = 0; i < grassQueue.Count; i++)
        {
            if (grassQueue[i] != null)
            {
                grassQueue[i].BuildPoints();
                allGrassInstances.Add(grassQueue[i]);
                grassQueue[i].enabled = false;
            }

            if ((Time.realtimeSinceStartup - stopwatch) * 1000f > maxFrameTimeMs)
            {
                // Aktualizacja paska (od 60% do 95%)
                if (activeLoadingSlider != null)
                {
                    float progress = 0.6f + ((float)i / grassQueue.Count) * 0.35f;
                    activeLoadingSlider.value = progress;
                }
                yield return null;
                stopwatch = Time.realtimeSinceStartup;
            }
        }

        SpawnGlobalVegetation();
        StartCoroutine(GrassCullingRoutine());

        // Pasek na 100%
        if (activeLoadingSlider != null) activeLoadingSlider.value = 1.0f;
        yield return new WaitForSeconds(0.2f); // Krótkie opóŸnienie

        // --- FINALIZACJA I TELEPORTACJA ---
        TeleportPlayerToCenter();

        // Usuwamy ekran ³adowania
        if (activeLoadingScreen != null) Destroy(activeLoadingScreen);

        Debug.Log("Generowanie zakoñczone.");
        IsMapReady = true;
    }

    // --- FUNKCJA TELEPORTACJI (IGNORUJ¥CA WODÊ) ---
    void TeleportPlayerToCenter()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null && NetworkClient.localPlayer != null)
            player = NetworkClient.localPlayer.gameObject;

        if (player == null)
        {
            Debug.LogError("B£¥D: Nie znaleziono gracza do teleportacji!");
            return;
        }

        Debug.Log("Teleportacja gracza na twardy l¹d...");

        Physics.SyncTransforms();

        float centerX = (chunks * chunkSize) / 2f;
        float centerZ = (chunks * chunkSize) / 2f;

        Vector3 worldCenter = new Vector3(centerX, 200f, centerZ);
        Vector3 safeSpawn = new Vector3(centerX, 60f, centerZ);
        bool foundSpot = false;

        for (int i = 0; i < 200; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * (i * 4f);
            Vector3 checkPos = worldCenter + new Vector3(randomCircle.x, 0, randomCircle.y);

            RaycastHit[] hits = Physics.RaycastAll(checkPos, Vector3.down, 300f);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.name.Contains("Chunk"))
                {
                    if (hit.point.y > (waterLevel + 0.5f))
                    {
                        safeSpawn = hit.point + Vector3.up * 2f;
                        foundSpot = true;
                        goto SpotFound;
                    }
                }
            }
        }

    SpotFound:

        // Logika ostrze¿eñ
        if (!foundSpot) Debug.LogWarning("Nie znaleziono miejsca. Teleportujê na domyœln¹.");

        // Przenoszenie gracza
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // Wy³¹czamy fizykê na moment przeniesienia

        player.transform.position = safeSpawn;

        if (cc != null) cc.enabled = true; // W³¹czamy fizykê z powrotem

        // USUNIÊTO LINIE Z 'canControl', BO JU¯ JEJ NIE U¯YWAMY
    }

    void SpawnGlobalVegetation()
    {
        VegetationGenerator vegGen = FindObjectOfType<VegetationGenerator>();
        if (vegGen != null)
        {
            float totalSize = chunks * chunkSize;
            Bounds worldBounds = new Bounds(
                new Vector3(totalSize / 2f, 100f, totalSize / 2f),
                new Vector3(totalSize, 300f, totalSize)
            );
            vegGen.SpawnVegetation(worldBounds);
        }
    }

    void GenerateGlobalOcean()
    {
        if (waterPrefab == null) return;
        GameObject water = Instantiate(waterPrefab, transform);
        water.name = "Global_Ocean";
        float totalWorldSize = chunks * chunkSize;
        float center = totalWorldSize / 2f;
        water.transform.position = new Vector3(center, waterLevel, center);
        float scaleSize = totalWorldSize * 1.5f;
        water.transform.localScale = new Vector3(scaleSize, 1f, scaleSize);
        water.layer = LayerMask.NameToLayer("Water");
    }

    void GenerateSingleChunkTerrain(float[,] noiseMap, int cx, int cy, int worldMetricSize)
    {
        if (terrainMaterial == null) return;

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

        // Kolorowanie trawy
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Color[] grassColors = new Color[vertices.Length];

        System.Random prng = new System.Random(seed + cx * 100 + cy);
        float maxGrassDensity = Mathf.Max(grassDensityBase, grassDensityEdge);
        bool chunkHasGrass = false;

        for (int i = 0; i < vertices.Length; i++)
        {
            float h = vertices[i].y;
            Vector3 normal = normals[i];
            float heightFactor = Mathf.InverseLerp(grassMinHeight, grassMinHeight + grassFadeRange, h);
            float slopeDot = Vector3.Dot(normal, Vector3.up);
            float slopeMask = Mathf.InverseLerp(slopeLimit - slopeBlur, slopeLimit + slopeBlur, slopeDot);
            float grassPresence = heightFactor * slopeMask;
            float wantedGrass = Mathf.Lerp(grassDensityEdge, grassDensityBase, grassPresence);
            float finalGrass = (grassPresence < 0.01f) ? 0.0f : (wantedGrass / maxGrassDensity);

            if (h < grassMinHeight) finalGrass = 0f;
            if (finalGrass > 0) chunkHasGrass = true;

            grassColors[i] = new Color(finalGrass, 0f, 0f, 1f);
        }

        mesh.colors = grassColors;
        mf.sharedMesh = mesh;
        mc.sharedMesh = mesh; // Wa¿ne: Collider terenu!

        if (grassMaterial != null && chunkHasGrass)
        {
            AddPointGrass(chunk, grassClumpMesh, grassMaterial, maxGrassDensity, (cx * 100) + cy + seed);
        }
    }

    void AddPointGrass(GameObject target, Mesh bladeMesh, Material mat, float density, int seedVal)
    {
        PointGrassRenderer pgr = target.AddComponent<PointGrassRenderer>();
        pgr.distSource = PointGrassCommon.DistributionSource.MeshFilter;
        if (bladeMesh != null) { pgr.bladeType = PointGrassCommon.BladeType.Mesh; pgr.SetBladeMesh(bladeMesh); }
        else { pgr.bladeType = PointGrassCommon.BladeType.Flat; }

        pgr.SetMaterial(mat);
        pgr.shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        pgr.multiplyByArea = true;
        pgr.pointCount = density;
        pgr.useDensity = true;
        pgr.densityCutoff = 0.1f;
        pgr.useLength = true;
        pgr.lengthMapping = new Vector2(0.8f, 1.4f);
        pgr.randomiseSeed = true;
        pgr.seed = seedVal;
        pgr.overwriteNormalDirection = true;
        pgr.forcedNormal = Vector3.up;
        Vector3 boundsCenter = new Vector3(chunkSize / 2f, heightMultiplier / 2f, chunkSize / 2f);
        Vector3 boundsSize = new Vector3(chunkSize, heightMultiplier * 2f, chunkSize);
        pgr.boundingBoxOffset = new Bounds(boundsCenter, boundsSize);
        grassQueue.Add(pgr);
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
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
    }
}