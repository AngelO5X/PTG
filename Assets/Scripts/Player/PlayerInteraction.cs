using UnityEngine;
using Mirror;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Ustawienia Interakcji")]
    public float interactionDistance = 1.5f;
    public LayerMask interactableLayers;

    [Header("Definicje Przedmiotów")]
    public Item woodItem;
    public Item rockItem;

    private InventoryManager inventory;
    private Camera playerCam;

    void Start()
    {
        if (!isLocalPlayer) return;
        inventory = GetComponent<InventoryManager>();
        playerCam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (PauseMenu_.GameIsPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayers))
        {
            GameObject target = hit.collider.gameObject;
            int layer = target.layer;
            string layerName = LayerMask.LayerToName(layer);

            if (layerName == "Tree")
            {
                inventory.AddItem(woodItem);
                CmdHarvest(target); // Wysy³amy proœbê o usuniêcie do serwera
            }
            else if (layerName == "Rock")
            {
                inventory.AddItem(rockItem);
                CmdHarvest(target);
            }
        }
    }

    [Command]
    void CmdHarvest(GameObject target)
    {
        // Serwer niszczy obiekt. Jeœli obiekt ma NetworkIdentity, u¿yj NetworkServer.Destroy.
        // Jeœli to zwyk³y prefab (jak drzewa z generatora), wystarczy Destroy.
        if (target != null)
        {
            Destroy(target);
            // W Mirror, jeœli obiekt nie ma NetworkIdentity, Destroy na serwerze 
            // nie usunie go automatycznie u klientów. 
            // Jeœli Twoje drzewa s¹ zwyk³ymi obiektami, u¿yj RPC poni¿ej:
            RpcDestroyOnClients(target);
        }
    }

    [ClientRpc]
    void RpcDestroyOnClients(GameObject target)
    {
        if (target != null && !isServer) // Serwer ju¿ go usun¹³
        {
            Destroy(target);
        }
    }
}