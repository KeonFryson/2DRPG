using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float defaultMoveSpeed = 5f;
    [SerializeField] private float sprintSpeedBoost = 2.5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private InputSystem_Actions inputActions;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    bool isSprinting = false;
    public bool isHoldingItem = false;

    // Public read-only property used by other classes to query which side the player is facing
    public bool FacingLeft => spriteRenderer != null && spriteRenderer.flipX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        inputActions = new InputSystem_Actions();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        PlayerInput();
        AdjustPlayerFacingDirection();
    }
    private void PlayerInput()
    {
        movement = inputActions.Player.Move.ReadValue<Vector2>();
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);
    }

    private void AdjustPlayerFacingDirection()
    {
        // Use the new Input System to get pointer/mouse position to avoid InvalidOperationException
        if (Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);

        if (mousePos.x < playerScreenPoint.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void Move()
    {
        Vector2 newPosition = rb.position + movement * (defaultMoveSpeed * Time.fixedDeltaTime);

        if (isSprinting)
        {
            newPosition = rb.position + movement * ((defaultMoveSpeed * sprintSpeedBoost) * Time.fixedDeltaTime);
        }

        rb.MovePosition(newPosition);
    }
    private void FixedUpdate()
    {
        Move();
    }

}