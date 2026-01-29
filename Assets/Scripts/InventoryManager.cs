using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class InventoryManager : NetworkBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public Item item;
        public int count;
    }

    public List<InventorySlot> slots = new List<InventorySlot>();
    private GameObject inventoryPanel;
    private bool isInventoryOpen = false;
    public InventoryUI uiDisplay;

    void Start()
    {
        if (!isLocalPlayer) return;
        FindUI();
    }

    void FindUI()
    {
        // Szukanie HUD i panelu Inventory zgodnie z Twoją strukturą
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("HUD"))
            {
                Transform inv = obj.transform.Find("Inventory");
                if (inv != null)
                {
                    inventoryPanel = inv.gameObject;
                    inventoryPanel.SetActive(false);
                }
                break;
            }
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    // Logika dodawania i stackowania przedmiotów

    // 2. Zaktualizuj funkcję AddItem
    public void AddItem(Item newItem)
    {
        if (newItem == null) return;

        bool found = false;
        foreach (var slot in slots)
        {
            if (slot.item == newItem)
            {
                slot.count++;
                found = true;
                break;
            }
        }

        if (!found)
        {
            slots.Add(new InventorySlot { item = newItem, count = 1 });
        }

        // WYWOŁANIE ODŚWIEŻENIA
        if (uiDisplay != null) uiDisplay.UpdateUI();

        Debug.Log("Zebrano: " + newItem.itemName);
    }

    void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        // Zarządzanie kursorem myszy
        if (isInventoryOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}