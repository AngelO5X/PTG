using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventoryManager; // Przeci¹gnij tu skrypt z Gracza (lub znajdŸ go w kodzie)
    public Transform itemsParent;             // Obiekt, który trzyma Twoje sloty UI
    public GameObject slotPrefab;             // Prefab pojedynczego slotu (ikona + tekst)

    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        // 1. Czyœcimy stare ikonki (opcjonalne, jeœli tworzysz je dynamicznie)
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Tworzymy nowe ikonki na podstawie listy z InventoryManager
        foreach (var slot in inventoryManager.slots)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemsParent);

            // Ustawiamy ikonê
            Image icon = newSlot.GetComponentInChildren<Image>();
            if (slot.item.icon != null) icon.sprite = slot.item.icon;

            // Ustawiamy tekst iloœci
            TextMeshProUGUI text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
            text.text = slot.count.ToString();
        }
    }
}