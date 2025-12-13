using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisionCheck : MonoBehaviour
{
    public Dictionary<PlayerManager, Vector3> RecentSeenPlayer = new Dictionary<PlayerManager, Vector3>();
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.TryGetComponent<PlayerManager>(out PlayerManager p))
        {
            if(RecentSeenPlayer.TryGetValue(p, out Vector3 CurrentPosition))
            {
                CurrentPosition = p.transform.position;
            }
            else
            {
                RecentSeenPlayer.Add(p, p.transform.position);
            }
            
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.TryGetComponent<PlayerManager>(out PlayerManager p))
        {
            if (RecentSeenPlayer.TryGetValue(p, out Vector3 CurrentPosition))
            {
                RecentSeenPlayer.Remove(p);
            }
        }
    }
}
