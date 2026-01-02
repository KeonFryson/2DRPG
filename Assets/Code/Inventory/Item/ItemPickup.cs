using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ItemData item;
    private Inventroy Inventroy;
    public CircleCollider2D pickupTrigger;
    public GameObject player;

    private void Awake()
    {
        Inventroy = FindFirstObjectByType<Inventroy>();
        pickupTrigger = GetComponent<CircleCollider2D>();
        player = GameObject.FindWithTag("Player");

        if (player != null && transform.IsChildOf(player.transform))
        {
            enabled = false;
            Destroy(pickupTrigger);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PickUpItem();
        }
    }

    
    private void PickUpItem()
    {
        bool wasAdded = Inventroy.Add(item);
        if (wasAdded)
        {
            Debug.Log("Picked up: " + item.itemName);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Could not pick up: " + item.itemName + ". Inventory may be full.");
        }
    }
}
