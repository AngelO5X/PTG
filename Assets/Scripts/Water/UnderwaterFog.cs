using UnityEngine;
using Mirror;

public class UnderwaterFogBuiltin : NetworkBehaviour
{
    public Color underwaterFogColor = new Color(0f, 0.35f, 0.45f);
    public float underwaterFogDensity = 0.05f;

    Color defaultColor;
    float defaultDensity;

    void Start()
    {
        if (!isLocalPlayer) return;

        defaultColor = RenderSettings.fogColor;
        defaultDensity = RenderSettings.fogDensity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;
        if (!other.CompareTag("WaterTrigger")) return;

        RenderSettings.fogColor = underwaterFogColor;
        RenderSettings.fogDensity = underwaterFogDensity;
        Debug.Log("ENTER: " + other.name);
    }

    void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer) return;
        if (!other.CompareTag("WaterTrigger")) return;

        RenderSettings.fogColor = defaultColor;
        RenderSettings.fogDensity = defaultDensity;
    }
}
