using System;
using System.Collections;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

namespace UseEntity
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavEntity : Entity, INavEntity
    {
        public NavMeshAgent agent;
        public Rigidbody rb;
        [SerializeField] private float wanderRadius;
        [SerializeField] private float wanderMinimumTime = 5;
        [SerializeField] private float wanderMaximumTime = 15;
        [SerializeField] private float wanderReachDistance = 1;
        [SerializeField] private bool wandering = false;
        [SerializeField] private bool waiting = false;

        private NavMeshSurface surface;
        [SerializeField] private LayerMask surfaceLayer;
        [SerializeField] private Vector3 navSurfaceSize;
        [SerializeField] private int navSurfaceCount = -1;


        //TODO: Implement basic rigidbody controls through function calls
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (agent.velocity.magnitude != 0)
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

            //rebaking should just be a big area, then if the entity moves away from that area it moves to that and sets a new bake position
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

            if (!agent.SetDestination(wanderTarget))
            {
                EnsureNavMeshSurface();
                agent.SetDestination(wanderTarget);
            }

        }

        void EnsureNavMeshSurface()
        {
            NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();
            this.surface = surface;

            if (surface == null)
            {
                GameObject surfaceObj = new GameObject("RuntimeNavMeshSurface");
                surface = surfaceObj.AddComponent<NavMeshSurface>();

                surface.layerMask = surfaceLayer;
                surface.agentTypeID = agent.agentTypeID;
                surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                surface.size = navSurfaceSize;
            }
            surface.BuildNavMesh();
            navSurfaceCount = NavMesh.CalculateTriangulation().areas.Length;
        }
    }


}
