using UnityEngine;
using Mirror;

public class PlayerHUDInitializer : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        GameObject hud = GameObject.Find("HUD");
        if (hud != null)
        {
            hud.SetActive(true);
        }
    }
}