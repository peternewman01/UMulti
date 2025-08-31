using System;
using System.Collections;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(NavMeshAgent))]
public class NavEntity : Entity
{
    public NavMeshAgent agent;
    public Rigidbody rb;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderMinimumTime = 5;
    [SerializeField] private float wanderMaximumTime = 15;
    [SerializeField] private float wanderReachDistance = 1;
    [SerializeField] private bool wandering = false;
    [SerializeField] private bool waiting = false;

    [SerializeField] private LayerMask surfaceLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        transform.forward = agent.velocity;

        if (!wandering && !waiting)
        {
            wandering = true;
            StartCoroutine(Wander());
        }
        else if (Vector3.Distance(transform.position, agent.destination) < wanderReachDistance)
        {
            wandering = false;
        }
    }

    public IEnumerator Wander()
    {
        float wanderDelay = UnityEngine.Random.Range(wanderMaximumTime, wanderMinimumTime);
        waiting = true;
        yield return new WaitForSeconds(wanderDelay);
        waiting = false;

        Vector3 wanderTarget = transform.position + UnityEngine.Random.insideUnitSphere * wanderRadius;

        bool hold = NavMesh.SamplePosition(wanderTarget, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas);
        wanderTarget = hold ? hit.position : transform.position;

        if(!agent.SetDestination(wanderTarget))
        {
            EnsureNavMeshSurface();
            agent.SetDestination(wanderTarget);
        }

    }

    void EnsureNavMeshSurface()
    {
        NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();

        if (surface == null)
        {
            GameObject surfaceObj = new GameObject("RuntimeNavMeshSurface");
            surface = surfaceObj.AddComponent<NavMeshSurface>();

            surface.layerMask = surfaceLayer;
            surface.agentTypeID = agent.agentTypeID;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        }
        surface.BuildNavMesh();
    }
}
