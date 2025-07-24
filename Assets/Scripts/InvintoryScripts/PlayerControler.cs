using Mono.Cecil.Cil;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody))]
public class PlayerControler : MonoBehaviour
{
    public InputActionAsset InputActions;
    public bool Interacted = false;

    [Header("Movement")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] public Rigidbody MainCam;
    [SerializeField] private float lookSpeed;
    private Vector3 target;
    private float speed;

    [Header("Crafting")]
    public CraftingPanel CraftingPanel;
    public InvintoryPanel InvintoryPanel;

    private Rigidbody rb;

    private InputAction move;
    private InputAction look;
    private InputAction sprint;
    private InputAction interact;

    private Vector2 moveVec;
    private Vector2 lookVec;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        move = InputSystem.actions.FindAction("Move");
        look = InputSystem.actions.FindAction("Look");
        sprint = InputSystem.actions.FindAction("Sprint");
        interact = InputSystem.actions.FindAction("Interact");

        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        MainCam.transform.eulerAngles = Vector3.zero;
    }

    private void Update()
    {          
        moveVec = move.ReadValue<Vector2>();
        lookVec = look.ReadValue<Vector2>();
        if (sprint.IsPressed())
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = walkingSpeed;
        }
        Interacted = interact.WasPressedThisFrame();

        if(Interacted)
        {
            if(InvintoryPanel.PanelShowing)
            {
                InvintoryPanel.HidePanel();
            }
            else
            {
                InvintoryPanel.ShowPanel();
            }
        }

        target = MainCam.transform.rotation * new Vector3(moveVec.normalized.x, 0, moveVec.normalized.y) * speed * Time.deltaTime;
        target.y = 0;
        rb.MovePosition(transform.position + target);

        float rotationAmmountY = lookVec.x * lookSpeed * Time.deltaTime;
        float rotationAmmountX = lookVec.y * -lookSpeed * Time.deltaTime;

        MainCam.transform.eulerAngles += lookSpeed * new Vector3(rotationAmmountX, rotationAmmountY, 0);

    }
}
