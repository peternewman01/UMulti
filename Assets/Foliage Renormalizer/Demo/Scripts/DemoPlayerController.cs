using UnityEngine;

namespace FoliageRenormalizer.Demo
{
    public class DemoPlayerController : MonoBehaviour
    {
        public float WalkingSpeed = 3;
        public float RunningMultiplier = 1.65f;
        public float Acceleration = 5;
        public Camera[] AllCams;

        private Transform Camera;
        private Camera currentCam;
        private float YRotation;
        private CharacterController cc;
        private int currentCameraMode = 0;

        Vector2 inputDirection = Vector2.zero;
        Vector2 velocityXZ = Vector2.zero;
        Vector3 velocity = Vector3.zero;
        float speedTarget;

        private void Awake()
        {
            Camera = transform.GetChild(0);
            cc = GetComponent<CharacterController>();
            Togglecursor(true);
            SetActiveCam(0);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Togglecursor(!(Cursor.lockState == CursorLockMode.Locked));
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetActiveCam(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AllCams[1].transform.SetPositionAndRotation(currentCam.transform.position, currentCam.transform.rotation);
                SetActiveCam(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
                SetActiveCam(2);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                SetActiveCam(3);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                SetActiveCam(4);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                SetActiveCam(5);

            if (currentCameraMode != 0)
                return;
            transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
            YRotation -= Input.GetAxis("Mouse Y");
            YRotation = Mathf.Clamp(YRotation, -80, 80);
            Camera.localEulerAngles = new Vector3(YRotation, 0, 0);

            SetInput();

            Move();
        }

        void SetActiveCam(int mode)
        {
            foreach (Rotate ro in FindObjectsByType<Rotate>(FindObjectsSortMode.None))
                ro.ResetRotation();
            if (AllCams.Length <= mode)
                return;
            currentCameraMode = mode;
            foreach (Camera c in AllCams)
                c.gameObject.SetActive(false);
            AllCams[currentCameraMode].gameObject.SetActive(true);
            currentCam = AllCams[currentCameraMode];
        }

        void Togglecursor(bool locked)
        {
            if (locked)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        
        public void SetInput()
        {
            bool[] inputs = new bool[]
                {
                Input.GetKey(KeyCode.W),
                Input.GetKey(KeyCode.A),
                Input.GetKey(KeyCode.S),
                Input.GetKey(KeyCode.D),
                Input.GetKey(KeyCode.LeftShift)
                };
            speedTarget = 0;
            inputDirection = Vector2.zero;
            if (inputs[0])
            {
                inputDirection.y += 1;
                speedTarget = WalkingSpeed;
            }
            if (inputs[1])
            {
                inputDirection.x -= 1;
                speedTarget = WalkingSpeed;
            }
            if (inputs[2])
            {
                inputDirection.y -= 1;
                speedTarget = WalkingSpeed;
            }
            if (inputs[3])
            {
                inputDirection.x += 1;
                speedTarget = WalkingSpeed;
            }
            if (inputs[4])
            {
                speedTarget *= RunningMultiplier;
            }
        }
        void Move()
        {
            if (cc.isGrounded)
            {
                velocity.y = 0;
            }
            Vector2 forward = new Vector2(transform.forward.x, transform.forward.z);
            Vector2 right = new Vector2(transform.right.x, transform.right.z);
            Vector2 inputDir = Vector3.Normalize(right * inputDirection.x + forward * inputDirection.y);
            velocityXZ = Vector2.MoveTowards(velocityXZ, inputDir.normalized * speedTarget, Time.deltaTime * Acceleration);
            //velocityXZ = Vector2.ClampMagnitude(velocityXZ, speedTarget);
            velocity.x = velocityXZ.x * Time.deltaTime;
            velocity.z = velocityXZ.y * Time.deltaTime;
            velocity.y += -9.81f * Time.deltaTime * Time.deltaTime;

            cc.enabled = true;
            cc.Move(velocity);
            cc.enabled = false;
        }
    }
}