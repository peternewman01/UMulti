using CR;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;

public class PlayerObjectInteract : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;

    [SerializeField] private CustomPassVolume outlinePasses;
    [SerializeField] private CleanOutlineCustomPass blackPass;
    [SerializeField] private CleanOutlineCustomPass goldPass;

    [SerializeField] private float searchRadius = 3f;

    [SerializeField] private List<Renderer> objectRenderers = new List<Renderer>();
    [SerializeField] private GameObject HighlightedObject = null;
    private Renderer HighlightedObjectRenderer;

    [SerializeField] private SphereCollider pickupRadius;

    [SerializeField] private int positionOnOrder = 0;

    private bool scroll;

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
        checkScroll();

        blackPass.m_DrawRenderers = objectRenderers;

        goldPass.m_DrawRenderers.Clear();
        goldPass.m_DrawRenderers.Add(HighlightedObjectRenderer);

        if (playerManager.Interact)
        {
            if (HighlightedObject.TryGetComponent<Object>(out var o))
            {
                o.Interact();
            }
        }

    }

    private void checkScroll()
    {
        if(scroll)
        {
            if(playerManager.scrolling > 0)
            {
                scroll = false;

                ScrollHighlightUp();
            }
            else if(playerManager.scrolling < 0)
            {
                scroll = false;

                ScrollHighlightDown();
            }
        }
        else if(playerManager.scrolling == 0)
        {
            scroll = true;
        }

    }

    private void ScrollHighlightUp()
    {
        positionOnOrder--;
        if(positionOnOrder < 0)
        {
            positionOnOrder = objectRenderers.Count() - 1;
        }

        HighlightedObject = objectRenderers[positionOnOrder].gameObject;
        HighlightedObjectRenderer = objectRenderers[positionOnOrder];
    }
    private void ScrollHighlightDown()
    {
        positionOnOrder++;
        if (positionOnOrder >= objectRenderers.Count())
        {
            positionOnOrder = 0;
        }

        HighlightedObject = objectRenderers[positionOnOrder].gameObject;
        HighlightedObjectRenderer = objectRenderers[positionOnOrder];
    }

    private void OnTriggerEnter(Collider col)
    {
        Object obj = col.GetComponent<Object>();
        if (obj)
        {
            Renderer r = col.GetComponent<Renderer>();
            if (HighlightedObject == null)
            {
                HighlightedObject = col.gameObject;
                objectRenderers.Add(r);

                positionOnOrder = objectRenderers.Count-1;
                HighlightedObjectRenderer = r;
            }
            else if (!objectRenderers.Contains(r))
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
            if (col.gameObject == HighlightedObject)
            {
                HighlightedObject = null;
                objectRenderers.Remove(r);

                positionOnOrder -= 1;
                HighlightedObjectRenderer = null;
            }
            else if (objectRenderers.Contains(r))
            {
                objectRenderers.Remove(r);
            }
        }
    }
}
