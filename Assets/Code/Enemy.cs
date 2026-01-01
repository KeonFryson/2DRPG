using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float detectionAngle = 45f; // Half-angle of the cone

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    private Rigidbody2D rb;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("References")]
    [SerializeField] private Transform detectionTransform; // Child object that will rotate

    [Header("Debug")]
    [SerializeField] private bool showDetectionGizmos = true;

    private Transform player;
    private bool playerDetected = false;
    private SpriteRenderer spriteRenderer;
    public Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Create detection child object if it doesn't exist
        if (detectionTransform == null)
        {
            GameObject detectionObj = new GameObject("DetectionCone");
            detectionObj.transform.SetParent(transform);
            detectionObj.transform.localPosition = Vector3.zero;
            detectionObj.transform.localRotation = Quaternion.identity;
            detectionTransform = detectionObj.transform;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;

        // Find the player in the scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check if player is in detection cone
        playerDetected = IsPlayerInDetectionCone();
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        if (playerDetected)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Stop movement when player is not detected
            rb.linearVelocity = Vector2.zero;
        }
    }

    private bool IsPlayerInDetectionCone()
    {
        Vector2 directionToPlayer = (player.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if player is within range
        if (distanceToPlayer > detectionRange)
        {
            return false;
        }

        // Calculate the angle between detection cone's forward direction and direction to player
        Vector2 detectionForward = detectionTransform.right;
        float angleToPlayer = Vector2.Angle(detectionForward, directionToPlayer);

        // Check if player is within the cone angle
        return angleToPlayer <= detectionAngle;
    }

    private void MoveTowardsPlayer()
    {
        // Calculate direction to player
        Vector2 direction = (player.position - transform.position).normalized;

        // Move towards player using Rigidbody2D
        rb.linearVelocity = direction * moveSpeed;

        // Rotate the detection cone to face player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        detectionTransform.rotation = Quaternion.Slerp(
            detectionTransform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        // Flip sprite based on direction (sprite stays upright)
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (direction.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }

        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage, current health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
            Debug.Log("Enemy died.");
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }


    private void Die()
    {
        // Handle enemy death (e.g., play animation, drop loot, etc.)
        Destroy(gameObject);
    }

    // Visualize the detection cone in the editor
    private void OnDrawGizmos()
    {
        if (!showDetectionGizmos) return;

        // Use detection transform if available, otherwise use enemy transform
        Transform gizmoTransform = detectionTransform != null ? detectionTransform : transform;

        // Draw detection range
        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw cone edges
        Vector3 forward = gizmoTransform.right;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, detectionAngle) * forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -detectionAngle) * forward * detectionRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Draw arc
        Vector3 previousPoint = transform.position + leftBoundary;
        for (int i = 1; i <= 20; i++)
        {
            float currentAngle = -detectionAngle + (detectionAngle * 2 * i / 20f);
            Vector3 currentPoint = transform.position +
                (Quaternion.Euler(0, 0, currentAngle) * forward * detectionRange);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}