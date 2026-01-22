using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VegetationGenerator : MonoBehaviour
{
    [Header("G³ówne Ustawienia")]
    [Tooltip("Warstwa terenu (Default).")]
    public LayerMask groundLayer;

    private Transform container;

    [Header("Optymalizacja (Culling)")]
    [Tooltip("Dystans widzenia roœlinnoœci (np. 80 metrów). Powy¿ej tego znikaj¹.")]
    public float viewDistance = 80f;
    [Tooltip("Co ile sekund sprawdzaæ odleg³oœæ (np. 0.5s). Wy¿sze wartoœci = lepsza wydajnoœæ.")]
    public float checkInterval = 0.5f;

    [System.Serializable]
    public class VegetationType
    {
        public string name;
        public GameObject prefab;

        [Header("Iloœæ (Losowo pomiêdzy)")]
        public int minCount = 500;
        public int maxCount = 1000;

        [Header("Wygl¹d")]
        [Range(0.1f, 5f)] public float minScale = 0.8f;
        [Range(0.1f, 5f)] public float maxScale = 1.2f;

        // --- NOWE POLE ---
        [Header("Pozycjonowanie")]
        [Tooltip("Przesuniêcie w pionie. Ujemna wartoœæ wbije obiekt w ziemiê (np. -0.2).")]
        public float heightOffset = -0.1f;
        // -----------------
    }

    [Header("Lista Roœlin")]
    public List<VegetationType> vegetationTypes;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Transform playerTransform;

    public void SpawnVegetation(Bounds worldBounds)
    {
        ClearVegetation();
        StopAllCoroutines();

        if (container == null)
        {
            GameObject go = new GameObject("Vegetation Container");
            container = go.transform;
        }

        foreach (var type in vegetationTypes)
        {
            int targetCount = Random.Range(type.minCount, type.maxCount);
            SpawnSingleType(type, targetCount, worldBounds);
        }

        StartCoroutine(CullingRoutine());
    }

    void SpawnSingleType(VegetationType veg, int count, Bounds mapBounds)
    {
        int currentCount = 0;
        int attempts = 0;
        int maxAttempts = count * 10;

        while (currentCount < count && attempts < maxAttempts)
        {
            attempts++;

            float randomX = Random.Range(mapBounds.min.x, mapBounds.max.x);
            float randomZ = Random.Range(mapBounds.min.z, mapBounds.max.z);

            Vector3 rayOrigin = new Vector3(randomX, 300f, randomZ);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 500f, groundLayer))
            {
                float height = hit.point.y;
                float angle = Vector3.Angle(hit.normal, Vector3.up);

                // Sprawdzenie wysokoœci i k¹ta nachylenia
                if (height > 1.23f && angle < 45f)
                {
                    CreateObject(veg, hit.point);
                    currentCount++;
                }
            }
        }
    }

    void CreateObject(VegetationType veg, Vector3 position)
    {
        // --- ZMIANA: Dodanie Offsetu ---
        // Dodajemy wartoœæ heightOffset do osi Y. 
        // Jeœli wpiszesz w inspektorze -0.5, obiekt wygeneruje siê pó³ metra ni¿ej.
        Vector3 finalPosition = position + new Vector3(0, veg.heightOffset, 0);

        GameObject newObj = Instantiate(veg.prefab, finalPosition, Quaternion.identity, container);

        newObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);

        float scale = Random.Range(veg.minScale, veg.maxScale);
        newObj.transform.localScale = Vector3.one * scale;

        newObj.SetActive(false);
        spawnedObjects.Add(newObj);
    }

    IEnumerator CullingRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);
        float sqrDistanceLimit = viewDistance * viewDistance;

        while (true)
        {
            if (playerTransform == null)
            {
                FindPlayer();
                yield return wait;
                continue;
            }

            Vector3 playerPos = playerTransform.position;
            int itemsProcessed = 0;

            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] == null) continue;

                float distSqr = (spawnedObjects[i].transform.position - playerPos).sqrMagnitude;
                bool shouldBeVisible = distSqr < sqrDistanceLimit;

                if (spawnedObjects[i].activeSelf != shouldBeVisible)
                {
                    spawnedObjects[i].SetActive(shouldBeVisible);
                }

                itemsProcessed++;
                if (itemsProcessed > 100)
                {
                    itemsProcessed = 0;
                    yield return null;
                }
            }

            yield return wait;
        }
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
        else if (NetworkClient.localPlayer != null) playerTransform = NetworkClient.localPlayer.transform;
    }

    public void ClearVegetation()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        if (container != null)
        {
            foreach (Transform child in container) Destroy(child.gameObject);
        }
    }
}