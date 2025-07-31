using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private Transform boolet;
    [SerializeField] private Transform slash;
    private Vector2 movementInput;
    private float moveSpeed = 3f;

    private Animator anim;
    public InputActionAsset InputActions;
    public Transform cameraTransform;
    public Transform aimCamTransform;

    private Vector3 camForward;
    private Vector3 camRight;

    private bool canShoot = false;
    private float lastShotTime = 0f;
    private float shootCooldown = 0.2f;

    public override void OnNetworkSpawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Player added!");

        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = false;
            InputActions.FindActionMap("Player").Disable();
        }
        else
        {
            anim = transform.Find("lilCultist3").GetComponent<Animator>();
            InputActions.FindActionMap("Player").Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        if (!IsOwner) return;
        Walking();
        AimShooting();
    }

    private void Walking()
    {
        if (cameraTransform == null) return;

        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        float speedMult;

        if (cameraTransform.gameObject.activeSelf)
        {
            camForward = cameraTransform.forward;
            camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            speedMult = 1;

            if(Mathf.Abs(xInput) >= .01f || Mathf.Abs(zInput) >= .01f)
            {
                //if(!IsHost)
                //    RequestWalkAnimServerRpc(true);
                anim.SetBool("Walking", true);
            }
            else
            {
                //if(!IsHost)
                //    RequestWalkAnimServerRpc(false);
                anim.SetBool("Walking", false);
            }
        }
        else
        {
            camForward = aimCamTransform.forward;
            camRight = aimCamTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            speedMult = .2f;
        }

        Vector3 moveDir = camForward * zInput + camRight * xInput;
        transform.position += moveDir * moveSpeed * speedMult * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f * speedMult);
        }

        Vector3 moveDirection = camForward * movementInput.y + camRight * movementInput.x;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void AimShooting()
    {
        if (cameraTransform == null || aimCamTransform == null) return;

        if (Input.GetMouseButton(1))
        {
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

        if (Input.GetMouseButton(0) && canShoot && Time.time - lastShotTime >= shootCooldown)
        {
            lastShotTime = Time.time;

            RequestShootServerRpc(transform.position + Vector3.up, transform.forward);
        }
        else if(Input.GetMouseButton(0) && Time.time - lastShotTime >= shootCooldown)
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

}
