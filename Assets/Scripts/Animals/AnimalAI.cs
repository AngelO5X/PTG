using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimalAI : MonoBehaviour
{
    [Header("Zarz¹dzanie Populacj¹")]
    public int maxAnimalsAroundPlayer = 15;
    public float checkInterval = 1.0f;

    [Header("Dystanse")]
    public float minSpawnDistance = 20f;
    public float maxSpawnDistance = 60f;
    public float despawnDistance = 90f;

    [Header("Ustawienia")]
    public GameObject[] animalPrefabs;
    public LayerMask groundLayer;
    private LayerMask waterLayer;

    private Transform playerTransform;
    private List<GameObject> activeAnimals = new List<GameObject>();
    private ArchipelagoGenerator mapGenerator;

    IEnumerator Start()
    {
        // 1. Warstwy
        groundLayer |= (1 << 0);
        int waterIndex = LayerMask.NameToLayer("Water");
        if (waterIndex != -1) waterLayer = 1 << waterIndex;

        // 2. Mapa
        mapGenerator = FindObjectOfType<ArchipelagoGenerator>();
        if (mapGenerator != null) yield return new WaitUntil(() => mapGenerator.IsMapReady);
        else yield return new WaitForSeconds(2f);

        // 3. Gracz
        while (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("AnimalAI: Gracz znaleziony. Start systemu.");
        StartCoroutine(PopulationLoop());
    }

    IEnumerator PopulationLoop()
    {
        while (true)
        {
            if (playerTransform != null)
            {
                activeAnimals.RemoveAll(item => item == null);
                DespawnFarAnimals();

                if (activeAnimals.Count < maxAnimalsAroundPlayer)
                {
                    TrySpawnNewAnimal();
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void DespawnFarAnimals()
    {
        for (int i = activeAnimals.Count - 1; i >= 0; i--)
        {
            GameObject animal = activeAnimals[i];
            float dist = Vector3.Distance(animal.transform.position, playerTransform.position);

            if (dist > despawnDistance)
            {
                activeAnimals.RemoveAt(i);
                Destroy(animal);
            }
        }
    }

    void TrySpawnNewAnimal()
    {
        for (int i = 0; i < 5; i++) // 5 prób znalezienia miejsca
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 offset = new Vector3(randomDir.x, 0, randomDir.y) * randomDist;
            Vector3 potentialPos = playerTransform.position + offset;

            Vector3 rayStart = new Vector3(potentialPos.x, 300f, potentialPos.z);
            RaycastHit hit;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 600f, groundLayer | waterLayer))
            {
                if (((1 << hit.collider.gameObject.layer) & waterLayer) != 0) continue; // Woda
                if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0) // Ziemia
                {
                    SpawnAnimalAt(hit.point);
                    return;
                }
            }
        }
    }

    void SpawnAnimalAt(Vector3 position)
    {
        if (animalPrefabs.Length == 0) return;

        GameObject prefab = animalPrefabs[Random.Range(0, animalPrefabs.Length)];
        GameObject newAnimal = Instantiate(prefab, position, Quaternion.identity);

        // Losowy obrót
        newAnimal.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Konfiguracja kontrolera
        AnimalController controller = newAnimal.GetComponent<AnimalController>();
        if (controller == null) controller = newAnimal.AddComponent<AnimalController>();

        // Przekazujemy kluczowe dane do zwierzêcia
        controller.playerTransform = playerTransform;
        controller.groundLayer = groundLayer;
        controller.waterLayer = waterLayer;

        activeAnimals.Add(newAnimal);
    }
}