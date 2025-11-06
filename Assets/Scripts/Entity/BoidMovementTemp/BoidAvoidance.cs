using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Boid))]
public class BoidAvoidance : MonoBehaviour
{
    //[SerializeField] LayerMask _mask;
    [SerializeField] float _avoidanceRadius = 1f;
    [SerializeField] Vector3 _avoidanceOffset = new Vector3(0f, 0f, 2f);
    [SerializeField] float _avoidanceRange = 2f;
    [SerializeField] float _avoidancePower = 3f;
    [SerializeField] float _jumpPower = 3f;
    private Rigidbody rb;
    private Vector3 startForward;

    Boid _boid;

    private void Awake()
    {
        _boid = GetComponent<Boid>();
        rb = GetComponent<Rigidbody>();
        startForward = transform.forward;

    }

    //NOTE: the weird turn thing isnt happening here -h
    private void Avoid()
    {
        if (Physics.SphereCast(transform.position + (Quaternion.FromToRotation(startForward, transform.forward) * _avoidanceOffset), _avoidanceRadius, transform.forward, out RaycastHit hit, _avoidanceRange))
        {
            /*if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                rb.AddForce(Vector3.up.normalized * _jumpPower);
                StartCoroutine(DelayPush(transform.forward.normalized * _jumpPower / 2, 0.2f));
                Debug.Log("jump");
            }
            else */if (hit.collider.gameObject != gameObject)
            {
                var hitPos = new Vector3(hit.transform.position.x, transform.position.y, hit.transform.position.z);
                var thisPos = transform.position;
                var vec = Vector3.zero;

                Vector3 local = transform.InverseTransformPoint(Quaternion.FromToRotation(_avoidanceOffset, transform.forward) * _avoidanceOffset).normalized;
                if (local.x < -9) Debug.Log("Right");
                else Debug.Log("Left");

                if (local.x < -0.8)
                {
                    vec = new Vector3(1, 0f, 0f);
                }
                else
                {
                    vec = new Vector3(-1, 0f, 0f);
                }
                Vector3 between = Vector3.Slerp(_boid.GetVelocity(), vec.normalized, 0.5f);
                _boid.Steer(between, _boid.Data.avoidanceWeight, _avoidancePower);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Quaternion.FromToRotation(startForward, transform.forward) * _avoidanceOffset), _avoidanceRadius);
    }

    private void Update()
    {
        Avoid();
    }

    public IEnumerator DelayPush(Vector3 force, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        transform.position += (force) * Time.deltaTime;
        Debug.Log("push");
    }

    public Vector3 getOffsetPosition()
    {
        Vector3 offsetPosition = transform.position + (Quaternion.FromToRotation(startForward, transform.forward) * _avoidanceOffset);
        return offsetPosition;
    }
}
