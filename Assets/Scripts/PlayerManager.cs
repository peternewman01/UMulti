using Newtonsoft.Json.Serialization;
using NUnit;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PlayerManager : NetworkBehaviour
{
    public InputActionAsset InputActions;
    public Canvas MainCanvas;
    [SerializeField] private GameObject ControlPanelPrefab;
    [HideInInspector] public ControlPanel controlPanel;
    [SerializeField] private Invintory inv;
    private Transform spawnedSlash;
    private Vector2 previousMousePosition;
    private bool isMouseMoving;

    [Header("Movement")]
    [SerializeField] private Rigidbody rb;

    [SerializeField] private float walkingSpeed = 10f;
    [SerializeField] private float dashForce = 500f;
    [SerializeField] private float dashResetTime = 0.3f;
    [SerializeField] private float dashUpScale = 0.2f;
    public bool isGrounded = false;
    private bool canDash = true;
    private bool canMove = true;
    private Vector3 target;
    private Vector3 movingAttackHeading;

    private InputAction move;
    private InputAction look;
    private InputAction sprint;
    private InputAction interact;
    private InputAction attack;
    private InputAction heavyAttack;
    private InputAction click;
    private InputAction jump;
    private InputAction scroll;
    private InputAction invButton;
    private InputAction mousePosition;

    private Vector2 movementInput;
    private float moveSpeed = 3f;
    public Transform cameraTransform;
    //public Transform aimCamTransform;

    private Vector3 camForward;
    private Vector3 camRight;

    [Header("IK Animations")]
    [SerializeField] private MultiAimConstraint headConstraint;
    [SerializeField] private MultiAimConstraint bodyConstraint;
    [SerializeField] private float lookSwitchHalfAngle = 90f;
    [SerializeField] float lookSmoothSpeed = 10f;
    [SerializeField] float weightLerpSpeed = 6f;
    float targetForwardWeight = 1f;
    float targetCameraWeight = 0f;
    private int cameraSourceIndex = 1;
    private RigBuilder rigBuilder;
    private Vector3 lookTargetVelocity;
    private Vector3 desiredLookPos;

    [Header("Boolet")]
    [SerializeField] private Transform boolet;
    [SerializeField] private Transform pinnacle;
    [SerializeField] private Transform slash;
    [SerializeField] private Transform slashHeavy;

    private Animator anim;

    public float abilityIndex = -1; //-1 = no ability selected
    private float lastShotTime = 0f;
    private float shootCooldown = 0.2f;

    private float lastWaitTime = 0f;
    private float waitCooldown = 30;

    [Header("Jumps")]
    [SerializeField] private int jumpCount = 2;
    [SerializeField] private int jumpsUsed = 0;
    [SerializeField] private float jumpForce = 250;
    [SerializeField] private Transform groundCheck;

    [Header("Interacting")]
    public bool Interact;
    public float scrolling = 0f;

    [Header("Weapon")]
    [SerializeField] private Transform weapon;

    [SerializeField] private float swingAngle = 30f;
    [SerializeField] private float swingDuration = 0.3f;
    [SerializeField] private Vector3 swingAxis = Vector3.up;
    [SerializeField] private Vector3 axisOffset = new Vector3(0, 90, 0);
    [SerializeField] private TrailRenderer weaponTrail;
    [SerializeField] private Collider weaponCollider;
    [SerializeField] private Transform lookAtPoint;
    [SerializeField] private LayerMask aimLayers;
    [SerializeField] private Quaternion idealHandRot;
    [SerializeField] private Transform movementTrails;
    [SerializeField] private CinemachineCamera cinemachineCam;
    //in future use weapon holding item def for these vals
    [SerializeField] private float fovIncrease = 10f;
    [SerializeField] private float fovDuration = 0.4f;
    [SerializeField, Range(-10f, 10f)] private float fovSkew = .5f; //negative = skew left (faster rise), positive = skew right (slower rise)
    [SerializeField] private float doubleTapWindow = 0.5f;
    [SerializeField] private float doubleTapActiveTime = 2.5f;

    private float duration;
    private float fovIncreaseBase;
    private float fovDurationBase;
    private float fovSkewBase;
    private float lastTapTime = -1f;
    private Vector2 lastDir;
    private bool wasPressedLastFrame = false;
    private float lastDirTapTime = -1f;

    public bool doubleTapForward, doubleTapBack, doubleTapLeft, doubleTapRight;
    private float forwardResetTimer, backResetTimer, leftResetTimer, rightResetTimer;

    private Coroutine fovRoutineLight;
    private Coroutine fovRoutineHeavy;
    private float defaultFOV = 90f;
    float maxLookDistance = 100f;
    float heightOffset = 1.0f;
    private Transform currentSlash;
    private Vector3 swingDir;
    private Quaternion baseRotation;

    private bool isSwinging = false;
    private bool isSwingingbool = false;
    private bool swingInProgress = false;
    private bool swingingMoving = false;

    private float baseSwingDuration;

    private bool once = true;
    private float swingTimer = 0f;
    [SerializeField] private float preslash = 0.15f;
    [SerializeField] private float swingExponent = 2.5f;
    private Quaternion swingStart;
    private Quaternion swingEnd;
    private Vector3 initialWeaponLocalPos;
    private Quaternion initialWeaponLocalRot;
    private float lastSwingEndTime;
    [SerializeField] private float resetDelay = 0.5f;
    [SerializeField] private float resetSpeed = 5f;

    [SerializeField] float scrollSensitivity = 10f;
    float cameraDistance;
    CinemachineComponentBase componentBase;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private HotbarSelector hotbar;
    public ShowHodling leftHolding;

    [Header("Rope Setup")]
    [SerializeField] private Transform[] ropeStarts;
    [SerializeField] private Transform[] ropeEnds;
    [SerializeField] private int segmentsPerRope = 10;
    [SerializeField] private float segmentScale = 0.1f;
    [SerializeField] private float springStrength = 100f;
    [SerializeField] private float springDamping = 5f;
    [SerializeField] private int splinePoints = 10;
    [SerializeField] private Transform RopeTops;
    [SerializeField] private float ropeTopsSpeed;

    [Header("Visuals")]
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private float ropeWidth = 0.05f;
    [SerializeField] private GameObject dashVFX;

    private List<LineRenderer> ropeLines = new List<LineRenderer>();
    private List<List<Transform>> ropeSegments = new List<List<Transform>>();
    private List<SpringJoint> startJoints = new List<SpringJoint>();
    private List<SpringJoint> endJoints = new List<SpringJoint>();
    private Vector3 ropeRestingLocalPos;
    private Vector3 lastMovementDirection = Vector3.forward;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    public override void OnNetworkSpawn()
    {
        fovDurationBase = fovDuration;
        fovIncreaseBase = fovIncrease;
        baseSwingDuration = swingDuration;
        fovSkewBase = fovSkew;
        ropeRestingLocalPos = RopeTops.localPosition;
        initialWeaponLocalPos = weapon.localPosition;
        initialWeaponLocalRot = weapon.localRotation;
        MainCanvas = FindFirstObjectByType<Canvas>();
        hotbar = MainCanvas.transform.Find("Hotbar").GetComponent<HotbarSelector>();
        hotbar.player = this;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Player added!");

        if (!IsOwner)
        {
            //GetComponent<PlayerInput>().enabled = false;
        }
        else
        {
            anim = transform.Find("lilCultist3").GetComponent<Animator>();
        }

        //move = InputSystem.actions.FindAction("Move");
        //look = InputSystem.actions.FindAction("Look");
        //sprint = InputSystem.actions.FindAction("Sprint");
        //interact = InputSystem.actions.FindAction("Interact");
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Player added!");

        if (!IsOwner)
        {
            //GetComponent<PlayerInput>().enabled = false;
        }
        else
        {
            anim = transform.Find("lilCultist3").GetComponent<Animator>();
        }

        move = InputSystem.actions.FindAction("Move");
        look = InputSystem.actions.FindAction("Look");
        sprint = InputSystem.actions.FindAction("Sprint");
        interact = InputSystem.actions.FindAction("Interact");
        attack = InputSystem.actions.FindAction("Attack"); //light attack
        heavyAttack = InputSystem.actions.FindAction("Heavy"); //heavy attack
        click = InputSystem.actions.FindAction("Click");
        jump = InputSystem.actions.FindAction("Jump");
        scroll = InputSystem.actions.FindAction("Scroll");
        invButton = InputSystem.actions.FindAction("InvButton");


        previousMousePosition = look.ReadValue<Vector2>();

        rb = GetComponent<Rigidbody>();
        inv = gameObject.GetComponent<Invintory>();

        controlPanel = Instantiate(ControlPanelPrefab, MainCanvas.transform).GetComponent<ControlPanel>();
        controlPanel.playerManager = this;

        //weaponCollider = weapon.transform.GetChild(2).GetComponent<CapsuleCollider>();
        //if (weaponCollider == null)
        //    Debug.Log(weapon.transform.GetChild(2).name);
        controlPanel.invintory = inv;
        controlPanel.gameObject.SetActive(false);

        Invoke("SpawningRaycast", 0.2f);


        leftHolding = GetComponentInChildren<ShowHodling>();

        //holding.holdingSlot = controlPanel.slotHolding;

        if (ropeStarts.Length != ropeEnds.Length)
        {
            Debug.LogError("ropeStarts and ropeEnds must have the same length.");
            return;
        }

        for (int i = 0; i < ropeStarts.Length; i++)
        {
            CreateRope(ropeStarts[i], ropeEnds[i]);
        }

        controlPanel.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        if (Camera.main != null)
        {
            //add camera transform as a second source for headConstraint and bodyConstrait
            if (headConstraint != null)
            {
                var sourceHead = headConstraint.data.sourceObjects;
                if (sourceHead.Count <= cameraSourceIndex)
                {
                    sourceHead.Add(new WeightedTransform(Camera.main.transform, 0f));
                }
                else
                {
                    //replace existing index 1 with cameraTransform (keep weight at 0)
                    sourceHead[cameraSourceIndex] = new WeightedTransform(Camera.main.transform, 0f);
                }
                headConstraint.data.sourceObjects = sourceHead;
            }

            if (bodyConstraint != null)
            {
                var sourceBody = bodyConstraint.data.sourceObjects;
                if (sourceBody.Count <= cameraSourceIndex)
                {
                    sourceBody.Add(new WeightedTransform(Camera.main.transform, 0f));
                }
                else
                {
                    sourceBody[cameraSourceIndex] = new WeightedTransform(Camera.main.transform, 0f);
                }
                bodyConstraint.data.sourceObjects = sourceBody;
            }
            rigBuilder = transform.Find("lilCultist3").GetComponent<RigBuilder>();
            if (rigBuilder == null)
                Debug.Log("NOOO RIG BUILDER :W");

            if (rigBuilder != null)
            {
                //rebuild the rig so the MultiAimConstraints pick up the new sources
                rigBuilder.Build();
            }
        }
    }

    public void GotCamera()
    {
        cinemachineCam = cameraTransform.GetComponent<CinemachineCamera>();
    }

    public override void OnNetworkDespawn()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    void Update()
    {
        if (!IsOwner) return;
        Walking();
        AimShooting();
        JumpCheck();
        DashCheck();
        CameraScroll();
        ResetWeaponPositionIfIdle();
        UpdateSwing();
        CheckCameraViews();
        SmoothUpdateConstraintWeights();

        if (rb.linearVelocity.magnitude >= 1f)
            movementTrails.gameObject.SetActive(true);
        else
            movementTrails.gameObject.SetActive(false);

        Interact = interact.WasPressedThisFrame();

        scrolling = scroll.ReadValue<float>();

        //check if looking around to deter idle behavior
        Vector2 currentMousePosition = look.ReadValue<Vector2>();

        // Check if the current position is different from the previous one
        if (currentMousePosition != previousMousePosition)
        {
            isMouseMoving = true;
        }
        else
        {
            isMouseMoving = false;
        }

        previousMousePosition = currentMousePosition;

        //for torso ik target
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxLookDistance, aimLayers))
        {
            desiredLookPos = hit.point;
        }
        else
        {
            desiredLookPos = cameraTransform.position + cameraTransform.forward * maxLookDistance;
        }

        lookAtPoint.position = Vector3.Lerp(lookAtPoint.position, desiredLookPos, Time.deltaTime * lookSmoothSpeed);

        //for rope physics and line renderer
        for (int i = 0; i < ropeLines.Count; i++)
        {
            var segments = ropeSegments[i];
            List<Vector3> splinePositions = new List<Vector3>();

            List<Vector3> controlPoints = new List<Vector3>();
            controlPoints.Add(ropeStarts[i].position);
            foreach (var seg in segments)
                controlPoints.Add(seg.position);
            controlPoints.Add(ropeEnds[i].position);

            for (int j = 0; j < controlPoints.Count - 1; j++)
            {
                Vector3 p0 = j == 0 ? controlPoints[j] : controlPoints[j - 1];
                Vector3 p1 = controlPoints[j];
                Vector3 p2 = controlPoints[j + 1];
                Vector3 p3 = (j + 2 < controlPoints.Count) ? controlPoints[j + 2] : controlPoints[j + 1];

                for (int k = 0; k < splinePoints; k++)
                {
                    float t = k / (float)splinePoints;
                    Vector3 point = 0.5f * (
                        (2f * p1) +
                        (-p0 + p2) * t +
                        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                        (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
                    );
                    splinePositions.Add(point);
                }
            }

            splinePositions.Add(controlPoints[^1]);

            ropeLines[i].positionCount = splinePositions.Count;
            ropeLines[i].SetPositions(splinePositions.ToArray());

            if (startJoints[i] != null)
                startJoints[i].connectedAnchor = ropeStarts[i].position;

            if (endJoints[i] != null)
                endJoints[i].connectedAnchor = ropeEnds[i].position;
        }

        if (invButton.WasPressedThisFrame())
        {
            if(controlPanel.gameObject.activeSelf)
            {
                controlPanel.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                controlPanel.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void JumpCheck()
    {
        int groundMask = LayerMask.GetMask("Ground");
        if (Physics.CheckSphere(groundCheck.position, 0.1f, groundMask) && rb.linearVelocity.y < 3f)
        {
            jumpsUsed = 0;
            isGrounded = true;
            anim.SetBool("IsGrounded", true);
        }
        else
        {
            isGrounded = false;
            anim.SetBool("IsGrounded", false);
        }

        if (jumpsUsed < jumpCount)
        {
            if (jump.WasPerformedThisFrame())
            {
                jumpsUsed++;
                rb.AddForce(transform.up * jumpForce);
                anim.SetBool("IsGrounded", false);
                anim.SetBool("IsJumping", true);
                anim.SetBool("IsIdleLong", false);
                lastWaitTime = Time.time;
            }
        }
        if (rb.linearVelocity.y < -1f)
        {
            anim.SetBool("IsJumping", false);
        }
    }

    private void Walking()
    {
        if (!canMove)
        {
            transform.forward = lastMovementDirection;
            return;
        }

        movementInput = move.ReadValue<Vector2>();

        //Debug.LogWarning("x: " + movementInput.x + ", y: " + movementInput.y);

        //for "dash attacks"
        float threshold = 0.7f;
        bool pressed = movementInput.sqrMagnitude > (threshold * threshold);
        if (pressed && !wasPressedLastFrame)
        {
            Vector2 currentDir = new Vector2(
                Mathf.RoundToInt(movementInput.x),
                Mathf.RoundToInt(movementInput.y)
            );

            if (currentDir != lastDir)
            {
                doubleTapForward = doubleTapBack = doubleTapLeft = doubleTapRight = false;
            }

            if (currentDir == lastDir && (Time.time - lastDirTapTime) <= doubleTapWindow)
            {
                if (currentDir.y > 0) { doubleTapForward = true; forwardResetTimer = Time.time; }
                if (currentDir.y < 0) { doubleTapBack = true; backResetTimer = Time.time; }
                if (currentDir.x > 0) { doubleTapRight = true; rightResetTimer = Time.time; }
                if (currentDir.x < 0) { doubleTapLeft = true; leftResetTimer = Time.time; }
            }

            lastDir = currentDir;
            lastDirTapTime = Time.time;
        }

        wasPressedLastFrame = pressed;

        if (doubleTapForward && Time.time - forwardResetTimer > doubleTapActiveTime) doubleTapForward = false;
        if (doubleTapBack && Time.time - backResetTimer > doubleTapActiveTime) doubleTapBack = false;
        if (doubleTapLeft && Time.time - leftResetTimer > doubleTapActiveTime) doubleTapLeft = false;
        if (doubleTapRight && Time.time - rightResetTimer > doubleTapActiveTime) doubleTapRight = false;


        Vector3 targetOffset = new Vector3(movementInput.x, 0, movementInput.y) * ropeTopsSpeed;
        Vector3 desiredLocalPos = ropeRestingLocalPos + targetOffset;
        RopeTops.localPosition = Vector3.Lerp(RopeTops.localPosition, desiredLocalPos, Time.deltaTime * 5f);

        target = (cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x);
        target.y = 0;
        target.Normalize();

        Vector3 horizontalVelocity = target * walkingSpeed;
        Vector3 newVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);

        if (movementInput.sqrMagnitude > 0.01f)
        {
            float castDistance = (horizontalVelocity.magnitude * Time.deltaTime) + 0.1f;
            Vector3 castOrigin = transform.position + Vector3.up * 1f;

            if (Physics.Raycast(castOrigin, horizontalVelocity.normalized, castDistance))
            {
                newVelocity.x = 0f;
                newVelocity.z = 0f;
            }
        }

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, newVelocity, Time.unscaledDeltaTime * 5);

        if (cameraTransform.gameObject.activeSelf)
        {
            if (Mathf.Abs(movementInput.x) >= .01f || Mathf.Abs(movementInput.y) >= .01f)
            {
                anim.SetBool("Walking", true);
                anim.SetBool("IsIdleLong", false);
                lastWaitTime = Time.time;
            }
            else
            {
                anim.SetBool("Walking", false);
            }
        }

        if (Time.time - lastWaitTime >= waitCooldown)
        {
            anim.SetBool("IsIdleLong", true);
            lastWaitTime = Time.time;
        }

        if (movementInput.magnitude > 0)
        {
            Vector3 hold = rb.linearVelocity;
            hold.y = 0;
            lastMovementDirection = hold;
            transform.forward = hold;
        }
        else
        {
            transform.forward = lastMovementDirection;
        }
    }

    private void CameraScroll()
    {
        if(componentBase == null)
        {
            componentBase = cameraTransform.GetComponent<CinemachineCamera>().GetCinemachineComponent(CinemachineCore.Stage.Body);
        }


        if(scrolling != 0)
        {
            cameraDistance = scrolling * scrollSensitivity;
            if(componentBase is CinemachineOrbitalFollow)
            {
                (componentBase as CinemachineOrbitalFollow).RadialAxis.Value -= cameraDistance;
            }
        }
    }

    private void DashCheck()
    {
        if(sprint.WasPressedThisFrame() && canDash && canMove)
        {
            canDash = false;

            anim.SetBool("IsIdleLong", false);
            lastWaitTime = Time.time;
            Invoke("CanDash", dashResetTime);

            Vector3 dashDirection = transform.forward + Vector3.up * dashUpScale;
            rb.AddForce(dashDirection.normalized * dashForce);
            //anim.SetBool("IsKnockedOver", true);
            //Invoke("KnockOverReset", 1);
        }
    }

    //temporary knockdown animation location - will be replaced with dash anim cancel, DO NOT DELETE
    private void KnockOverReset()
    {
        anim.SetBool("IsKnockedOver", false);
    }

    private void AimShooting()
    {
        //if (!holding.canHit)
        {
            //return; 
        }

        if (cameraTransform == null) return;

        if (attack.WasPressedThisFrame() && abilityIndex != -1 && Time.time - lastShotTime >= shootCooldown)
        {
            lastShotTime = Time.time;

            CameraShakeManager.instance.CameraShake(impulseSource, .1f); //recoil

            hotbar.TriggerAbility();
            RequestShootServerRpc(transform.position + Vector3.up, transform.forward);
        }
        else if (attack.WasPressedThisFrame() && Time.time - lastShotTime >= shootCooldown && !swingInProgress && once)
        {
            once = false;
            swingInProgress = true;
            lastShotTime = Time.time;
            anim.SetBool("IsIdleLong", false);
            StartLightSwing(spawnedSlash);
        }
        else if (heavyAttack.WasPressedThisFrame() && Time.time - lastShotTime >= shootCooldown && !swingInProgress && once)
        {
            once = false;
            swingInProgress = true;
            lastShotTime = Time.time;
            anim.SetBool("IsIdleLong", false);
            StartHeavySwing(spawnedSlash);
        }

    }

    private void StartLightSwing(Transform slashTransform)
    {
        if (weapon == null) return;

        if (IsOwner && cinemachineCam != null)
        {
            if (fovRoutineLight != null) StopCoroutine(fovRoutineLight);
            //in future get from weapon holding item def
            fovIncrease = fovIncreaseBase;
            fovDuration = fovDurationBase;
            fovRoutineLight = StartCoroutine(SlashFOVEffect());
        }
        if ((doubleTapForward || doubleTapRight || doubleTapLeft || doubleTapBack) && movementInput.magnitude > 0.0001f) //movementInput.magnitude < 0.0001f
            StartCoroutine(SwingRoutine(slashTransform, true, true));
        else // is not moving
        {
            StartCoroutine(SwingRoutine(slashTransform, true, false));
            swingingMoving = false;
        }
    }

    private void StartHeavySwing(Transform slashTransform)
    {
        //Debug.Log("weapon: " + (weapon == null) + ", isSwinging: " + isSwinging);
        if (weapon == null) return;

        //Debug.Log("IsOwner: " + IsOwner + ", cinemachineCam: " + (cinemachineCam != null));

        if (IsOwner && cinemachineCam != null)
        {
            if (fovRoutineHeavy != null) StopCoroutine(fovRoutineHeavy);
            //in future get from weapon holding item def
            fovIncrease = fovIncreaseBase * 2;
            fovDuration = fovDurationBase * 2;
            fovRoutineHeavy = StartCoroutine(SlashFOVEffect());
        }
        if (doubleTapForward || doubleTapRight || doubleTapLeft || doubleTapBack) //movementInput.magnitude < 0.0001f
            StartCoroutine(SwingRoutine(slashTransform, false, true));
        else
        {
            StartCoroutine(SwingRoutine(slashTransform, false, false));
            swingingMoving = false;
        }
    }


    private IEnumerator SwingRoutine(Transform slashTransform, bool isLightAttack, bool isMoving)
    {
        if (isSwinging) yield break;
        swingingMoving = true;
        movingAttackHeading = transform.forward;
        Vector3 movingSlashHeading = -transform.right;
        if(!isLightAttack || isMoving)
            canMove = false;

        if (isMoving)
        {
            swingingMoving = true;
        }

        Debug.Log(isLightAttack + " " + isMoving);
        anim.SetBool("Walking", false);
        isSwinging = true;
        once = true;
        swingInProgress = true;
        isSwingingbool = true;
        weaponTrail.enabled = false;
        weaponCollider.enabled = false;
        StartCoroutine(PausePlayerDelay(isLightAttack ? 1 : 1.3f));

        swingTimer = 0f;

        float actualPreslash = isLightAttack ? preslash : preslash * 2f; //replace preslash with weaponPreslashTimer from WeaponData.cs

        if (isMoving)
        {
            dashVFX.SetActive(true);
            actualPreslash *= 2f;
        }

        yield return new WaitForSeconds(actualPreslash);

        //Debug.Log("Slashing");

        if (isLightAttack)
            RequestSlashServerRpc(transform.position + Vector3.up, movingSlashHeading, true);
        else
            RequestSlashServerRpc(transform.position + Vector3.up, movingSlashHeading, false);

        currentSlash = slashTransform;

        float swingScale = isLightAttack ? 1f : 1.3f;
        duration = baseSwingDuration * swingScale;

        weaponCollider.enabled = true;

        baseRotation = currentSlash.rotation * Quaternion.Euler(axisOffset);

        if (!isLightAttack)
        {
            swingStart = baseRotation * Quaternion.AngleAxis(-90f, Vector3.right);
            swingEnd = baseRotation * Quaternion.AngleAxis(90f, Vector3.right);
        }
        else
        {
            swingStart = baseRotation * Quaternion.AngleAxis(-swingAngle * 0.5f, swingAxis);
            swingEnd = baseRotation * Quaternion.AngleAxis(swingAngle * 1.1f, swingAxis);
        }
    }


    private void UpdateSwing()
    {
        if (!isSwinging || weapon == null) return;

        swingTimer += Time.deltaTime;
        float t = Mathf.Clamp01(swingTimer / duration);
        t = Mathf.Pow(t, swingExponent);

        weapon.rotation = Quaternion.Slerp(swingStart, swingEnd, t);

        Vector3 dir = (weapon.rotation * Vector3.forward).normalized;
        weapon.position = transform.position + Vector3.up * heightOffset + dir * 0.2f;

        //Debug.Log("forward " + doubleTapForward + ", right: " + doubleTapRight + ", left: " + doubleTapLeft + ", back: " + doubleTapBack);
        //only move player from swing acceleration IF they were moving when swing started
        if (doubleTapForward || doubleTapRight || doubleTapLeft || doubleTapBack)
        {
            Debug.Log("Dash for attack!");
            Vector3 dashDirection = movingAttackHeading + Vector3.up * dashUpScale;
            rb.AddForce(dashDirection * dashForce * (isGrounded ? .1f : .08f), ForceMode.Acceleration);
        }

        if (t > 0.05f)
            weaponTrail.enabled = true;

        if (t >= 1f)
        {
            weaponTrail.Clear();
            weaponTrail.enabled = false;
            weaponCollider.enabled = false;
            isSwinging = false;
            isSwingingbool = false;
            swingInProgress = false;
            //swingingMoving = false;
            doubleTapForward = doubleTapBack = doubleTapRight = doubleTapLeft = false;

            lastSwingEndTime = Time.time;
        }
    }


    private void ResetWeaponPositionIfIdle()
    {
        if (weapon == null) return;

        if (!isSwinging && Time.time - lastSwingEndTime >= resetDelay)
        {
            weapon.localPosition = Vector3.Lerp(weapon.localPosition, initialWeaponLocalPos, Time.deltaTime * resetSpeed);
            weapon.localRotation = Quaternion.Slerp(weapon.localRotation, initialWeaponLocalRot, Time.deltaTime * resetSpeed);
        }
    }

    private void SpawningRaycast()
    {
        if (Physics.Raycast(new Ray(new Vector3(0, 500, 0), Vector3.down), out RaycastHit hitInfo, float.MaxValue, LayerMask.GetMask("Ground"))) //raycast down to ground @ (0, 500, 0) 
        {
            transform.position = hitInfo.point;
        }
    }

    //[Rpc(SendTo.Server)] //was trying to figure out how to send animations from client, but found out it was a bool override that was built in...
    //private void RequestWalkAnimServerRpc(bool state)
    //{
    //    Debug.Log("ASSIGNING WALKING TO: " + state);
    //    anim.SetBool("Walking", state);
    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestShootServerRpc(Vector3 spawnPosition, Vector3 shootDirection)
    {
        float forwardOffset = .5f; //distance in front of the player
        Vector3 spawnOffset = transform.forward * forwardOffset;

        Transform spawnedBoolet = Instantiate(boolet);
        Transform spawnedPinnacle = Instantiate(pinnacle);
        spawnedBoolet.transform.position = spawnPosition + spawnOffset;
        spawnedPinnacle.transform.position = weaponTrail.transform.position;
        spawnedPinnacle.transform.rotation = weaponTrail.transform.rotation;

        var netObj = spawnedBoolet.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        var netObj2 = spawnedPinnacle.GetComponent<NetworkObject>();
        netObj2.Spawn(true);
        spawnedPinnacle.transform.parent = weaponTrail.transform.parent;

        var rb = spawnedBoolet.GetComponent<Rigidbody>();
        rb.AddForce(shootDirection.normalized * 5000f);

        Destroy(spawnedBoolet.gameObject, 3);
        Destroy(spawnedPinnacle.gameObject, 2);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestSlashServerRpc(Vector3 spawnPosition, Vector3 shootDirection, bool isLight)
    {
        weaponTrail.Clear();
        weaponTrail.enabled = false;
        Transform p = transform.parent;

        if(!leftHolding.getShowsHolding())
        {
            if (isLight && !swingingMoving)
                spawnedSlash = Instantiate(slash); //use slashLightVFX from WeaponData.cs
            else if (isLight && swingingMoving)
                spawnedSlash = Instantiate(slash); //use slashDashLightVFX from WeaponData.cs
            else if (!isLight && !swingingMoving)
                spawnedSlash = Instantiate(slashHeavy); //use slashHeavyVFX from WeaponData.cs
            else
                spawnedSlash = Instantiate(slashHeavy); //use slashDashHeavyVFX from WeaponData.cs

            float forwardOffset = .5f;
            Vector3 spawnOffset = transform.forward * forwardOffset;

            spawnedSlash.transform.position = spawnPosition + spawnOffset; //use weaponReach from WeaponData.cs
            spawnedSlash.transform.rotation = Quaternion.LookRotation(shootDirection);
            spawnedSlash.transform.Rotate(Random.Range(-90f, 90f), 0f, 0f);

            UseEntity.Damage slashHurt = spawnedSlash.gameObject.GetComponent<UseEntity.Damage>();
            slashHurt.setDamage(isLight ? 1 : 3);
            Debug.Log("Damage is " + slashHurt.getDamage());
        }
        else
        {
            GameObject holdingObject = leftHolding.getHoldingObject();

            if(holdingObject.TryGetComponent<WeaponData>(out WeaponData data))
            {
                if (isLight && !swingingMoving)
                    spawnedSlash = Instantiate(data.GetLightVFX().transform); //use slashLightVFX from WeaponData.cs
                else if (isLight && swingingMoving)
                    spawnedSlash = Instantiate(data.GetDashingLightVFX().transform); //use slashDashLightVFX from WeaponData.cs
                else if (!isLight && !swingingMoving)
                    spawnedSlash = Instantiate(data.GetHeavyVFX().transform); //use slashHeavyVFX from WeaponData.cs
                else
                    spawnedSlash = Instantiate(data.GetDashingHeavyVFX().transform); //use slashDashHeavyVFX from WeaponData.cs

                Vector3 spawnOffset = transform.forward * data.GetReach();
                UseEntity.Damage slashHurt = spawnedSlash.gameObject.GetComponent<UseEntity.Damage>();

                spawnedSlash.transform.position = spawnPosition + spawnOffset; //use weaponReach from WeaponData.cs
                spawnedSlash.transform.rotation = Quaternion.LookRotation(shootDirection);
                spawnedSlash.transform.Rotate(Random.Range(-90f, 90f), 0f, 0f);
                slashHurt.setDamage(isLight ? data.GetLightDamage() : data.GetHeavyDamage());
            }
            Debug.Log("Damage is " + slashHurt.getDamage());
        }

        var netObj = spawnedSlash.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        spawnedSlash.parent = transform;

        StartCoroutine(UnparentAfterDelay(transform, p, 0.3f));
        Destroy(spawnedSlash.gameObject, .5f);

        //weaponTrail.enabled = true;

        if (swingingMoving && isLight)
           StartLightSwing(spawnedSlash);
        else if (swingingMoving && !isLight)
            StartHeavySwing(spawnedSlash);
        swingingMoving = false;
    }

    private IEnumerator PausePlayerDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        dashVFX.SetActive(false);

        //rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        swingingMoving = false;
        canMove = true;
    }

    private IEnumerator UnparentAfterDelay(Transform t, Transform p, float delay)
    {
        yield return new WaitForSeconds(delay);
        t.parent = null;
    }

    private void CreateRope(Transform start, Transform end)
    {
        List<Transform> segments = new List<Transform>();

        GameObject ropeContainer = new GameObject($"Rope_{ropeSegments.Count}");
        //ropeContainer.transform.SetParent(transform);

        Vector3 dir = (end.position - start.position).normalized;
        float totalDistance = Vector3.Distance(start.position, end.position);
        float spacing = totalDistance / (segmentsPerRope + 1);

        Transform prev = null;

        for (int i = 0; i < segmentsPerRope; i++)
        {
            Vector3 pos = start.position + dir * spacing * (i + 1);
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.transform.SetParent(ropeContainer.transform);
            seg.transform.position = pos;
            seg.transform.localScale = Vector3.one * segmentScale;
            seg.GetComponent<Renderer>().enabled = false; //hide the cube

            Rigidbody rb = seg.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.linearDamping = .4f;
            rb.angularDamping = .1f;
            rb.useGravity = false;

            if (prev != null)
            {
                SpringJoint sj = seg.AddComponent<SpringJoint>();
                sj.connectedBody = prev.GetComponent<Rigidbody>();
                sj.spring = springStrength;
                sj.damper = springDamping;
                sj.maxDistance = segmentScale;
                sj.minDistance = 0f;
                sj.autoConfigureConnectedAnchor = true;
            }

            segments.Add(seg.transform);
            prev = seg.transform;
        }

        //connect first segment to start
        SpringJoint firstJoint = segments[0].gameObject.AddComponent<SpringJoint>();
        firstJoint.connectedBody = null;
        firstJoint.connectedAnchor = start.position;
        firstJoint.spring = springStrength;
        firstJoint.damper = springDamping;
        firstJoint.maxDistance = 1f;
        firstJoint.minDistance = 0f;
        firstJoint.autoConfigureConnectedAnchor = false;
        startJoints.Add(firstJoint);

        //connect last segment to end
        SpringJoint lastJoint = segments[^1].gameObject.AddComponent<SpringJoint>();
        lastJoint.connectedBody = null;
        lastJoint.connectedAnchor = end.position;
        lastJoint.spring = springStrength;
        lastJoint.damper = springDamping;
        lastJoint.maxDistance = 1f;
        lastJoint.minDistance = 0f;
        lastJoint.autoConfigureConnectedAnchor = false;
        endJoints.Add(lastJoint);

        ropeSegments.Add(segments);

        //create line renderer
        GameObject lrObj = new GameObject($"RopeLine_{ropeLines.Count}");
        lrObj.transform.SetParent(ropeContainer.transform);
        LineRenderer lr = lrObj.AddComponent<LineRenderer>();
        lr.material = ropeMaterial;
        lr.startWidth = ropeWidth;
        lr.endWidth = ropeWidth;
        ropeLines.Add(lr);
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void ClientAndHostRpc(int value, ulong sourceNetworkObjectId)
    //{
    //    Debug.Log($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
    //    if (IsOwner) //Only send an RPC to the owner of the NetworkObject
    //    {
    //        ServerOnlyRpc(value + 1, sourceNetworkObjectId);
    //    }
    //}

    private IEnumerator SlashFOVEffect()
    {
        float elapsed = 0f;
        float halfTime = fovDuration * 0.5f;

        cinemachineCam.Lens.FieldOfView = defaultFOV;

        while (elapsed < fovDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fovDuration);

            //apply skew (parabola skewed to left/right)
            float skewData = 1f + fovSkew * 2f;

            if(leftHolding.getShowsHolding())
            {
                GameObject holdingObject = leftHolding.getHoldingObject();
                if (holdingObject.TryGetComponent<WeaponData>(out WeaponData data))
                {
                    float weight = Mathf.Clamp(data.GetWeight(), -10, 10);
                    skewData = weight;
                }
            }

                float skew = Mathf.Pow(t, skewData); // -1 = fast rise, +1 = slow rise //replace with a clamped weaponWeight (from WeaponData.cs) to be from -10 to 10

            float parabola = 4f * skew * (1f - skew);
            float fovValue = defaultFOV + parabola * fovIncrease;

            cinemachineCam.Lens.FieldOfView = fovValue;
            yield return null;
        }

        cinemachineCam.Lens.FieldOfView = defaultFOV;
    }


    private void CheckCameraViews()
    {
        if (cameraTransform == null || (headConstraint == null && bodyConstraint == null))
            return;

        Vector3 camForwardXZ = cameraTransform.forward;
        camForwardXZ.y = 0f;
        if (camForwardXZ.sqrMagnitude < 0.001f)
            camForwardXZ = cameraTransform.position - transform.position;
        camForwardXZ.Normalize();

        Vector3 playerForwardXZ = transform.forward;
        playerForwardXZ.y = 0f;
        playerForwardXZ.Normalize();

        float angle = Vector3.SignedAngle(playerForwardXZ, camForwardXZ, Vector3.up);

        bool inRange = Mathf.Abs(angle) <= lookSwitchHalfAngle;

        // Instead of snapping weights here, simply set new targets
        if (inRange)
        {
            targetForwardWeight = 1f;
            targetCameraWeight = 0f;
        }
        else
        {
            targetForwardWeight = 0f;
            targetCameraWeight = 1f;
        }
    }

    private void SmoothUpdateConstraintWeights()
    {
        UpdateConstraintWeightSmooth(headConstraint);
        UpdateConstraintWeightSmooth(bodyConstraint);
    }

    private void UpdateConstraintWeightSmooth(MultiAimConstraint constraint)
    {
        if (constraint == null) return;

        var sources = constraint.data.sourceObjects;

        var srcForward = sources[0];
        var srcCamera = sources[cameraSourceIndex];

        srcForward.weight = Mathf.Lerp(srcForward.weight, targetForwardWeight, Time.deltaTime * weightLerpSpeed);
        srcCamera.weight = Mathf.Lerp(srcCamera.weight, targetCameraWeight, Time.deltaTime * weightLerpSpeed);

        sources[0] = srcForward;
        sources[cameraSourceIndex] = srcCamera;
        constraint.data.sourceObjects = sources;
    }

    private void CanDash() { canDash = true; }

    public Invintory GetInventory() => inv;
}
