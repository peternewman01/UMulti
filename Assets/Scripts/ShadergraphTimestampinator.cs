using UnityEngine;

public class ShadergraphTimestampinator : MonoBehaviour
{
    private ParticleSystemRenderer r;
    private void OnEnable()
    {
        r = transform.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>();
        r.material.SetFloat("LastTriggerTimestamp", Time.time);
    }
}
