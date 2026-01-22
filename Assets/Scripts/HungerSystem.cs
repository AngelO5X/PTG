using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class HungerSystem : NetworkBehaviour
{
    private Slider hungerSlider;
    private TextMeshProUGUI hungerValueText;
    private PlayerHealth playerHealth;

    [Header("Settings")]
    public float maxHunger = 100f;
    public float currentHunger;
    public float drainInterval = 5f;   // Co ile spada glod (np. 5s)
    public float damageInterval = 10f; // Co ile zabiera HP przy 0 (np. 10s)
    public float starveDamage = 1f;    // Ile HP zabiera

    void Start()
    {
        if (!isLocalPlayer) return;

        currentHunger = maxHunger;
        playerHealth = GetComponent<PlayerHealth>();

        // Szukanie UI (zakładając Tag HUD)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject hud = null;
        foreach (GameObject obj in allObjects) { if (obj.CompareTag("HUD")) { hud = obj; break; } }

        if (hud != null)
        {
            foreach (Slider s in hud.GetComponentsInChildren<Slider>(true))
                if (s.gameObject.name == "Hunger") hungerSlider = s;

            foreach (TextMeshProUGUI t in hud.GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t.gameObject.name == "Hunger_Value") hungerValueText = t;
        }

        // Pierwszy zegar: Spadek glodu co 5s
        InvokeRepeating(nameof(DecreaseHunger), drainInterval, drainInterval);
        
        // Drugi zegar: Zadawanie obrazen co 10s
        InvokeRepeating(nameof(StarveDamage), damageInterval, damageInterval);

        UpdateUI();
    }

    void DecreaseHunger()
    {
        if (currentHunger > 0)
        {
            currentHunger -= 1f;
            currentHunger = Mathf.Max(currentHunger, 0);
            UpdateUI();
        }
    }

    void StarveDamage()
    {
        if (currentHunger <= 0 && playerHealth != null)
        {
            playerHealth.TakeDamage(starveDamage);
            Debug.Log("Głodujesz! Tracisz HP co " + damageInterval + " sekund.");
        }
    }

    void UpdateUI()
    {
        if (hungerSlider != null) hungerSlider.value = currentHunger;
        if (hungerValueText != null) hungerValueText.text = Mathf.CeilToInt(currentHunger) + "/" + maxHunger;
    }
}