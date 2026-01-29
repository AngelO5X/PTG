using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimalAI : MonoBehaviour
{
    [Header("Zarz¹dzanie Populacj¹")]
    [Tooltip("Ile maksymalnie zwierz¹t ma byæ jednoczeœnie w pobli¿u gracza.")]
    public int maxAnimalsAroundPlayer = 15;

    [Tooltip("Jak czêsto (w sekundach) system ma sprawdzaæ spawn/despawn. Wy¿sze wartoœci = lepsza wydajnoœæ.")]
    public float checkInterval = 1.0f;

    [Header("Dystanse (Strefy)")]
    [Tooltip("Minimalna odleg³oœæ od gracza, gdzie mo¿e pojawiæ siê zwierzê (¿eby nie wyskoczy³o przed twarz¹).")]
    public float minSpawnDistance = 20f;

    [Tooltip("Maksymalna odleg³oœæ spawnu (promieñ renderowania).")]
    public float maxSpawnDistance = 60f;

    [Tooltip("Jeœli zwierzê odejdzie dalej ni¿ ta wartoœæ, zostanie usuniête.")]
    public float despawnDistance = 90f;

    [Header("Ustawienia Spawnu")]
    public GameObject[] animalPrefabs;
    public LayerMask groundLayer;
    private LayerMask waterLayer; // Wykrywane automatycznie

    [Header("Ustawienia Ruchu Zwierz¹t")]
    public float moveSpeed = 1.5f;
    public float escapeRadius = 8.0f;

    // Prywatne zmienne
    private Transform playerTransform;
    private List<GameObject> activeAnimals = new List<GameObject>();
    private ArchipelagoGenerator mapGenerator;

    IEnumerator Start()
    {
        // 1. Konfiguracja warstw (Auto-Fix)
        groundLayer |= (1 << 0); // Zawsze dodaj Default
        int waterIndex = LayerMask.NameToLayer("Water");
        if (waterIndex != -1) waterLayer = 1 << waterIndex;

        // 2. Szukanie generatora i czekanie na mapê
        mapGenerator = FindObjectOfType<ArchipelagoGenerator>();
        if (mapGenerator != null)
        {
            Debug.Log("AnimalAI: Czekam na wygenerowanie mapy...");
            yield return new WaitUntil(() => mapGenerator.IsMapReady);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        // 3. Szukanie gracza i CZEKANIE NA TELEPORT
        // Czekamy, a¿ gracz zostanie znaleziony I jego pozycja nie bêdzie (0,0,0) (chyba ¿e mapa tam jest)
        // Zak³adamy, ¿e generator teleportuje gracza wysoko lub daleko.
        Debug.Log("AnimalAI: Mapa gotowa. Czekam na gracza...");

        while (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("AnimalAI: Gracz znaleziony. Uruchamiam system dynamicznej populacji.");

        // 4. Uruchomienie pêtli zarz¹dzania populacj¹
        StartCoroutine(PopulationLoop());
    }

    // G³ówna pêtla logiczna - dzia³a w tle co 'checkInterval' sekund
    IEnumerator PopulationLoop()
    {
        while (true)
        {
            if (playerTransform != null)
            {
                // A. Wyczyœæ listê z "pustych" obiektów (jeœli coœ zosta³o usuniête inaczej)
                activeAnimals.RemoveAll(item => item == null);

                // B. DESPAWNOWANIE (Usuwanie tych, co s¹ za daleko)
                DespawnFarAnimals();

                // C. SPAWNOWANIE (Dodawanie nowych, jeœli brakuje)
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
        // Idziemy od ty³u listy, ¿eby bezpiecznie usuwaæ elementy
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
        // Próbujemy znaleŸæ dobre miejsce (maks 5 prób na cykl, ¿eby nie ci¹æ klatek)
        for (int i = 0; i < 5; i++)
        {
            // Losujemy punkt w "pierœcieniu" (miêdzy minSpawnDistance a maxSpawnDistance)
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 offset = new Vector3(randomDir.x, 0, randomDir.y) * randomDist;

            Vector3 potentialPos = playerTransform.position + offset;

            // Sprawdzamy Raycastem z góry (300m nad punktem)
            Vector3 rayStart = new Vector3(potentialPos.x, 300f, potentialPos.z);
            RaycastHit hit;
            LayerMask combinedMask = groundLayer | waterLayer;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 600f, combinedMask))
            {
                // 1. SprawdŸ czy to WODA
                if (((1 << hit.collider.gameObject.layer) & waterLayer) != 0) continue;

                // 2. SprawdŸ czy to ZIEMIA
                if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
                {
                    // Znaleziono dobre miejsce!
                    SpawnAnimalAt(hit.point + Vector3.up * 0.5f);
                    return; // Uda³o siê, koñczymy funkcjê w tej klatce
                }
            }
        }
    }

    void SpawnAnimalAt(Vector3 position)
    {
        if (animalPrefabs.Length == 0) return;

        GameObject prefab = animalPrefabs[Random.Range(0, animalPrefabs.Length)];
        GameObject newAnimal = Instantiate(prefab, position, Quaternion.identity);

        // Opcjonalnie: Losowy obrót startowy
        newAnimal.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        activeAnimals.Add(newAnimal);
    }

    // Update s³u¿y TYLKO do poruszania istniej¹cymi zwierzêtami
    void Update()
    {
        if (playerTransform == null) return;

        foreach (GameObject animal in activeAnimals)
        {
            if (animal == null) continue;

            float distanceToPlayer = Vector3.Distance(animal.transform.position, playerTransform.position);

            // LOGIKA UCIECZKI
            if (distanceToPlayer < escapeRadius)
            {
                Vector3 directionAway = (animal.transform.position - playerTransform.position).normalized;
                directionAway.y = 0; // Ruch tylko w poziomie

                Vector3 nextPosition = animal.transform.position + (directionAway * moveSpeed * Time.deltaTime);

                if (IsPositionSafe(nextPosition))
                {
                    animal.transform.position = nextPosition;

                    if (directionAway != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(directionAway);
                        animal.transform.rotation = Quaternion.Slerp(animal.transform.rotation, targetRot, 5f * Time.deltaTime);
                    }
                }
            }

            // GRAWITACJA (Klejenie do terenu)
            StickToGround(animal);
        }
    }

    bool IsPositionSafe(Vector3 targetPos)
    {
        Vector3 rayStart = targetPos + Vector3.up * 5f;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f, groundLayer | waterLayer))
        {
            // Jeœli woda - niebezpiecznie
            if (((1 << hit.collider.gameObject.layer) & waterLayer) != 0) return false;
            return true;
        }
        return false;
    }

    void StickToGround(GameObject animal)
    {
        // Prosty system trzymania siê pod³o¿a
        RaycastHit hit;
        if (Physics.Raycast(animal.transform.position + Vector3.up * 2f, Vector3.down, out hit, 10f, groundLayer))
        {
            Vector3 pos = animal.transform.position;
            pos.y = hit.point.y;
            animal.transform.position = pos;
        }
    }
}