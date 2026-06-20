using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HealthComponent))]
public class HealthRegenerator : MonoBehaviour
{
    [Tooltip("Delay after taking damage before regen begins (seconds)")]
    public float DelayBeforeRegen = 15f;

    [Tooltip("If zero, uses Model.RegenPerSecond on the HealthComponent (HealthModel).")]
    public float RegenPerSecondOverride = 0f;

    [Tooltip("Regen tick interval (seconds)")]
    public float TickInterval = 0.1f;

    HealthComponent health;
    float lastDamageTime = -999f;
    Coroutine regenCoroutine;

    void Awake()
    {
        health = GetComponent<HealthComponent>();
    }

    void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
    }

    void HandleDamaged(DamageInfo info, float actualTaken)
    {
        lastDamageTime = Time.time;
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    void Update()
    {
        if (health.IsDead) return;

        if (Time.time - lastDamageTime >= DelayBeforeRegen)
        {
            if (regenCoroutine == null)
                regenCoroutine = StartCoroutine(DoRegen());
        }
    }

    IEnumerator DoRegen()
    {
        while (!health.IsDead)
        {
            float regenRate = RegenPerSecondOverride > 0f ? RegenPerSecondOverride : (health != null && health is object && health is HealthComponent ? (health as HealthComponent).Model?.RegenPerSecond ?? 0f : 0f);
            if (regenRate <= 0f) yield break;

            float healAmount = regenRate * TickInterval;
            if (healAmount > 0f) health.Heal(healAmount);

            yield return new WaitForSeconds(TickInterval);
        }
    }
}
