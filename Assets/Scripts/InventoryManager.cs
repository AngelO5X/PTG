using UnityEngine;
using Mirror;

public class InventoryManager : NetworkBehaviour
{
    private GameObject inventoryPanel;
    private bool isInventoryOpen = false;

    void Start()
    {
        if (!isLocalPlayer) return;

        // Szukamy obiektu Inventory wewnątrz HUD
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("HUD"))
            {
                // Szukamy dziecka o nazwie Inventory
                Transform inv = obj.transform.Find("Inventory");
                if (inv != null) 
                {
                    inventoryPanel = inv.gameObject;
                    inventoryPanel.SetActive(false); // Ukryj na starcie
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

    void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        // Odblokowanie myszki, żeby można było klikać w sloty
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