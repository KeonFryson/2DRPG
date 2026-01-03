using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float detectionAngle = 45f;
    [SerializeField] protected LayerMask obstacleLayers;

    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float rotationSpeed = 5f;
    [SerializeField] protected float stopDistance = 0.5f;
    [SerializeField] protected float waypointReachedDistance = 0.3f;
    protected Rigidbody2D rb;

    [Header("Pathfinding Settings")]
    [SerializeField] protected float pathUpdateInterval = 0.5f;
    [SerializeField] protected bool usePathfinding = true;
    protected float pathUpdateTimer = 0f;
    protected List<Vector2> currentPath;
    protected int currentWaypointIndex = 0;

    [Header("Random Walking Settings")]
    [SerializeField] protected float randomWalkSpeed = 1.5f;
    [SerializeField] protected float randomWalkRadius = 5f;
    [SerializeField] protected float idleTimeMin = 1f;
    [SerializeField] protected float idleTimeMax = 3f;
    protected Vector2 randomWalkTarget;
    protected bool isWalking = false;
    protected float idleTimer = 0f;

    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected float deathAnimationDuration = 1f;

    [Header("References")]
    [SerializeField] protected Transform detectionTransform;

    [Header("Debug")]
    [SerializeField] protected bool showDetectionGizmos = true;
    [SerializeField] protected bool showPathGizmos = true;

    protected Transform player;
    protected bool playerDetected = false;
    protected bool playerInPursuitMode = false;
    protected Vector2 lastKnownPlayerPosition;
    protected bool hasLastKnownPosition = false;
    protected SpriteRenderer spriteRenderer;
    public Animator animator;
    protected bool isDead = false;

    protected virtual void Awake()
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

    protected virtual void Start()
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

    protected virtual void Update()
    {
        if (player == null || isDead) return;

        playerDetected = IsPlayerInDetectionCone();

        if (playerDetected)
        {
            playerInPursuitMode = true;
            lastKnownPlayerPosition = player.position;
            hasLastKnownPosition = true;
        }
        else if (playerInPursuitMode)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, distanceToPlayer, obstacleLayers);

                if (hit.collider == null)
                {
                    playerDetected = true;
                    lastKnownPlayerPosition = player.position;
                    hasLastKnownPosition = true;
                }
                else
                {
                    playerInPursuitMode = false;
                }
            }
            else
            {
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

    protected virtual void FixedUpdate()
    {
        if (player == null || isDead) return;

        if (usePathfinding && currentPath != null && currentPath.Count > 0)
        {
            FollowPath();
        }
        else
        {
            FallbackMovement();
        }
    }

    protected virtual void UpdateRandomWalk()
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

    protected virtual void SetNewRandomWalkTarget()
    {
        int maxAttempts = 10;
        int attempts = 0;
        bool validTargetFound = false;

        while (attempts < maxAttempts && !validTargetFound)
        {
            Vector2 randomDirection = Random.insideUnitCircle * randomWalkRadius;
            Vector2 potentialTarget = (Vector2)transform.position + randomDirection;

            Collider2D hit = Physics2D.OverlapCircle(potentialTarget, waypointReachedDistance, obstacleLayers);

            if (hit == null)
            {
                RaycastHit2D pathCheck = Physics2D.Raycast(transform.position, randomDirection.normalized, randomDirection.magnitude, obstacleLayers);

                if (pathCheck.collider == null)
                {
                    randomWalkTarget = potentialTarget;
                    validTargetFound = true;
                }
            }

            attempts++;
        }

        if (!validTargetFound)
        {
            randomWalkTarget = transform.position;
        }

        isWalking = true;
    }

    protected virtual void UpdatePath()
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

    protected virtual void FollowPath()
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

    protected virtual void FallbackMovement()
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

    protected virtual bool IsPlayerInDetectionCone()
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

    protected virtual void MoveTowardsPosition(Vector2 targetPosition, float speed = -1f)
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

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("hurt");
        Debug.Log($"{GetType().Name} took {damage} damage, current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isDead", isDead);
        StartCoroutine(DeathCoroutine());
    }

    protected virtual IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        Destroy(gameObject);
    }

    protected virtual void OnDrawGizmos()
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