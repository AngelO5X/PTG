using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror; // WA¯NE: Dodaj namespace Mirror

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreenObject; // Twój obiekt LoadingScreen (ca³y Canvas/Panel)
    public Slider loadingSlider;           // Pasek postêpu (Slider)

    public void PlayGame()
    {
        Debug.Log("Przycisk PLAY -> Uruchamianie Hosta...");

        StartCoroutine(StartHostWithLoadingScreen());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator StartHostWithLoadingScreen()
    {
        // 1. W³¹czamy Loading Screen
        if (loadingScreenObject != null)
        {
            loadingScreenObject.SetActive(true);
            if (loadingSlider != null) loadingSlider.value = 0f;

            // TO JEST KLUCZOWE: Obiekt nie mo¿e znikn¹æ przy zmianie sceny przez Mirrora
            DontDestroyOnLoad(loadingScreenObject);
        }

        yield return null; // Czekamy klatkê, ¿eby UI zd¹¿y³o siê odœwie¿yæ

        // 2. Sprawdzamy czy NetworkManager istnieje (powinien byæ w scenie Menu)
        if (NetworkManager.singleton != null)
        {
            // 3. Uruchamiamy Hosta. 
            // Mirror automatycznie za³aduje scenê ustawion¹ w "Online Scene" w Inspektorze NetworkManagera.
            NetworkManager.singleton.StartHost();
        }
        else
        {
            Debug.LogError("Brak NetworkManagera w scenie! Nie mo¿na wystartowaæ gry.");
        }
    }
}