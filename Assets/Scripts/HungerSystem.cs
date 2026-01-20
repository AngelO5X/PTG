using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class HungerSystem : NetworkBehaviour
{
    private GameObject hudObject;
    private Slider hungerSlider;
    private TextMeshProUGUI hungerValueText;

    [Header("Settings")]
    public float maxHunger = 100f;
    public float currentHunger;
    public float drainInterval = 5f; // Co ile sekund ma spadac glod

    void Start()
    {
        // Wykonaj tylko dla gracza lokalnego
        if (!isLocalPlayer) return;

        currentHunger = maxHunger;

        // Szukamy HUD (nawet jesli jest wylaczony)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("HUD"))
            {
                hudObject = obj;
                break;
            }
        }

        if (hudObject != null)
        {
            // Szukamy Slidera wewnatrz obiektu Hunger
            // Zakladamy, ze skrypt jest na Playerze, a Hunger jest dzieckiem HUD
            Slider[] allSliders = hudObject.GetComponentsInChildren<Slider>(true);
            foreach (Slider s in allSliders)
            {
                if (s.gameObject.name == "Hunger")
                {
                    hungerSlider = s;
                    break;
                }
            }

            // Szukamy tekstu Hunger_Value (na screenie widac go w Hunger -> Fill Area)
            TextMeshProUGUI[] allTexts = hudObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI t in allTexts)
            {
                if (t.gameObject.name == "Hunger_Value")
                {
                    hungerValueText = t;
                    break;
                }
            }
        }

        // Uruchomienie odliczania (Metoda, czas do pierwszego wywolania, powtarzalnosc)
        InvokeRepeating(nameof(DecreaseHunger), drainInterval, drainInterval);

        UpdateHungerUI();
    }

    void DecreaseHunger()
    {
        if (!isLocalPlayer) return;

        if (currentHunger > 0)
        {
            currentHunger -= 1f;
            currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
            UpdateHungerUI();
        }
        else
        {
            // Jesli glod spadnie do 0, mozesz zadawac obrazenia
            if (GetComponent<PlayerHealth>() != null)
            {
                GetComponent<PlayerHealth>().TakeDamage(2f);
            }
        }
    }

    void UpdateHungerUI()
    {
        if (hungerSlider != null)
        {
            hungerSlider.value = currentHunger;
        }

        if (hungerValueText != null)
        {
            // Formatowanie tekstu na 100 / 100
            hungerValueText.text = Mathf.CeilToInt(currentHunger).ToString() + "/" + maxHunger.ToString();
        }
    }
}