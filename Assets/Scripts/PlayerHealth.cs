using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Mirror; 

public class PlayerHealth : NetworkBehaviour
{
    private GameObject hudObject;
    private Slider healthBar;
    private TextMeshProUGUI hpValueText;

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
            
            // Szukamy Slidera o nazwie HealthBar
            Slider[] sliders = hudObject.GetComponentsInChildren<Slider>(true);
            foreach(Slider s in sliders) {
                if(s.gameObject.name == "HealthBar") healthBar = s;
            }
            
            foreach (TextMeshProUGUI tmpro in hudObject.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmpro.gameObject.name == "HP_Value")
                {
                    hpValueText = tmpro;
                    break;
                }
            }
            
            UpdateHealthUI();
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
        if (healthBar != null) healthBar.value = currentHealth;
        if (hpValueText != null)
        {
            hpValueText.text = Mathf.CeilToInt(currentHealth).ToString() + "/" + maxHealth.ToString();
        }
    }
}