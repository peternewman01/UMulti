using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DayNightCycle : MonoBehaviour
{
    public Light sunLight;

    [Range(0, 180)] public float timeOfDay = 0f; //0–180 full day, is passed to shader graph
    [SerializeField] private float dayDuration = 120f; //seconds per full cycle, i think we should do 10 min days, but am open to persuasion, @ me lol
    [SerializeField] private Gradient sunColorGradient;
    [SerializeField] private float sunIntensity = 1f;
    [SerializeField] private float moonIntensityMultiplier = 0.33f;

    private float rotationSpeed;
    private bool isDay = true; //is also passed to shader graph

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

        //rotate light according to time
        sunLight.transform.localRotation = Quaternion.Euler(timeOfDay, 110f, 0);

        //intensity based on day/night
        float intensityMultiplier = 0f;
        Color lightColor = Color.white;

        if (isDay) //daytime: sunrise to sunset
        {
            intensityMultiplier = Mathf.Clamp01(Mathf.Sin(timeOfDay * Mathf.Deg2Rad));
            lightColor = sunColorGradient.Evaluate(timeOfDay / 180f);
        }
        else //nighttime: moonrise to moonset
        {
            float nightAngle = timeOfDay;
            intensityMultiplier = (1f - Mathf.Clamp01(Mathf.Sin(nightAngle * Mathf.Deg2Rad))) * moonIntensityMultiplier;
            lightColor = Color.Lerp(
                new Color(0.6f, 0.7f, 1f),  //moonlight(cooler color)
                new Color(0.3f, 0.4f, 0.6f), //deep night
                Mathf.Sin(nightAngle * Mathf.Deg2Rad)
            );
        }

        sunLight.intensity = sunIntensity * intensityMultiplier;
        sunLight.color = lightColor;
    }
}
