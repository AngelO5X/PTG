using UnityEngine;
using UnityEngine.Rendering; // Potrzebne jeœli u¿ywasz Volume (opcjonalnie)

public class UnderwaterEffect : MonoBehaviour
{
    [Header("Ustawienia Warstwy")]
    public LayerMask waterLayer; // Wybierz "Water" w inspektorze

    [Header("Ustawienia Podwodne")]
    public Color underwaterColor = new Color(0.22f, 0.65f, 0.77f, 0.5f); // Niebieskawy odcieñ
    public float underwaterFogDensity = 0.1f; // Gêstoœæ mg³y pod wod¹

    [Header("Ustawienia Powietrza (Domyœlne)")]
    private Color defaultFogColor;
    private float defaultFogDensity;
    private bool defaultFogEnabled;

    [Header("Efekt Wizualny (UI)")]
    public GameObject underwaterPanel; // Panel UI z niebieskim obrazkiem (opcjonalnie)

    private void Start()
    {
        // Zapisz ustawienia pocz¹tkowe (powietrza)
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
        defaultFogEnabled = RenderSettings.fog;
    }

    private void OnTriggerEnter(Collider other)
    {
        // SprawdŸ czy weszliœmy w warstwê wody (u¿ywaj¹c bitwise operation)
        if ((waterLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            EnableUnderwaterEffect();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((waterLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            DisableUnderwaterEffect();
        }
    }

    void EnableUnderwaterEffect()
    {
        // W³¹cz mg³ê i ustaw jej kolor
        RenderSettings.fog = true;
        RenderSettings.fogColor = underwaterColor;
        RenderSettings.fogDensity = underwaterFogDensity;

        // Jeœli masz shader nieba, warto zmieniæ go na jednolity kolor pod wod¹
        // RenderSettings.skybox = null; 

        // W³¹cz niebieski filtr UI (jeœli przypisany)
        if (underwaterPanel != null) underwaterPanel.SetActive(true);
    }

    void DisableUnderwaterEffect()
    {
        // Przywróæ ustawienia
        RenderSettings.fog = defaultFogEnabled;
        RenderSettings.fogColor = defaultFogColor;
        RenderSettings.fogDensity = defaultFogDensity;

        // Wy³¹cz filtr UI
        if (underwaterPanel != null) underwaterPanel.SetActive(false);
    }
}