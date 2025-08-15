using CR;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using System.Drawing;
using UnityEngine.UIElements;

public class PlayerObjectInteract : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;

    [SerializeField] private CustomPassVolume outlinePasses;
    [SerializeField] private CleanOutlineCustomPass blackPass;
    [SerializeField] private CleanOutlineCustomPass goldPass;

    [SerializeField] private float searchRadius = 3f;

    [SerializeField] private List<Renderer> objectRenderers = new List<Renderer>();

    [SerializeField] private SphereCollider pickupRadius;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();

        outlinePasses = GameObject.FindFirstObjectByType<CustomPassVolume>();

        foreach (CustomPass cp in outlinePasses.customPasses)
        {
            if (cp is CleanOutlineCustomPass cleanPass)
            {
                if (cp.name == "BlackOutline")
                {
                    blackPass = cleanPass;
                }
                else if (cp.name == "GoldOutline")
                {
                    goldPass = cleanPass;
                }
            }
        }

        if(!pickupRadius)
        {
            pickupRadius = gameObject.AddComponent<SphereCollider>();
            pickupRadius.radius = searchRadius;
            pickupRadius.isTrigger = true;
        }

    }

    void Update()
    {

        blackPass.m_DrawRenderers = objectRenderers;

        Renderer highlighted = ClosestRendererToCameraForward();

        goldPass.m_DrawRenderers.Clear();
        goldPass.m_DrawRenderers.Add(highlighted);

        if (playerManager.Interact)
        {
            if (highlighted.TryGetComponent<Object>(out var o))
            {
                o.Invintory = this.GetComponent<Invintory>();
                o.Interact();
            }
        }

    }

    private void OnTriggerEnter(Collider col)
    {
        Object obj = col.GetComponent<Object>();
        if (obj)
        {
            Renderer r = col.GetComponent<Renderer>();
            if (!objectRenderers.Contains(r))
            {
                objectRenderers.Add(r);
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        Object obj = col.GetComponent<Object>();
        if (obj)
        {
            Renderer r = col.GetComponent<Renderer>();
            if (objectRenderers.Contains(r))
            {
                objectRenderers.Remove(r);
            }
        }
    }

    private Renderer ClosestRendererToCameraForward()
    {
        if (objectRenderers.Count > 0)
        {
            Camera cam = Camera.main;
            if (cam == null) return null;

            Renderer closestRenderer = null;
            float rDist = -1f;
            Renderer removeHold = objectRenderers[0];
            bool removeHoldSet = false;

            foreach (Renderer r in objectRenderers)
            {
                if (r)
                {
                    Vector3 toPoint = r.transform.position - cam.transform.position;
                    float dist = Vector3.Cross(toPoint, cam.transform.forward.normalized).magnitude;

                    if (rDist < 0f || dist < rDist)
                    {
                        rDist = dist;
                        closestRenderer = r;
                    }
                }
                else
                {
                    removeHold = r;
                    removeHoldSet = true;
                }
            }
            if (removeHoldSet)
            {
                objectRenderers.Remove(removeHold);
            }
            return closestRenderer;
        }

        return null;
    }

}
