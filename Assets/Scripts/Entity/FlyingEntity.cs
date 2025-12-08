using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UseEntity;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Boid))]
public class FlyingEntity : Entity
{
    private const float START_DIVE_ADD = -0.1f;

    [SerializeField] private Vector3 flyingDirection;
    [SerializeField] private float maxYAngle = 30f;
    [SerializeField] private float minYAngle = -75f;
    [SerializeField] private float diveAccelleration = 0.25f;
    private float currentDiveAdd = START_DIVE_ADD;
    [SerializeField] private float currentDiveMaxSpeed = 3;

    [SerializeField] float positionRadius = 5f;
    [SerializeField] float desiredOrbitRadius = 1.5f;
    [SerializeField] float orbitSpringStrength = 2f; 
    [SerializeField] float minHeight = 3f;
    [SerializeField] float maxHeight = 8f;
    [SerializeField] float timeToWander = 8f;
    [SerializeField] Boid boid;
    [SerializeField] LayerMask groundMask;

    private bool startPeriod = true;
    private float startPeriodTime = 1f;

    private float magnitudeThreshold = 0.1f;
    private Vector3 target;

    private Rigidbody rb;

    private void Start()
    {
        flyingDirection = transform.forward.normalized;

        boid = GetComponent<Boid>();
        StartCoroutine(Wander(timeToWander));

        Invoke("EndStartPeriod", startPeriodTime);
        
        rb = GetComponent<Rigidbody>();
    }

    private void EndStartPeriod() { startPeriod = false; }

    private void Update()
    {
        rb.linearVelocity = Vector3.zero;

        if (boid.wasNullInPeriod && !boid.HasNewTarget)
        {
            boid.Movement.OnJump(); 
            StartCoroutine(boid.Avoidance.DelayPush((boid.target - transform.position).normalized * boid.Data.forwardMovementSpeed * 6, 0.2f));
        }
        boid.Steer(flyingDirection * boid.Data.forwardMovementSpeed);

        //bad, should rely on behavior stuff
        if(Vector3.Distance(target, transform.position) < boid.StopDist)
        {
            Vector3 target = transform.position + (Random.onUnitSphere * positionRadius);
            target.y = 10000;

            if (Physics.Raycast(target, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundMask))
            {
                target = hit.point;
                target.y += Random.Range(minHeight, maxHeight);
                this.target = target;
            }
            else
            {
                Debug.LogError("Failed To Find Ground");
            }
        }
        Vector3 normalDirectionToPoint = (target - transform.position).normalized;
        flyingDirection = Vector3.Slerp(flyingDirection, normalDirectionToPoint, Time.deltaTime * 2);


        float maxY = Mathf.Sin(maxYAngle * Mathf.Deg2Rad);
        float minY = Mathf.Sin(minYAngle * Mathf.Deg2Rad);
        flyingDirection.y = Mathf.Clamp(flyingDirection.y, minY, maxY);
        ApplyOrbitalSpring();
        CheckDiveAccelleration();
    }

    private IEnumerator Wander(float delay)
    {
        while (true)
        {
            Vector3 target = transform.position + (Random.onUnitSphere * positionRadius);
            target.y = 10000;

            if (Physics.Raycast(target, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundMask))
            {
                if(hit.normal != Vector3.up)
                {
                    break;
                }
                target = hit.point;
                target.y += Random.Range(minHeight, maxHeight);
                this.target = target;
            }
            else
            {
                Debug.LogError("Failed To Find Ground");
            }

            yield return new WaitForSeconds(delay);
        }
        StartCoroutine(Wander(timeToWander));
    }
    private void ApplyOrbitalSpring()
    {
        Vector3 toTarget = target - transform.position;
        float dist = Vector3.Distance(target, transform.position);
        float radialError = dist - desiredOrbitRadius;

        if(dist < desiredOrbitRadius)
        {
            Vector3 radialDir = toTarget.normalized;
            
            Vector3 springForce = radialDir * (radialError * orbitSpringStrength);
            flyingDirection += springForce * Time.deltaTime;
        }
    }

    private void CheckDiveAccelleration()
    {
        if(flyingDirection.normalized.y < 0)
        {
            currentDiveAdd += diveAccelleration;

            boid.Steer(Vector3.down * Mathf.Min(currentDiveAdd, currentDiveMaxSpeed));
        }
        else
        {
            currentDiveAdd = 0;
        }
    }
}
