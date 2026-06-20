using UnityEngine;
using System;

/// <summary>
/// Defines the interface for health systems
/// </summary>
public interface IHealth
{
    float Current { get; }
    float Maximum { get; }
    bool IsDead { get; }

    event Action<DamageInfo, float> OnDamaged;
    event Action<float> OnHealed;
    event Action OnDeath;
    event Action<float, float> OnHealthChanged;

    float TakeDamage(DamageInfo info);
    float Heal(float amount);
}
