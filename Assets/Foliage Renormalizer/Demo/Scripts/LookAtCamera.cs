using UnityEngine;

namespace FoliageRenormalizer.Demo
{
    public class LookAtCamera : MonoBehaviour
    {
        [Tooltip("Rotate only around Y to keep text upright.")]
        public bool onlyRotateAroundY = true;

        private Camera targetCamera;

        void LateUpdate()
        {
            targetCamera = Camera.main;

            if (targetCamera == null) return;
            Vector3 dir = targetCamera.transform.position - transform.position;
            if (onlyRotateAroundY)
            {
                Vector3 camPos = targetCamera.transform.position;
                Vector3 lookPos = new Vector3(camPos.x, transform.position.y, camPos.z);
                dir = lookPos - transform.position;
            }

            dir = -dir;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}