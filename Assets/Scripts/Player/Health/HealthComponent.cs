using UnityEngine;
using System;

[DisallowMultipleComponent]
public class HealthComponent : MonoBehaviour, IHealth
{
    [Tooltip("Tunable model shared across prefabs")]
    public HealthModel Model;

    [SerializeField, Tooltip("Starting current HP. If zero or negative, set to Model.MaxHP on Awake.")]
    private float current = 0f;

    public float Current => current;
    public float Maximum { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<DamageInfo, float> OnDamaged;
    public event Action<float> OnHealed;
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;

    void Awake()
    {
        if (Model == null)
        {
            Debug.LogWarning($"{name}: HealthModel not assigned. Creating default model.");
            Model = ScriptableObject.CreateInstance<HealthModel>();
        }

        Maximum = Model.MaxHP;
        if (current <= 0f) current = Maximum;
        current = Mathf.Clamp(current, 0f, Maximum);
    }

    // Damage application uses the model's armor/resistances.
    public float TakeDamage(DamageInfo info)
    {
        if (IsDead) return 0f;

        float damage = info.Amount;

        if (!info.BypassArmor && Model != null)
        {
            damage = Mathf.Max(0f, damage - Model.ArmorFlat);
            damage *= (1f - Model.ArmorPercent);
            float resist = Model.GetResistance(info.Type);
            damage *= (1f - resist);
        }

        damage = Mathf.Max(0f, damage);

        float before = current;
        current = Mathf.Clamp(current - damage, 0f, Maximum);
        float actualTaken = before - current;

        if (actualTaken > 0f)
        {
            OnDamaged?.Invoke(info, actualTaken);
            OnHealthChanged?.Invoke(current, Maximum);
        }

        if (current <= 0f && !IsDead)
        {
            IsDead = true;
            OnDeath?.Invoke();
        }

        return actualTaken;
    }

    public float Heal(float amount)
    {
        if (IsDead) return 0f;
        float before = current;
        current = Mathf.Clamp(current + amount, 0f, Maximum);
        float healed = current - before;
        if (healed > 0f)
        {
            OnHealed?.Invoke(healed);
            OnHealthChanged?.Invoke(current, Maximum);
        }
        return healed;
    }

    public void SetMax(float max, bool adjustCurrent = true)
    {
        Maximum = Mathf.Max(1f, max);
        if (adjustCurrent) current = Mathf.Clamp(current, 0f, Maximum);
        OnHealthChanged?.Invoke(current, Maximum);
    }

    public void AddMax(float amount, bool adjustCurrent = true)
    {
        SetMax(Maximum + amount, adjustCurrent);
    }

    // Optional helper: fully restore
    public void FullRestore()
    {
        if (IsDead) return;
        current = Maximum;
        OnHealed?.Invoke(current);
        OnHealthChanged?.Invoke(current, Maximum);
    }
}
