using UnityEngine;
using System;

/// <summary>
/// Handles enemy health, damage, and death
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isAlive = true;

    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float deathDelay = 0f;
    [SerializeField] private AudioClip deathSoundClip;
    [SerializeField] private AudioClip damageSoundClip;

    // Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDeath;
    public event Action OnTakeDamage;

    private AudioSource audioSource;

    private void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Apply damage to the enemy
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (!isAlive) return;

        currentHealth -= damageAmount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnTakeDamage?.Invoke();

        // Play damage sound
        if (damageSoundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSoundClip);
        }

        // Visual feedback
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the enemy
    /// </summary>
    public void Heal(float healAmount)
    {
        if (!isAlive) return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Kill the enemy
    /// </summary>
    private void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        OnDeath?.Invoke();

        // Play death sound
        if (deathSoundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSoundClip);
        }

        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destroy enemy after delay
        Destroy(gameObject, deathDelay);
    }

    /// <summary>
    /// Flash the enemy sprite when taking damage
    /// </summary>
    private System.Collections.IEnumerator DamageFlash()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    // Getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsAlive() => isAlive;

    // Setters
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
