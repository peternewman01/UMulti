using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UseEntity;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Boid))]
public class PassiveEntity : Entity
{
    [SerializeField] float radius = 5f;
    [SerializeField] float timeToWander = 8f;
    [SerializeField] Boid boid;
    [SerializeField] LayerMask groundMask;

    private void Start()
    {
        boid = GetComponent<Boid>();
        StartCoroutine(Wander(timeToWander));

    }


    /*    IEnumerator Wander(float delay)
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);

                Vector3 target = transform.position + (Random.onUnitSphere * radius);
                target.y = transform.position.y;

                boid.SetTarget(target);
            }
        }*/

    private IEnumerator Wander(float delay)
    {
        while (true)
        {
            Vector3 target = transform.position + (Random.onUnitSphere * radius);
            target.y = 10000;

            if (Physics.Raycast(target, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundMask))
            {
                if(hit.normal != Vector3.up)
                {
                    break;
                }
                target = hit.point;
                target.y += 1f;
                boid.SetTarget(hit.point);
            }
            else
            {
                Debug.LogError("Failed To Find Ground");
            }

            yield return new WaitForSeconds(delay);
        }
        StartCoroutine(Wander(timeToWander));
    }

}
