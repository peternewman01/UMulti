using Unity.Cinemachine;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField, Range(0,1000)] private float intensityPower = 1f;

    private Light lightSource;
    private float lightInitialIntensity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        lightSource = transform.GetComponent<Light>();
        lightInitialIntensity = lightSource.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        float lightIntensity = Mathf.Sin(Time.time) * intensityPower * lightInitialIntensity;
        lightSource.intensity = lightIntensity;
    }
}
