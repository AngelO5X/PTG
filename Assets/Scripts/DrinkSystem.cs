using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class DrinkSystem : NetworkBehaviour
{
    private Slider drinkSlider;
    private TextMeshProUGUI drinkValueText;
    private PlayerHealth playerHealth;

    [Header("Settings")]
    public float maxDrink = 100f;
    public float currentDrink;
    public float drainInterval = 5f;   // Co ile spada woda (np. 5s)
    public float damageInterval = 10f; // Co ile zabiera HP przy 0 (np. 10s)
    public float thirstDamage = 1f;

    void Start()
    {
        if (!isLocalPlayer) return;

        currentDrink = maxDrink;
        playerHealth = GetComponent<PlayerHealth>();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject hud = null;
        foreach (GameObject obj in allObjects) { if (obj.CompareTag("HUD")) { hud = obj; break; } }

        if (hud != null)
        {
            foreach (Slider s in hud.GetComponentsInChildren<Slider>(true))
                if (s.gameObject.name == "Drink") drinkSlider = s;

            foreach (TextMeshProUGUI t in hud.GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t.gameObject.name == "Drink_Value") drinkValueText = t;
        }

        // Pierwszy zegar: Spadek picia co 5s
        InvokeRepeating(nameof(DecreaseDrink), drainInterval, drainInterval);
        
        // Drugi zegar: Zadawanie obrazen co 10s
        InvokeRepeating(nameof(ThirstDamage), damageInterval, damageInterval);

        UpdateUI();
    }

    void DecreaseDrink()
    {
        if (currentDrink > 0)
        {
            currentDrink -= 1f;
            currentDrink = Mathf.Max(currentDrink, 0);
            UpdateUI();
        }
    }

    void ThirstDamage()
    {
        if (currentDrink <= 0 && playerHealth != null)
        {
            playerHealth.TakeDamage(thirstDamage);
            Debug.Log("Chce ci się pić! Tracisz HP co " + damageInterval + " sekund.");
        }
    }

    void UpdateUI()
    {
        if (drinkSlider != null) drinkSlider.value = currentDrink;
        if (drinkValueText != null) drinkValueText.text = Mathf.CeilToInt(currentDrink) + "/" + maxDrink;
    }
}