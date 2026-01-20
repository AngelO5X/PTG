using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Mirror; 

public class PlayerHealth : NetworkBehaviour
{
    private GameObject hudObject;
    private Slider healthBar;
    private TextMeshProUGUI hpValueText; // Referencja tylko do cyferek

    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        if (!isLocalPlayer) return;

        currentHealth = maxHealth;

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
            hudObject.SetActive(true);
            healthBar = hudObject.GetComponentInChildren<Slider>();
            
            // SZUKANIE KONKRETNEGO OBIEKTU HP_Value
            // Szukamy w dzieciach HUD obiektu, który nazywa się dokładnie "HP_Value"
            Transform textTransform = hudObject.transform.Find("HP_Value");
            
            // Jeśli HP_Value jest głębiej (np. wewnątrz HealthBar), używamy tej metody:
            if (textTransform == null)
            {
                foreach (TextMeshProUGUI tmpro in hudObject.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (tmpro.gameObject.name == "HP_Value")
                    {
                        hpValueText = tmpro;
                        break;
                    }
                }
            }
            else
            {
                hpValueText = textTransform.GetComponent<TextMeshProUGUI>();
            }
            
            UpdateHealthUI();
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(5);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        // Teraz aktualizujemy tylko HP_Value, napis HP zostanie nienaruszony
        if (hpValueText != null)
        {
            hpValueText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
        }
    }
}


