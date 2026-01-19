using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using MicahW.PointGrass;

public class ChunkOptimizer : MonoBehaviour
{
    // --- SETTINGS ---
    [Header("Settings")]
    [Tooltip("Dystans renderowania w metrach.")]
    public float renderDistance = 80f;

    [Tooltip("Czêstotliwoœæ sprawdzania (w sekundach).")]
    public float checkInterval = 0.5f;

    private List<ChunkData> allChunks = new List<ChunkData>();
    private Transform myTransform;

    // --- INTERNAL DATA ---
    private class ChunkData
    {
        public Transform transform;
        public MeshRenderer terrainRenderer;
        public PointGrassRenderer grassRenderer;
        public bool isVisible;
    }

    // --- INITIALIZATION ---
    void Start()
    {
        if (GetComponent<NetworkIdentity>() != null && !GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            Destroy(this);
            return;
        }

        myTransform = transform;
        StartCoroutine(FindChunksAndOptimizeRoutine());
    }

    // --- OPTIMIZATION LOOP ---
    IEnumerator FindChunksAndOptimizeRoutine()
    {
        // Czekamy na wygenerowanie œwiata
        yield return new WaitForSeconds(1.0f);

        var generator = FindObjectOfType<ArchipelagoGenerator>();
        if (generator == null)
        {
            Debug.LogError("Nie znaleziono ArchipelagoGenerator!");
            yield break;
        }

        // Cache'owanie chunków
        foreach (Transform child in generator.transform)
        {
            ChunkData data = new ChunkData();
            data.transform = child;
            data.terrainRenderer = child.GetComponent<MeshRenderer>();
            data.grassRenderer = child.GetComponent<PointGrassRenderer>();
            data.isVisible = true;

            allChunks.Add(data);
        }

        // Pêtla sprawdzaj¹ca
        while (true)
        {
            OptimizeChunks();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    // --- LOGIC ---
    void OptimizeChunks()
    {
        Vector3 playerPos = myTransform.position;
        float renderDistSqr = renderDistance * renderDistance;

        for (int i = 0; i < allChunks.Count; i++)
        {
            ChunkData chunk = allChunks[i];

            if (chunk.transform == null) continue;

            float distSqr = (chunk.transform.position.x - playerPos.x) * (chunk.transform.position.x - playerPos.x) +
                            (chunk.transform.position.z - playerPos.z) * (chunk.transform.position.z - playerPos.z);

            bool shouldBeVisible = distSqr < renderDistSqr;

            if (chunk.isVisible != shouldBeVisible)
            {
                chunk.isVisible = shouldBeVisible;

                if (chunk.terrainRenderer != null)
                    chunk.terrainRenderer.enabled = shouldBeVisible;

                if (chunk.grassRenderer != null)
                    chunk.grassRenderer.enabled = shouldBeVisible;
            }
        }
    }
}