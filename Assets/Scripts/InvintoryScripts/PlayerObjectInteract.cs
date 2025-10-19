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
    [SerializeField] private string selectPassName;
    [SerializeField] private string inRangePassName;
    [SerializeField] private float searchRadius = 3f;
    [SerializeField] private SphereCollider pickupRadius;

    private List<UseEntity.Interactable> nearbyInteractables = new List<UseEntity.Interactable>();
    private CleanOutlineCustomPass selectPass;
    private CleanOutlineCustomPass inRangePass;
    private Camera mainCameraRef ;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        mainCameraRef = Camera.main;
        outlinePasses = FindFirstObjectByType<CustomPassVolume>();

        foreach (CustomPass cp in outlinePasses.customPasses)
        {
            if (cp is CleanOutlineCustomPass cleanPass)
            {
                if (cp.name == selectPassName) selectPass = cleanPass;
                else if (cp.name == inRangePassName) inRangePass = cleanPass;
            }
        }

        if(!pickupRadius)
        {
            pickupRadius = gameObject.AddComponent<SphereCollider>();
            pickupRadius.radius = searchRadius;
            pickupRadius.isTrigger = true;
        }

        selectPass.m_DrawRenderers.Add(null);

    }

    void Update()
    {
        UseEntity.Interactable closestInteractable = GetClosestInteractable();
        if(closestInteractable == null)
        {
            selectPass.m_DrawRenderers[0] = null;
            return;
        }

        selectPass.m_DrawRenderers[0] = closestInteractable.GetComponent<Renderer>();
        if (playerManager.Interact)
        {
            nearbyInteractables.Remove(closestInteractable);
            closestInteractable.Interact(playerManager);
        }

    }

    private void OnTriggerEnter(Collider col)
    {
        UseEntity.Interactable obj = col.GetComponentInParent<UseEntity.Interactable>();
        if (obj == null) return;
        nearbyInteractables.Add(obj);
        inRangePass.m_DrawRenderers.Add(obj.GetComponent<Renderer>());
    }

    private void OnTriggerExit(Collider col)
    {
        UseEntity.Interactable obj = col.GetComponent<UseEntity.Interactable>();
        if (obj == null) return;
        nearbyInteractables.Remove(obj);
        inRangePass.m_DrawRenderers.Remove(obj.GetComponent<Renderer>());
    }

    //TODO: make based on Physically closesest -h
    private UseEntity.Interactable GetClosestInteractable()
    {
        UseEntity.Interactable closestInteractable = null;
        float closestValue = float.PositiveInfinity;
        foreach(UseEntity.Interactable interactable in nearbyInteractables)
        {
            if(interactable == null)
            {
                nearbyInteractables.Remove(interactable);
                continue;
            }

            float dist = Vector3.Distance(interactable.transform.position, playerManager.transform.position);
            if (closestValue > dist)
            {
                closestInteractable = interactable;
                closestValue = dist;
            }
        }

        return closestInteractable;
    }

    /*private Renderer ClosestRendererToCameraForward()
    {
        if (nearbyInteractables.Count > 0)
        {
            Camera cam = Camera.main;
            if (cam == null) return null;

            Renderer closestRenderer = null;
            float rDist = -1f;
            Renderer removeHold = nearbyInteractables[0];
            bool removeHoldSet = false;

            foreach (Renderer r in nearbyInteractables)
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
                nearbyInteractables.Remove(removeHold);
            }
            return closestRenderer;
        }

        return null;
    }*/

}
