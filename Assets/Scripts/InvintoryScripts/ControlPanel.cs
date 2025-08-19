using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    public GameObject slot;
    public Transform startPosition;
    [SerializeField] private List<Slot> slots = new List<Slot>();

    [SerializeField] private int slotSpawnCount = 63;

    [Header("Player Stuff")]
    public PlayerManager playerManager;
    public Invintory invintory;

    private void Start()
    {
        for(int i = 0; i <  slotSpawnCount; i++)
        {
            slots.Add(Instantiate(slot, startPosition).GetComponent<Slot>());
        }
    }
}
