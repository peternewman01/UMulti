using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class WeatherSystem : MonoBehaviour
{
    [SerializeField] private float minFogAttenuation = 10f;
    [SerializeField] private float maxFogAttenuation = 70f;
    [SerializeField] private float transitionSpeed = 0.25f;
    [SerializeField] private Transform weatherSpawn;

    [SerializeField] private WeatherPreset[] weathers;
    private int weatherIndex;

    private Volume gameVolume;
    private Fog fog;
    private VolumetricClouds clouds;

    private WeatherPreset currentPreset;
    private WeatherPreset targetPreset;

    private bool isTransitioning;
    private float transitionT;

    private float currentWind;

    private GameObject currentWeatherInstance;
    private GameObject targetWeatherInstance;

    private void Start()
    {
        gameVolume = FindFirstObjectByType<Volume>();

        if (gameVolume.profile.TryGet(out fog))
            fog.meanFreePath.overrideState = true;

        if (gameVolume.profile.TryGet(out clouds))
        {
            clouds.densityMultiplier.overrideState = true;
            clouds.densityCurve.overrideState = true;
            clouds.shapeFactor.overrideState = true;
            clouds.shapeScale.overrideState = true;
            clouds.erosionFactor.overrideState = true;
            clouds.erosionScale.overrideState = true;
            clouds.ambientOcclusionCurve.overrideState = true;
        }

        weatherIndex = 0;
        currentPreset = weathers[weatherIndex];

        ApplyPresetInstant(currentPreset);
        SpawnInitialWeatherPrefab(currentPreset);
    }

    private void Update()
    {
        if (isTransitioning)
        {
            TransitionWeather();
            return;
        }
        else
            TickActiveWeather(currentPreset);
    }

    void TickActiveWeather(WeatherPreset currWeather)
    {
        if (isTransitioning) return;
        currWeather.currentValue = Mathf.Clamp01(currWeather.currentValue + Random.Range(-1f, 1f) * Time.deltaTime);
        if (currWeather.currentValue <= 0.05f)
            RegressWeather();

        if (currWeather.currentValue >= .95f)
            ProgressWeather();
    }

    void ProgressWeather()
    {
        if (weatherIndex >= weathers.Length - 1) return;
        targetPreset = weathers[++weatherIndex];
        BeginTransition();
    }

    void RegressWeather()
    {
        if (weatherIndex <= 0) return;
        targetPreset = weathers[--weatherIndex];
        BeginTransition();
    }

    void BeginTransition()
    {
        isTransitioning = true;
        transitionT = 0f;

        if (targetPreset.weatherPrefab != null)
        {
            targetWeatherInstance = Instantiate(
                targetPreset.weatherPrefab,
                weatherSpawn.position,
                Quaternion.identity,
                weatherSpawn
            );
        }
    }

    void TransitionWeather()
    {
        transitionT += Time.deltaTime * transitionSpeed;
        float t = Mathf.Clamp01(transitionT);

        // Fog
        fog.meanFreePath.value = Mathf.Lerp(
            Mathf.Lerp(minFogAttenuation, maxFogAttenuation, currentPreset.fogIntensity),
            Mathf.Lerp(minFogAttenuation, maxFogAttenuation, targetPreset.fogIntensity),
            t
        );

        // Clouds
        clouds.densityMultiplier.value = Mathf.Lerp(
            currentPreset.cloudDensity,
            targetPreset.cloudDensity,
            t
        );

        clouds.shapeFactor.value = Mathf.Lerp(
            currentPreset.cloudShapeFactor,
            targetPreset.cloudShapeFactor,
            t
        );

        //clouds.shapeScale.value = LogLerp(
        //    currentPreset.cloudShapeScale,
        //    targetPreset.cloudShapeScale,
        //    t);

        //clouds.erosionScale.value =LogLerp(
        //    currentPreset.cloudErosionScale,
        //    targetPreset.cloudErosionScale,
        //    t);


        clouds.erosionFactor.value = Mathf.Lerp(
            currentPreset.cloudErosionFactor,
            targetPreset.cloudErosionFactor,
            t
        );

        if (t > 0.5f)
        {
            clouds.densityCurve.value = targetPreset.cloudDensityCurve;
            clouds.ambientOcclusionCurve.value = targetPreset.cloudAmbientOcclusionCurve;
        }

        // Lighting
        RenderSettings.ambientIntensity = Mathf.Lerp(
            1f - currentPreset.cloudDarkness,
            1f - targetPreset.cloudDarkness,
            t
        );

        //RenderSettings.sun.intensity = Mathf.Lerp(
        //    1f - currentPreset.cloudDarkness,
        //    1f - targetPreset.cloudDarkness,
        //    t
        //);

        // Wind
        currentWind = Mathf.Lerp(
            currentPreset.windIntensity,
            targetPreset.windIntensity,
            t
        );

        if (t >= 1f)
        {
            if (currentWeatherInstance != null)
                Destroy(currentWeatherInstance);

            currentWeatherInstance = targetWeatherInstance;
            targetWeatherInstance = null;

            currentPreset = targetPreset;
            currentPreset.currentValue = currentPreset.initialValue;

            isTransitioning = false;
        }
    }

    void ApplyPresetInstant(WeatherPreset preset)
    {
        fog.meanFreePath.value =
            Mathf.Lerp(minFogAttenuation, maxFogAttenuation, preset.fogIntensity);

        clouds.densityMultiplier.value = preset.cloudDensity;
        clouds.densityCurve.value = preset.cloudDensityCurve;
        clouds.shapeFactor.value = preset.cloudShapeFactor;
        clouds.shapeScale.value = preset.cloudShapeScale;
        clouds.erosionFactor.value = preset.cloudErosionFactor;
        clouds.erosionScale.value = preset.cloudErosionScale;
        clouds.ambientOcclusionCurve.value = preset.cloudAmbientOcclusionCurve;

        //RenderSettings.ambientIntensity = 1f - preset.cloudDarkness;
        //RenderSettings.sun.intensity = 1f - preset.cloudDarkness;

        currentWind = preset.windIntensity;
    }

    void SpawnInitialWeatherPrefab(WeatherPreset preset)
    {
        if (preset.weatherPrefab == null) return;

        currentWeatherInstance = Instantiate(
            preset.weatherPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );
    }
}
