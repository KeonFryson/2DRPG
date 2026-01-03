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

    private InventoryManger inventoryManager;
    private Inventroy inventory;
    private InventoryItem currentHeldItem;

    private GameObject currentItemInstance;

    public Transform HandVis;


    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        inputActions = new InputSystem_Actions();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        target = this.transform;

        inventoryManager = FindFirstObjectByType<InventoryManger>();
        inventory = FindFirstObjectByType<Inventroy>();
        HandVis = transform.GetChild(0);
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += _ => Attack();

        if (inventoryManager != null)
        {
            Inventroy.OnInventoryChanged += OnInventoryChanged;
        }
    }

    private void OnDisable()
    {
        inputActions.Player.Attack.performed -= _ => Attack();
        inputActions.Disable();

        if (inventoryManager != null)
        {
            Inventroy.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    private void Update()
    {
        if (playerController == null || playerController.isDead)
            return;


        UpdateHeldItem();

        if (playerController.isHoldingItem)
        {
            UpdateSideBasedOnMouse(0.55f);

        }
        else
        {
            UpdateSideBasedOnMouse(0.267f);

        }


    }

    private void OnInventoryChanged(System.Collections.Generic.List<InventoryItem> currentInventory)
    {
        UpdateHeldItem();
    }

    private void UpdateHeldItem()
    {
        if (inventoryManager == null || inventory == null)
            return;

        int selectedSlot = inventoryManager.GetSelectedSlot();

        if (selectedSlot >= 0 && selectedSlot < inventory.inventory.Count)
        {
            InventoryItem selectedItem = inventory.inventory[selectedSlot];

            if (selectedItem != null && selectedItem.item != null)
            {
                // Only destroy and recreate if the item has changed
                if (currentHeldItem != selectedItem)
                {
                    // Destroy previous item instance if it exists
                    if (currentItemInstance != null)
                    {
                        Destroy(currentItemInstance);
                    }

                    // Instantiate the item prefab as a child of the hand
                    if (selectedItem.item.itemPrefab != null)
                    {
                        currentItemInstance = Instantiate(selectedItem.item.itemPrefab, HandVis);
                        currentItemInstance.transform.localPosition = Vector3.zero;
                        playerController.isHoldingItem = true;
                    }
                    else
                    {
                        // Fallback to sprite if no prefab is assigned
                        Debug.LogWarning("Item prefab is not assigned for item: " + selectedItem.item.itemName);
                        playerController.isHoldingItem = true;
                    }

                    currentHeldItem = selectedItem;
                }
            }
            else
            {
                // No item in this slot
                ClearHeldItem();
            }
        }
        else
        {
            // Selected slot is empty
            ClearHeldItem();
        }
    }

    private void ClearHeldItem()
    {
        if (currentItemInstance != null)
        {
            Destroy(currentItemInstance);
            currentItemInstance = null;
        }

        currentHeldItem = null;
        playerController.isHoldingItem = false;
    }

    private void Attack()
    {

        if (playerController == null || playerController.isDead)
            return;

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