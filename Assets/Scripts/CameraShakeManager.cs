using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void CameraShake(CinemachineImpulseSource impulseSource, float amount)
    {
        impulseSource.GenerateImpulseWithForce(amount);
    }
}
