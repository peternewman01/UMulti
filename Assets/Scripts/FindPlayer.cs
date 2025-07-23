using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class FindPlayer : NetworkBehaviour
{
    //wait till find a player prefab that IsOwner and set cinemachine freelook's target to player
    private CinemachineCamera freeLookCam;
    private CinemachineCamera aimCam;
    private CameraPointer cp;

    private void OnEnable()
    {
        freeLookCam = transform.GetComponent<CinemachineCamera>();
        cp = transform.parent.GetComponent<CameraPointer>();
        aimCam = cp.aimCam;
    }

    private void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    private System.Collections.IEnumerator WaitForLocalPlayer()
    {
        while (true)
        {
            foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                if (obj.IsOwner)
                {
                    obj.GetComponent<PlayerManager>().cameraTransform = transform;
                    if(aimCam != null) obj.GetComponent<PlayerManager>().aimCamTransform = aimCam.transform;
                    Transform target = obj.transform.Find("CameraTrackingTarget");

                    //Debug.Log(obj.name + " added");
                    if (freeLookCam != null)
                    {
                        freeLookCam.Target.TrackingTarget = target;
                        if (aimCam != null) aimCam.Target.TrackingTarget = target;
                        yield break; // Done
                    }
                }
            }
            yield return null;
        }
    }
}
