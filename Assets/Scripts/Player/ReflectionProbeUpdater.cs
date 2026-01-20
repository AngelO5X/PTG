using UnityEngine;

public class ReflectionProbeUpdater : MonoBehaviour
{
    [Header("Ustawienia")]
    public ReflectionProbe probeToUpdate;
    public float refreshInterval = 0.5f; // Czas w sekundach

    private float timer = 0f;

    void Start()
    {
        // Jeœli nie przypiszesz rêcznie, skrypt spróbuje znaleŸæ komponent na tym samym obiekcie
        if (probeToUpdate == null)
            probeToUpdate = GetComponent<ReflectionProbe>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= refreshInterval)
        {
            // Reset licznika
            timer = 0f;

            // Wymuszenie odœwie¿enia
            if (probeToUpdate != null)
            {
                probeToUpdate.RenderProbe();
            }
        }
    }
}