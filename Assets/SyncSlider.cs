using UnityEngine;
using UnityEngine.UI;

public class SyncSlider : MonoBehaviour
{
    void OnEnable()
    {
        Slider mySlider = GetComponent<Slider>();

        // 1. Ustaw pozycjê suwaka na podstawie zapisanych ustawieñ
        float savedVolume = PlayerPrefs.GetFloat("SavedMusicVolume", 0.75f);
        mySlider.value = savedVolume;

        // 2. Automatycznie po³¹cz suwak z AudioManagerem
        mySlider.onValueChanged.RemoveAllListeners(); // Czyœcimy stare po³¹czenia
        mySlider.onValueChanged.AddListener(delegate { OnSliderChanged(mySlider.value); });
    }

    void OnSliderChanged(float value)
    {
        // Korzystamy z faktu, ¿e AudioManager to Singleton (instance)
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetVolume(value);
        }
    }
}
