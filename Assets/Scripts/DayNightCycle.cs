using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    public Light sunLight;

    [Header("Settings")]
    [Range(0, 180)] public float timeOfDay = 0f; // 0–360 full rotation
    public float dayDuration = 120f; // seconds per full cycle
    public Gradient sunColorGradient;
    public float sunIntensity = 1f;
    public float moonIntensityMultiplier = 0.33f;

    private float rotationSpeed;
    private bool isDay = true;

    void Start()
    {
        rotationSpeed = 180 / dayDuration;

        if (sunColorGradient == null)
        {
            sunColorGradient = new Gradient
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 0.85f, 0.6f), 0f),     // sunrise
                    new GradientColorKey(Color.white, 0.25f),                 // noon
                    new GradientColorKey(new Color(1f, 0.55f, 0.3f), 0.5f),   // sunset
                    new GradientColorKey(new Color(0.5f, 0.6f, 0.8f), 0.75f), // night cool
                    new GradientColorKey(new Color(0.3f, 0.4f, 0.6f), 1f)     // midnight
                }
            };
        }
    }

    void Update()
    {
        timeOfDay += rotationSpeed * Time.deltaTime;
        if (timeOfDay >= 179f)
        {
            isDay = !isDay;
            timeOfDay -= 180f;
        }

        // Rotate light according to time
        sunLight.transform.localRotation = Quaternion.Euler(timeOfDay, 170f, 0);

        // Intensity based on day/night
        float intensityMultiplier = 0f;
        Color lightColor = Color.white;

        if (isDay) // Daytime: sunrise to sunset
        {
            intensityMultiplier = Mathf.Clamp01(Mathf.Sin(timeOfDay * Mathf.Deg2Rad));
            lightColor = sunColorGradient.Evaluate(timeOfDay / 360f);
        }
        else // Nighttime: moonrise to moonset
        {
            float nightAngle = timeOfDay;
            intensityMultiplier = Mathf.Clamp01(Mathf.Sin(nightAngle * Mathf.Deg2Rad)) * moonIntensityMultiplier;
            lightColor = Color.Lerp(
                new Color(0.6f, 0.7f, 1f),  // moonlight (cooler)
                new Color(0.3f, 0.4f, 0.6f), // deep night
                Mathf.Sin(nightAngle * Mathf.Deg2Rad)
            );
        }

        sunLight.intensity = sunIntensity * intensityMultiplier;
        sunLight.color = lightColor;
    }
}
