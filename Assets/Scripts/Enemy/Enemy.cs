using UnityEngine;

/// <summary>
/// Base enemy controller with movement and behavior
/// </summary>
public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private Transform playerTransform;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 5f;
    private float lastAttackTime = 0f;

    [Header("Animation")]
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string moveAnimationName = "Move";
    [SerializeField] private string attackAnimationName = "Attack";

    private EnemyHealth enemyHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private bool isMoving = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (enemyHealth == null)
        {
            enemyHealth = gameObject.AddComponent<EnemyHealth>();
        }

        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Start()
    {
        // Subscribe to death event
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnDeath;
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= OnDeath;
        }
    }

    private void Update()
    {
        if (enemyHealth == null || !enemyHealth.IsAlive()) return;

        // Get distance to player
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Detect player and decide action
        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                // Attack if in range
                TryAttack();
            }
            else
            {
                // Chase player
                ChasePlayer();
            }
        }
        else
        {
            // Stop moving if player out of range
            StopMoving();
        }

        UpdateAnimation();
    }

    /// <summary>
    /// Chase the player
    /// </summary>
    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        moveDirection = directionToPlayer * moveSpeed;
        isMoving = true;

        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
    }

    /// <summary>
    /// Stop moving
    /// </summary>
    private void StopMoving()
    {
        moveDirection = Vector2.zero;
        isMoving = false;
    }

    /// <summary>
    /// Attack the player
    /// </summary>
    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        PlayAnimation(attackAnimationName);

        // Deal damage to player
        if (playerTransform != null)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// Apply movement using Rigidbody2D
    /// </summary>
    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = moveDirection;
        }
    }

    /// <summary>
    /// Update animation based on state
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (isMoving)
        {
            PlayAnimation(moveAnimationName);
        }
        else
        {
            PlayAnimation(idleAnimationName);
        }
    }

    /// <summary>
    /// Play animation
    /// </summary>
    private void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.SetTrigger(animationName);
        }
    }

    /// <summary>
    /// Handle death
    /// </summary>
    private void OnDeath()
    {
        moveDirection = Vector2.zero;
        enabled = false;
    }

    // Getters and Setters
    public void SetMoveSpeed(float newSpeed) => moveSpeed = newSpeed;
    public void SetDetectionRange(float newRange) => detectionRange = newRange;
    public void SetAttackDamage(float newDamage) => attackDamage = newDamage;
    public void SetPlayerTransform(Transform player) => playerTransform = player;
    public EnemyHealth GetHealth() => enemyHealth;
}
