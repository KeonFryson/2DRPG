using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float detectionAngle = 45f;
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float waypointReachedDistance = 0.3f;
    private Rigidbody2D rb;

    [Header("Pathfinding Settings")]
    [SerializeField] private float pathUpdateInterval = 0.5f;
    [SerializeField] private bool usePathfinding = true;
    private float pathUpdateTimer = 0f;
    private List<Vector2> currentPath;
    private int currentWaypointIndex = 0;

    [Header("Random Walking Settings")]
    [SerializeField] private float randomWalkSpeed = 1.5f;
    //[SerializeField] private float randomWalkInterval = 3f;
    [SerializeField] private float randomWalkRadius = 5f;
    [SerializeField] private float idleTimeMin = 1f;
    [SerializeField] private float idleTimeMax = 3f;
    private Vector2 randomWalkTarget;
    //private float randomWalkTimer = 0f;
    private bool isWalking = false;
    private float idleTimer = 0f;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("References")]
    [SerializeField] private Transform detectionTransform;

    [Header("Debug")]
    [SerializeField] private bool showDetectionGizmos = true;
    [SerializeField] private bool showPathGizmos = true;

    private Transform player;
    private bool playerDetected = false;
    private bool playerInPursuitMode = false;
    private Vector2 lastKnownPlayerPosition;
    private bool hasLastKnownPosition = false;
    private SpriteRenderer spriteRenderer;
    public Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (detectionTransform == null)
        {
            GameObject detectionObj = new GameObject("DetectionCone");
            detectionObj.transform.SetParent(transform);
            detectionObj.transform.localPosition = Vector3.zero;
            detectionObj.transform.localRotation = Quaternion.identity;
            detectionTransform = detectionObj.transform;
        }
    }

    public void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure the player has the 'Player' tag.");
        }

        SetNewRandomWalkTarget();
    }

    public void Update()
    {
        if (player == null) return;

        playerDetected = IsPlayerInDetectionCone();

        // If player is detected in cone, enter pursuit mode
        if (playerDetected)
        {
            playerInPursuitMode = true;
            lastKnownPlayerPosition = player.position;
            hasLastKnownPosition = true;
        }
        // If in pursuit mode, check if player is still in range
        else if (playerInPursuitMode)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                // Player still in range, check for obstacles
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, distanceToPlayer, obstacleLayers);

                if (hit.collider == null)
                {
                    // No obstacles, continue pursuing
                    playerDetected = true;
                    lastKnownPlayerPosition = player.position;
                    hasLastKnownPosition = true;
                }
                else
                {
                    // Obstacle blocking view, exit pursuit mode
                    playerInPursuitMode = false;
                }
            }
            else
            {
                // Player left detection range, exit pursuit mode
                playerInPursuitMode = false;
            }
        }

        if (usePathfinding && AStarPathfinder.Instance != null)
        {
            pathUpdateTimer += Time.deltaTime;
            if (pathUpdateTimer >= pathUpdateInterval)
            {
                pathUpdateTimer = 0f;
                UpdatePath();
            }
        }

        if (!playerDetected && !hasLastKnownPosition)
        {
            UpdateRandomWalk();
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        if (usePathfinding && currentPath != null && currentPath.Count > 0)
        {
            FollowPath();
        }
        else
        {
            FallbackMovement();
        }
    }

    private void UpdateRandomWalk()
    {
        if (isWalking)
        {
            float distanceToTarget = Vector2.Distance(transform.position, randomWalkTarget);
            if (distanceToTarget <= waypointReachedDistance)
            {
                isWalking = false;
                idleTimer = Random.Range(idleTimeMin, idleTimeMax);
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                SetNewRandomWalkTarget();
            }
        }
    }

    private void SetNewRandomWalkTarget()
    {
        int maxAttempts = 10;
        int attempts = 0;
        bool validTargetFound = false;

        while (attempts < maxAttempts && !validTargetFound)
        {
            Vector2 randomDirection = Random.insideUnitCircle * randomWalkRadius;
            Vector2 potentialTarget = (Vector2)transform.position + randomDirection;

            // Check if there's an obstacle at the target position
            Collider2D hit = Physics2D.OverlapCircle(potentialTarget, waypointReachedDistance, obstacleLayers);

            if (hit == null)
            {
                // Also check if there's a clear path to the target
                RaycastHit2D pathCheck = Physics2D.Raycast(transform.position, randomDirection.normalized, randomDirection.magnitude, obstacleLayers);

                if (pathCheck.collider == null)
                {
                    randomWalkTarget = potentialTarget;
                    validTargetFound = true;
                }
            }

            attempts++;
        }

        // If no valid target found after max attempts, just stay in place
        if (!validTargetFound)
        {
            randomWalkTarget = transform.position;
        }

        isWalking = true;
    }

    private void UpdatePath()
    {
        if (AStarPathfinder.Instance == null)
        {
            Debug.LogWarning("AStarPathfinder instance not found!");
            return;
        }

        Vector2 targetPosition = Vector2.zero;
        bool shouldFindPath = false;

        if (playerDetected)
        {
            targetPosition = player.position;
            shouldFindPath = true;
        }
        else if (hasLastKnownPosition)
        {
            float distanceToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);
            if (distanceToLastKnown > stopDistance)
            {
                targetPosition = lastKnownPlayerPosition;
                shouldFindPath = true;
            }
            else
            {
                hasLastKnownPosition = false;
            }
        }

        if (shouldFindPath)
        {
            currentPath = AStarPathfinder.Instance.FindPath(transform.position, targetPosition);
            currentWaypointIndex = 0;
        }
        else
        {
            currentPath = null;
        }
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (currentWaypointIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetWaypoint = currentPath[currentWaypointIndex];
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint);

        if (distanceToWaypoint <= waypointReachedDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= currentPath.Count)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            targetWaypoint = currentPath[currentWaypointIndex];
        }

        MoveTowardsPosition(targetWaypoint);
    }

    private void FallbackMovement()
    {
        if (playerDetected)
        {
            MoveTowardsPosition(player.position);
        }
        else if (hasLastKnownPosition)
        {
            float distanceToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);

            if (distanceToLastKnown > stopDistance)
            {
                MoveTowardsPosition(lastKnownPlayerPosition);
            }
            else
            {
                hasLastKnownPosition = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else if (isWalking)
        {
            MoveTowardsPosition(randomWalkTarget, randomWalkSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private bool IsPlayerInDetectionCone()
    {
        Vector2 directionToPlayer = (player.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
        {
            return false;
        }

        Vector2 detectionForward = detectionTransform.right;
        float angleToPlayer = Vector2.Angle(detectionForward, directionToPlayer);

        if (angleToPlayer > detectionAngle)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, obstacleLayers);

        if (hit.collider != null)
        {
            return false;
        }

        return true;
    }

    private void MoveTowardsPosition(Vector2 targetPosition, float speed = -1f)
    {
        if (speed < 0) speed = moveSpeed;

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        detectionTransform.rotation = Quaternion.Slerp(
            detectionTransform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

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
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!showDetectionGizmos) return;

        Transform gizmoTransform = detectionTransform != null ? detectionTransform : transform;

        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 forward = gizmoTransform.right;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, detectionAngle) * forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -detectionAngle) * forward * detectionRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        Vector3 previousPoint = transform.position + leftBoundary;
        for (int i = 1; i <= 20; i++)
        {
            float currentAngle = -detectionAngle + (detectionAngle * 2 * i / 20f);
            Vector3 currentPoint = transform.position +
                (Quaternion.Euler(0, 0, currentAngle) * forward * detectionRange);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        if (hasLastKnownPosition && Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
        }

        if (Application.isPlaying && player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position);
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer <= detectionRange)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, obstacleLayers);

                if (hit.collider != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, hit.point);
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, player.position);
                }
            }
        }

        if (showPathGizmos && Application.isPlaying && currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawWireSphere(currentPath[i], 0.2f);
            }
            Gizmos.DrawWireSphere(currentPath[currentPath.Count - 1], 0.2f);

            if (currentWaypointIndex < currentPath.Count)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentPath[currentWaypointIndex], 0.3f);
            }
        }

        if (Application.isPlaying && !playerDetected && !hasLastKnownPosition)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(randomWalkTarget, 0.3f);
            Gizmos.DrawLine(transform.position, randomWalkTarget);
        }
    }
}