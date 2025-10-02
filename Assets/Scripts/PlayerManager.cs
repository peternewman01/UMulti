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
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PlayerManager : NetworkBehaviour
{
    public InputActionAsset InputActions;
    public Canvas MainCanvas;
    public GameObject ControlPanelPrefab;
    [SerializeField] private ControlPanel controlPanel;
    [SerializeField] private Invintory inv;


    [Header("Movement")]
    [SerializeField] private Rigidbody rb;

    [SerializeField] private float walkingSpeed = 10f;
    [SerializeField] private float dashForce = 500f;
    [SerializeField] private float dashResetTime = 0.3f;
    [SerializeField] private float dashUpScale = 0.2f;
    private bool canDash = true;
    private Vector3 target;

    private InputAction move;
    private InputAction look;
    private InputAction sprint;
    private InputAction interact;
    private InputAction attack;
    private InputAction click;
    private InputAction jump;
    private InputAction scroll;
    private InputAction invButton;

    private Vector2 movementInput;
    private float moveSpeed = 3f;
    public Transform cameraTransform;
    //public Transform aimCamTransform;

    private Vector3 camForward;
    private Vector3 camRight;

    [Header("Boolet")]
    [SerializeField] private Transform boolet;
    [SerializeField] private Transform pinnacle;
    [SerializeField] private Transform slash;

    private Animator anim;

    private bool canShoot = true;
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
    float maxLookDistance = 100f;
    float heightOffset = 1.0f;
    private Transform currentSlash;
    private Vector3 swingDir;
    private Quaternion baseRotation;

    private bool isSwinging = false;
    private bool isSwingingbool = false;
    private float swingTimer = 0f;
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
    [SerializeField] private CinemachineImpulseSource impulseSource; //recoil impulse

    public StickHolding holding;

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

    private List<LineRenderer> ropeLines = new List<LineRenderer>();
    private List<List<Transform>> ropeSegments = new List<List<Transform>>();
    private List<SpringJoint> startJoints = new List<SpringJoint>();
    private List<SpringJoint> endJoints = new List<SpringJoint>();
    private Vector3 ropeRestingLocalPos;

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
        ropeRestingLocalPos = RopeTops.localPosition;
        initialWeaponLocalPos = weapon.localPosition;
        initialWeaponLocalRot = weapon.localRotation;
        MainCanvas = FindFirstObjectByType<Canvas>();
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
        attack = InputSystem.actions.FindAction("Attack");
        click = InputSystem.actions.FindAction("Click");
        jump = InputSystem.actions.FindAction("Jump");
        scroll = InputSystem.actions.FindAction("Scroll");
        invButton = InputSystem.actions.FindAction("InvButton");

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


        holding = GetComponent<StickHolding>();
        holding.holdingSlot = controlPanel.slotHolding;

        if (ropeStarts.Length != ropeEnds.Length)
        {
            Debug.LogError("ropeStarts and ropeEnds must have the same length.");
            return;
        }

        for (int i = 0; i < ropeStarts.Length; i++)
        {
            CreateRope(ropeStarts[i], ropeEnds[i]);
        }

        controlPanel.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if(rb.linearVelocity.magnitude >= 1f)
            movementTrails.gameObject.SetActive(true);
        else
            movementTrails.gameObject.SetActive(false);

        Interact = interact.WasPressedThisFrame();

        scrolling = scroll.ReadValue<float>();

        //for torso ik target
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxLookDistance, aimLayers))
        {
            lookAtPoint.position = hit.point;
        }
        else
        {
            lookAtPoint.position = cameraTransform.position + cameraTransform.forward * maxLookDistance;
        }

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
            anim.SetBool("IsGrounded", true);
        }
        else
            anim.SetBool("IsGrounded", false);
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
        movementInput = move.ReadValue<Vector2>();

        Vector3 targetOffset = new Vector3(movementInput.x, 0, movementInput.y) * ropeTopsSpeed;
        Vector3 desiredLocalPos = ropeRestingLocalPos + targetOffset;
        RopeTops.localPosition = Vector3.Lerp(RopeTops.localPosition, desiredLocalPos, Time.deltaTime * 5f);

        target = (cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x);
        target.y = 0;
        target.Normalize();

        Vector3 pass = target * walkingSpeed + new Vector3(0, rb.linearVelocity.y, 0);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, pass, Time.unscaledDeltaTime * 5);


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
            transform.forward = hold;
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
            if(componentBase is CinemachinePositionComposer)
            {
                (componentBase as CinemachinePositionComposer).CameraDistance -= cameraDistance;
            }
        }
    }

    private void DashCheck()
    {
        if(sprint.WasPressedThisFrame() && canDash)
        {
            canDash = false;

            anim.SetBool("IsIdleLong", false);
            lastWaitTime = Time.time;
            Invoke("CanDash", dashResetTime);

            Vector3 dashDirection = transform.forward + Vector3.up * dashUpScale;
            rb.AddForce(dashDirection.normalized * dashForce);
            anim.SetBool("IsKnockedOver", true);
            Invoke("KnockOverReset", 1);
        }
    }

    //temporary knockdown animation location
    private void KnockOverReset()
    {
        anim.SetBool("IsKnockedOver", false);
    }

    private void AimShooting()
    {
        if (!holding.canHit)
        {
            //return; 
        }


        if (cameraTransform == null) return;

        if (click.IsPressed()) //aim right click
        {
            lastWaitTime = Time.time;
            anim.SetBool("IsIdleLong", false);

            canShoot = true;
        }
        else
        {
            canShoot = false;
        }

        if (attack.WasPressedThisFrame() && canShoot && Time.time - lastShotTime >= shootCooldown)
        {
            lastShotTime = Time.time;

            CameraShakeManager.instance.CameraShake(impulseSource, .1f); //recoil
            RequestShootServerRpc(transform.position + Vector3.up, transform.forward);
        }
        else if (attack.WasPressedThisFrame() && Time.time - lastShotTime >= shootCooldown)
        {
            lastShotTime = Time.time;

            anim.SetBool("IsIdleLong", false);
            RequestSlashServerRpc(transform.position + Vector3.up, -transform.right);
        }
    }

    private void StartSwing(Transform slashTransform)
    {
        if (weapon == null) return;

        isSwingingbool = false;
        isSwinging = true;
        weaponTrail.enabled = false;
        weaponCollider.enabled = true;
        swingTimer = 0f;

        currentSlash = slashTransform;

        // Use the slash�s orientation instead of the player�s
        baseRotation = currentSlash.rotation * Quaternion.Euler(axisOffset);

        // define start and end around the slash�s up axis
        swingStart = baseRotation * Quaternion.AngleAxis(-swingAngle * .5f, swingAxis);
        swingEnd = baseRotation * Quaternion.AngleAxis(swingAngle * 1.1f, swingAxis);
    }

    private void UpdateSwing()
    {
        if (!isSwinging || weapon == null || currentSlash == null) return;

        swingTimer += Time.deltaTime;
        float t = swingTimer / swingDuration;

        weapon.rotation = Quaternion.Slerp(swingStart, swingEnd, t);

        float distance = .2f;
        Vector3 dir = (weapon.rotation * Vector3.forward).normalized;
        weapon.position = transform.position + Vector3.up * heightOffset + dir * distance;
        weaponTrail.enabled = true;

        if (t >= .5f)
        {
            weaponTrail.Clear();
            weaponTrail.enabled = false;
            weaponCollider.enabled = false;
            isSwinging = false;
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
    private void RequestSlashServerRpc(Vector3 spawnPosition, Vector3 shootDirection)
    {
        weaponTrail.Clear();
        weaponTrail.enabled = false;
        print("weapon trail disabled");
        Transform p = transform.parent;

        float forwardOffset = .5f;
        Vector3 spawnOffset = transform.forward * forwardOffset;

        Transform spawnedSlash = Instantiate(slash);
        spawnedSlash.transform.position = spawnPosition + spawnOffset;
        spawnedSlash.transform.rotation = Quaternion.LookRotation(shootDirection);
        spawnedSlash.transform.Rotate(Random.Range(-90f, 90f), 0f, 0f);

        if(!holding.canHit)
        {
            hurtEntities slashHurt = spawnedSlash.gameObject.GetComponent<hurtEntities>();
            slashHurt.setDamage(1);
        }

        var netObj = spawnedSlash.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        spawnedSlash.parent = transform;

        StartCoroutine(UnparentAfterDelay(transform, p, 0.3f));
        Destroy(spawnedSlash.gameObject, .5f);

        //weaponTrail.enabled = true;
        print("weapontrail started");

        StartSwing(spawnedSlash);
    }

    private IEnumerator UnparentAfterDelay(Transform t, Transform p, float delay)
    {
        yield return new WaitForSeconds(delay);
        t.parent = null;
    }
    private void CreateRope(Transform start, Transform end)
    {
        List<Transform> segments = new List<Transform>();

        // Rope container to keep hierarchy clean
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
            seg.GetComponent<Renderer>().enabled = false; // Hide the cube

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

        // Connect first segment to start
        SpringJoint firstJoint = segments[0].gameObject.AddComponent<SpringJoint>();
        firstJoint.connectedBody = null;
        firstJoint.connectedAnchor = start.position;
        firstJoint.spring = springStrength;
        firstJoint.damper = springDamping;
        firstJoint.maxDistance = 1f;
        firstJoint.minDistance = 0f;
        firstJoint.autoConfigureConnectedAnchor = false;
        startJoints.Add(firstJoint);

        // Connect last segment to end
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

        // Create line renderer
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

    private void CanDash() { canDash = true; }

    public Invintory GetInventory() => inv;
}
