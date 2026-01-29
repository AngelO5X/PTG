using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimalController : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    public float moveSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float turnSpeed = 5.0f;

    [Header("AI - W³óczenie siê")]
    public float wanderRadius = 10f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [Header("AI - Ucieczka")]
    public float escapeRadius = 8.0f;

    // Te zmienne uzupe³nia AnimalAI (Manager)
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public LayerMask groundLayer;
    [HideInInspector] public LayerMask waterLayer;

    private Vector3 targetPosition;
    private float timer;
    private bool isIdle = true;

    void Start()
    {
        targetPosition = transform.position;
        timer = Random.Range(minIdleTime, maxIdleTime);
    }

    void Update()
    {
        // 1. Dopasowanie do terenu
        StickToGround();

        if (playerTransform == null) return;

        // 2. Decyzja: Ucieczka czy Spacer?
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer < escapeRadius)
        {
            HandleFleeing();
        }
        else
        {
            HandleWandering();
        }
    }

    void HandleFleeing()
    {
        // Przerywamy stanie w miejscu, bo musimy uciekaæ
        isIdle = false;

        // Kierunek przeciwny do gracza
        Vector3 dirToPlayer = (transform.position - playerTransform.position).normalized;
        Vector3 fleePos = transform.position + dirToPlayer * 5f;

        MoveTo(fleePos, runSpeed);
    }

    void HandleWandering()
    {
        if (isIdle)
        {
            // Odliczanie czasu postoju
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                FindNewWanderTarget();
            }
        }
        else
        {
            // Idziemy do celu
            MoveTo(targetPosition, moveSpeed);

            // Sprawdzamy czy doszliœmy (dystans mniejszy ni¿ 0.5m)
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                isIdle = true;
                timer = Random.Range(minIdleTime, maxIdleTime);
            }
        }
    }

    void FindNewWanderTarget()
    {
        // Próbujemy 5 razy znaleŸæ bezpieczne miejsce (nie wodê)
        for (int i = 0; i < 5; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 potentialPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (IsPositionSafe(potentialPos))
            {
                targetPosition = potentialPos;
                isIdle = false; // Przestajemy staæ, zaczynamy iœæ
                return;
            }
        }
        timer = 1.0f; // Jak nie znajdzie miejsca, czeka chwilê d³u¿ej
    }

    void MoveTo(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Ignorujemy ró¿nicê wysokoœci przy obrocie

        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    bool IsPositionSafe(Vector3 targetPos)
    {
        // Startujemy raycast wy¿ej, ¿eby wykryæ teren
        Vector3 rayStart = new Vector3(targetPos.x, transform.position.y + 10f, targetPos.z);
        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 50f, groundLayer | waterLayer))
        {
            // Jeœli trafiliœmy w wodê - odrzucamy to miejsce
            if (((1 << hit.collider.gameObject.layer) & waterLayer) != 0) return false;

            targetPosition.y = hit.point.y; // Aktualizujemy Y celu do poziomu gruntu
            return true;
        }
        return false;
    }

    void StickToGround()
    {
        RaycastHit hit;
        // Sprawdzamy grunt pod nogami
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hit, 5f, groundLayer))
        {
            Vector3 pos = transform.position;
            // P³ynne dopasowanie wysokoœci
            pos.y = Mathf.Lerp(pos.y, hit.point.y, 10f * Time.deltaTime);
            transform.position = pos;
        }
    }
}