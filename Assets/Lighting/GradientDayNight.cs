using UnityEngine;

public class FantasyDayNightCycle : MonoBehaviour
{
    // --- SETTINGS ---
    [Header("Time Settings")]
    public float dayDuration = 120.0f;
    [Range(0, 24)]
    public float currentTime = 12.0f;

    [Header("Sun Light")]
    public Light sunLight;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Moon Light")]
    public Light moonLight;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    [Header("Ambient Colors")]
    public Gradient ambientSky;
    public Gradient ambientEquator;
    public Gradient ambientGround;

    [Header("Fog Settings")]
    public float fogDensity = 0.02f;

    [Header("Sky Textures")]
    public Texture2D texSunrise;
    public Texture2D texNoon;
    public Texture2D texSunset;
    public Texture2D texNight;

    private float timeMultiplier;
    private Material skyboxMaterial;

    // --- INITIALIZATION ---
    void Start()
    {
        timeMultiplier = 24.0f / dayDuration;
        skyboxMaterial = RenderSettings.skybox;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensity;
    }

    // --- MAIN LOOP ---
    void Update()
    {
        currentTime += Time.deltaTime * timeMultiplier;
        if (currentTime >= 24.0f) currentTime %= 24.0f;

        float normTime = currentTime / 24.0f;

        UpdateCelestialBodies(normTime);
        UpdateSkyboxTextures();

        // Update Ambient Colors
        RenderSettings.ambientSkyColor = ambientSky.Evaluate(normTime);
        RenderSettings.ambientEquatorColor = ambientEquator.Evaluate(normTime);
        RenderSettings.ambientGroundColor = ambientGround.Evaluate(normTime);
        RenderSettings.fogColor = ambientEquator.Evaluate(normTime);
    }

    // --- CELESTIAL BODIES LOGIC ---
    void UpdateCelestialBodies(float normTime)
    {
        float angle = (normTime * 360.0f) - 90.0f;

        // Sun
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(angle, 170.0f, 0);
            sunLight.color = sunColor.Evaluate(normTime);
            sunLight.intensity = sunIntensity.Evaluate(normTime);

            bool isDay = sunLight.intensity > 0.01f;
            if (sunLight.gameObject.activeSelf != isDay) sunLight.gameObject.SetActive(isDay);
        }

        // Moon
        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(angle + 180.0f, 170.0f, 0);
            moonLight.color = moonColor.Evaluate(normTime);
            moonLight.intensity = moonIntensity.Evaluate(normTime);

            bool isNight = moonLight.intensity > 0.01f;
            if (moonLight.gameObject.activeSelf != isNight) moonLight.gameObject.SetActive(isNight);
        }
    }

    // --- SKYBOX LOGIC ---
    void UpdateSkyboxTextures()
    {
        if (skyboxMaterial == null) return;

        float blend = 0.0f;
        Texture2D t1 = null;
        Texture2D t2 = null;

        if (currentTime < 6.0f) { t1 = texNight; t2 = texSunrise; blend = currentTime / 6.0f; }
        else if (currentTime < 12.0f) { t1 = texSunrise; t2 = texNoon; blend = (currentTime - 6.0f) / 6.0f; }
        else if (currentTime < 18.0f) { t1 = texNoon; t2 = texSunset; blend = (currentTime - 12.0f) / 6.0f; }
        else { t1 = texSunset; t2 = texNight; blend = (currentTime - 18.0f) / 6.0f; }

        if (t1 && t2)
        {
            skyboxMaterial.SetTexture("_MainTex", t1);
            skyboxMaterial.SetTexture("_SecTex", t2);
            skyboxMaterial.SetFloat("_Blend", blend);
        }
    }
}