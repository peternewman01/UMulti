using UnityEngine;
using Unity.Cinemachine;

public class Impactinoator : MonoBehaviour
{
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private float vfxLifetime = 2f;

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        impulseSource = gameObject.GetComponent<CinemachineImpulseSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            return;

        //approximate contact point and normal using closest point
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        Vector3 hitDirection = (contactPoint - transform.position).normalized;

        //face opposite the hit direction
        Quaternion rotation = Quaternion.LookRotation(-hitDirection);

        //camera shake
        CameraShakeManager.instance.CameraShake(impulseSource, .1f);

        GameObject vfx = Instantiate(vfxPrefab, contactPoint, rotation);
        Destroy(vfx, vfxLifetime);
    }
}
