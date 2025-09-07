using UnityEngine;

public class Impactinoator : MonoBehaviour
{
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private float vfxLifetime = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            return;

        //approximate contact point and normal using closest point
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        Vector3 hitDirection = (contactPoint - transform.position).normalized;

        //face opposite the hit direction
        Quaternion rotation = Quaternion.LookRotation(-hitDirection);

        GameObject vfx = Instantiate(vfxPrefab, contactPoint, rotation);
        Destroy(vfx, vfxLifetime);
    }
}
