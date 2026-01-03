using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManger : MonoBehaviour
{
 

    public GameObject slotPrefab;

    public List<InventorySlot> inventorySlots = new List<InventorySlot>(4);

    private int currentSelectedSlot = 0;
    private InputSystem_Actions inputActions;
    private Inventroy inventory;
    public PlayerController playerController;
    private void Awake()
    { 
        inputActions = new InputSystem_Actions();
        inventory = GetComponent<Inventroy>();
        playerController = GetComponent<PlayerController>();
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventroy>();
        }
        if(playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
    }

    private void Start()
    {
        InitializeHotbar();
        UpdateSlotSelection();
    }

    public void OnEnable()
    {
        Inventroy.OnInventoryChanged += DrawHotbar;

        inputActions.Player.Enable();
        inputActions.Player.ScrollInventory.performed += OnScrollInventory;
        inputActions.Player.SelectSlot.performed += OnSelectSlot;
        inputActions.Player.DropItem.performed += OnDropItem;
    }

    public void OnDisable()
    {
        Inventroy.OnInventoryChanged -= DrawHotbar;

        inputActions.Player.ScrollInventory.performed -= OnScrollInventory;
        inputActions.Player.SelectSlot.performed -= OnSelectSlot;
        inputActions.Player.DropItem.performed -= OnDropItem;
        inputActions.Player.Disable();
    }

    private void OnScrollInventory(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<float>();

        if (scrollValue > 0f)
        {
            currentSelectedSlot--;
            if (currentSelectedSlot < 0)
                currentSelectedSlot = inventorySlots.Count - 1;
            UpdateSlotSelection();
        }
        else if (scrollValue < 0f)
        {
            currentSelectedSlot++;
            if (currentSelectedSlot >= inventorySlots.Count)
                currentSelectedSlot = 0;
            UpdateSlotSelection();
        }
    }

    private void OnSelectSlot(InputAction.CallbackContext context)
    {
        // Get the control that triggered this (e.g., "1", "2", "3", "4")
        string keyName = context.control.name;

        if (int.TryParse(keyName, out int slotNumber))
        {
            int slotIndex = slotNumber - 1; // Convert 1-based to 0-based index
            if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
            {
                currentSelectedSlot = slotIndex;
                UpdateSlotSelection();
            }
        }
    }

    private void OnDropItem(InputAction.CallbackContext context)
    {
        if (inventory == null || inventory.inventory == null || inventory.inventory.Count == 0)
            return;

        // Check if the current selected slot has an item
        if (currentSelectedSlot < inventory.inventory.Count)
        {
            InventoryItem itemToDrop = inventory.inventory[currentSelectedSlot];

            if (itemToDrop != null && itemToDrop.item != null && itemToDrop.item.itemPrefab != null)
            {
                // Spawn the item in the world near the player
                Vector3 dropPosition = playerController.transform.position + (Vector3)Random.insideUnitCircle.normalized * 1.5f;
                GameObject droppedItem = Instantiate(itemToDrop.item.itemPrefab, dropPosition, itemToDrop.item.itemPrefab.transform.rotation);

                Debug.Log("Dropped: " + itemToDrop.item.itemName);

                // Remove the item from inventory
                inventory.Remove(itemToDrop.item);
            }
        }
    }

    void UpdateSlotSelection()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].SetSelected(i == currentSelectedSlot);
        }
    }

    void InitializeHotbar()
    {
        // Create hotbar slots from prefab
        for (int i = 0; i < inventorySlots.Capacity; i++)
        {
            GameObject slotInstance = Instantiate(slotPrefab, this.transform);
            InventorySlot slot = slotInstance.GetComponent<InventorySlot>();
            slot.ClearSlot();
            inventorySlots.Add(slot);
        }
    }

    void DrawHotbar(List<InventoryItem> currentInventory)
    {
        if (currentInventory == null) return;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < currentInventory.Count)
            {
                inventorySlots[i].DrawSlot(currentInventory[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    void ResetInventory()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            slot.ClearSlot();
        }
    }

    public int GetSelectedSlot()
    {
        return currentSelectedSlot;
    }
}