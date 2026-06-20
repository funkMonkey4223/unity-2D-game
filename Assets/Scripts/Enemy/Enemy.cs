using UnityEngine;

/// <summary>
/// Base enemy controller with movement and behavior
/// Compatible with IHealth interface system
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
    private IHealth playerHealth;
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
                CachePlayerHealth();
            }
        }
        else
        {
            CachePlayerHealth();
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
    /// Attack the player using IHealth interface
    /// </summary>
    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        PlayAnimation(attackAnimationName);

        // Deal damage to player if IHealth interface is available
        if (playerHealth != null && !playerHealth.IsDead)
        {
            DamageInfo damageInfo = new DamageInfo
            {
                Amount = attackDamage,
                Type = DamageType.Physical,
                BypassArmor = false
            };

            playerHealth.TakeDamage(damageInfo);
        }
    }

    /// <summary>
    /// Cache player's IHealth interface for efficient damage dealing
    /// </summary>
    private void CachePlayerHealth()
    {
        if (playerTransform == null) return;

        playerHealth = playerTransform.GetComponent<IHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning($"Player '{playerTransform.name}' does not have a component implementing IHealth interface. Enemy damage will not work.");
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
    
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
        CachePlayerHealth();
    }

    public EnemyHealth GetHealth() => enemyHealth;
}
