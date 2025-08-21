using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.Animations;
using NUnit;

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
    public Transform aimCamTransform;

    private Vector3 camForward;
    private Vector3 camRight;

    [Header("Boolet")]
    [SerializeField] private Transform boolet;
    [SerializeField] private Transform slash;

    private Animator anim;

    private bool canShoot = true;
    private float lastShotTime = 0f;
    private float shootCooldown = 0.2f;

    private float lastWaitTime = 0f;
    private float waitCooldown = 30;

    [Header("Jumps")]
    [SerializeField] private int jumpCount = 1;
    [SerializeField] private int jumpsUsed = 0;
    [SerializeField] private float jumpForce = 250;
    [SerializeField] private Transform groundCheck;

    [Header("Interacting")]
    public bool Interact;
    public float scrolling = 0f;

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

        controlPanel.invintory = inv;
        inv.ui = controlPanel;
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

        Interact = interact.WasPressedThisFrame();

        scrolling = scroll.ReadValue<float>();

        if(invButton.WasPressedThisFrame())
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

        target = (cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x);
        target.y = 0;
        target.Normalize();

        Vector3 pass = target * walkingSpeed + new Vector3(0, rb.linearVelocity.y, 0);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, pass, Time.fixedDeltaTime * 5);


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
        if (cameraTransform == null || aimCamTransform == null) return;

        if (attack.IsPressed())
        {
            lastWaitTime = Time.time;
            anim.SetBool("IsIdleLong", false);
            aimCamTransform.gameObject.SetActive(true);
            cameraTransform.gameObject.SetActive(false);
            canShoot = true;
        }
        else
        {
            aimCamTransform.gameObject.SetActive(false);
            cameraTransform.gameObject.SetActive(true);
            canShoot = false;
        }

        if (click.WasPressedThisFrame() && canShoot && Time.time - lastShotTime >= shootCooldown)
        {
            lastShotTime = Time.time;

            RequestShootServerRpc(transform.position + Vector3.up, transform.forward);
        }
        else if (click.WasPressedThisFrame() && Time.time - lastShotTime >= shootCooldown)
        {
            lastShotTime = Time.time;
            RequestSlashServerRpc(transform.position + Vector3.up, -transform.right);
        }
    }

    //[Rpc(SendTo.Server)] //was trying to figure out how to send animations from client, but found out it was a bool override that was built in...
    //private void RequestWalkAnimServerRpc(bool state)
    //{
    //    Debug.Log("ASSIGNING WALKING TO: " + state);
    //    anim.SetBool("Walking", state);
    //}

    [Rpc(SendTo.Server)]
    private void RequestShootServerRpc(Vector3 spawnPosition, Vector3 shootDirection)
    {
        float forwardOffset = 1f; //distance in front of the player
        Vector3 spawnOffset = transform.forward * forwardOffset;

        Transform spawnedBoolet = Instantiate(boolet);
        spawnedBoolet.transform.position = spawnPosition + spawnOffset;

        var netObj = spawnedBoolet.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        
        var rb = spawnedBoolet.GetComponent<Rigidbody>();
        rb.AddForce(shootDirection.normalized * 5000f); //you can tweak the force
        
        Destroy(spawnedBoolet.gameObject, 3);
    }

    [Rpc(SendTo.Server)]
    private void RequestSlashServerRpc(Vector3 spawnPosition, Vector3 shootDirection)
    {
        float forwardOffset = .5f; //distance in front of the player
        Vector3 spawnOffset = transform.forward * forwardOffset;

        Transform spawnedSlash = Instantiate(slash);
        spawnedSlash.transform.position = spawnPosition + spawnOffset;
        spawnedSlash.transform.rotation = Quaternion.LookRotation(shootDirection);
        spawnedSlash.transform.Rotate(Random.Range(-90f, 90f), 0f, 0f);

        var netObj = spawnedSlash.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        Destroy(spawnedSlash.gameObject, .5f);
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
}
