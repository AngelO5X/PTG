using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [Header("Limit FPS")]
    [Tooltip("Ustaw limit klatek na sekundê (0 = brak ograniczenia)")]
    public int targetFPS = 60;
    public int vSync = 0;

    void Start()
    {
        // Ustawienie limitu FPS
        Application.targetFrameRate = targetFPS;

        // Opcjonalnie: w³¹czenie lub wy³¹czenie synchronizacji pionowej (VSync)
        QualitySettings.vSyncCount = vSync; // 0 = VSync wy³¹czone, 1 = w³¹czone
    }

    // Je¿eli chcesz zmieniaæ FPS w trakcie gry:
    void Update()
    {
        if (Application.targetFrameRate != targetFPS)
            Application.targetFrameRate = targetFPS;
    }
}
