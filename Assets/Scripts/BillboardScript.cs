using UnityEngine;

public class BillboardScript : MonoBehaviour
{
    public enum Mode { LookAt, CameraForward }

    [Header("Target")]
    public Camera targetCamera;

    [Header("Behavior")]
    public Mode mode = Mode.LookAt;
    public bool yAxisOnly = true;
    public bool invert = false;
    public float zRotationOffset = 90f;

    void LateUpdate()
    {
        var cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        if (mode == Mode.LookAt)
        {
            Vector3 toCam = cam.transform.position - transform.position;
            if (yAxisOnly) toCam.y = 0f;
            if (toCam.sqrMagnitude < 0.0001f) return;

            Vector3 forward = invert ? -toCam : toCam;
            transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            transform.Rotate(0f, 0f, zRotationOffset, Space.Self);
        }
        else // Mode.CameraForward
        {
            Vector3 camFwd = cam.transform.forward;
            if (yAxisOnly) camFwd = Vector3.ProjectOnPlane(camFwd, Vector3.up).normalized;
            Vector3 forward = invert ? -camFwd : camFwd;
            if (forward.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            transform.Rotate(0f, 0f, zRotationOffset, Space.Self);
        }
    }
}