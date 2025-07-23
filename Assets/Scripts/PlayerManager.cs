using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private Transform boolet;
    private Vector2 movementInput;
    private float moveSpeed = 3f;

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
        Debug.Log("Player added!");

        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = false;
            InputActions.FindActionMap("Player").Disable();
        }
        else
        {
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
    }

    [Rpc(SendTo.Server)]
    private void RequestShootServerRpc(Vector3 spawnPosition, Vector3 shootDirection)
    {
        Transform spawnedBoolet = Instantiate(boolet);
        spawnedBoolet.transform.position = spawnPosition;

        var netObj = spawnedBoolet.GetComponent<NetworkObject>();
        netObj.Spawn(true); // Server authority

        var rb = spawnedBoolet.GetComponent<Rigidbody>();
        rb.AddForce(shootDirection.normalized * 5000f); // You can tweak the force

        Destroy(spawnedBoolet.gameObject, 3);
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
