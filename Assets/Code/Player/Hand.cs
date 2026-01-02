using UnityEngine;
using UnityEngine.InputSystem;

public class Hand : MonoBehaviour
{
   

    private InputSystem_Actions inputActions;
    private Animator animator;
    public SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    
    private PlayerController player;
    private Transform target;



    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
         
        inputActions = new InputSystem_Actions();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
         
        target = this.transform;
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

        

        if (playerController.isHoldingItem)
        {
            UpdateSideBasedOnMouse(0.55f);
            
        }
        else
        {
            UpdateSideBasedOnMouse(0.267f);
            
        }
       

    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
        animator.SetBool("isHoldingSword", playerController.isHoldingItem);

    }

 
     
    private void UpdateSideBasedOnMouse(float offsetX)
    {
        if (Mouse.current == null || Camera.main == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 referenceWorldPos = (playerController != null) ? playerController.transform.position : transform.position;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(referenceWorldPos);

        //Transform target = (activeWeapon != null) ? activeWeapon.transform : transform;

        // If mouse is left of player -> flip horizontally. Otherwise ensure default orientation.
         

        if (mousePos.x < playerScreenPoint.x)
        {
            // Flip horizontally (preserve Z rotation)
            target.localEulerAngles = new Vector3(180, -180, 180f);

            // Move slightly closer on the X axis
            target.localPosition = new Vector3(
                -Mathf.Abs(offsetX),
                target.localPosition.y,
                target.localPosition.z
            );
        }
        else
        {
            // Flip horizontally (preserve Z rotation)
            target.localEulerAngles = new Vector3(0, 180f, 0);

            // Move slightly closer on the X axis
            target.localPosition = new Vector3(
                Mathf.Abs(offsetX),
                target.localPosition.y,
                target.localPosition.z
            );

        }
    }
}