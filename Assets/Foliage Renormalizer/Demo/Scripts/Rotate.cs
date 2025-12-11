using UnityEngine;

namespace FoliageRenormalizer.Demo
{
    public class Rotate : MonoBehaviour
    {
        public float Speed = 15;

        private Quaternion initialRot;

        private void Awake()
        {
            initialRot = transform.rotation;
        }

        void Update()
        {
            transform.Rotate(Vector3.up * Time.deltaTime * Speed);
        }

        public void ResetRotation()
        {
            transform.rotation = initialRot;
        }
    }
}