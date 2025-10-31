using UnityEngine;
using UnityEngine.InputSystem;

public class Sword : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private ActiveWeapon activeWeapon;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        inputActions = new InputSystem_Actions();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        activeWeapon = GetComponent<ActiveWeapon>()
                      ?? GetComponentInChildren<ActiveWeapon>()
                      ?? GetComponentInParent<ActiveWeapon>();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += _ => Attack();
    }

    private void OnDisable()
    {
        inputActions.Player.Attack.performed -= _ => Attack();
        inputActions.Disable();
    }

    private void Update()
    {
        UpdateSideBasedOnMouse();
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
    }

    // Only switch the sword side (flip) based on whether the mouse is left/right of the player.
    private void UpdateSideBasedOnMouse()
    {
        if (Mouse.current == null || Camera.main == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 referenceWorldPos = (playerController != null) ? playerController.transform.position : transform.position;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(referenceWorldPos);

        Transform target = (activeWeapon != null) ? activeWeapon.transform : transform;

        // If mouse is left of player -> flip horizontally. Otherwise ensure default orientation.
        if (mousePos.x < playerScreenPoint.x)
        {
            // flip horizontally (preserve rotation on Z if any by using localEulerAngles for X/Y)
            target.localEulerAngles = new Vector3(0f, 180f, target.localEulerAngles.z);
        }
        else
        {
            target.localEulerAngles = new Vector3(0f, 0f, target.localEulerAngles.z);
        }
    }
}