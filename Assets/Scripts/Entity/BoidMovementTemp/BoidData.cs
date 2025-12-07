using UnityEngine;

[CreateAssetMenu]
public class BoidData : ScriptableObject
{
    public float forwardMovementSpeed = 7f;
    public float maxSteerForce = 8f;

    public float maxSpeed = 8f;
    public float perceptionRadius = 10f;
    public float seperationRadius = 2f;
    public Vector3 jumpVelocity = new Vector3(0f, 15f, 0f);

    public float inputWeight = 5f;
    public float avoidanceWeight = 20f;
    public float noiseWeight = 0.05f;
    public float grouperWeight = 0.05f;

    public float limitX = 5f;

    public float deathAfterDropTime = 1.5f;
}
