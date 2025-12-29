using UnityEngine;

[CreateAssetMenu(fileName = "WeatherPreset", menuName = "Scriptable Objects/WeatherPreset")]
public class WeatherPreset : ScriptableObject
{
    public string weatherName;
    public float initialValue;
    public float currentValue;

    [Header("Fog")]
    [Range(0f, 1f)] public float fogIntensity = 0f;

    [Header("Clouds")]
    [Range(0f, 2f)] public float cloudDensity = 1f;
    public AnimationCurve cloudDensityCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0f, 1f)] public float cloudShapeFactor = 0.5f;
    public float cloudShapeScale = 1f;
    [Range(0f, 1f)] public float cloudErosionFactor = 0.5f;
    public float cloudErosionScale = 1f;
    public AnimationCurve cloudAmbientOcclusionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Lighting")]
    [Range(0f, 1f)] public float cloudDarkness = 0f;

    [Header("Wind")]
    [Range(0f, 1f)] public float windIntensity = 0f;

    public GameObject weatherPrefab;
}
