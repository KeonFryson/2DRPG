using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private InputSystem_Actions inputActions;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        spriteRenderer.flipX = mousePos.x < playerScreenPoint.x;
    }

    private void Move()
    {
        Vector2 newPosition = rb.position + movement * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }
    private void FixedUpdate()
    {
        Move();
    }

}