using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class HealingItem : MonoBehaviour
{
    [Tooltip("Amount healed when item is used")]
    public float HealAmount = 25f;

    [Tooltip("Cooldown between uses (seconds)")]
    public float Cooldown = 3f;

    [Tooltip("Target health component. If null, searches the same GameObject.")]
    public MonoBehaviour TargetHealthBehaviour; // Inspector-friendly; must implement IHealth

    [Tooltip("Assign an InputAction (from your InputActions asset or PlayerInput) that triggers the use")]
    public InputActionReference UseAction;

    private IHealth targetHealth;
    private float lastUseTime = -999f;

    void Awake()
    {
        if (TargetHealthBehaviour != null && TargetHealthBehaviour is IHealth)
            targetHealth = TargetHealthBehaviour as IHealth;
        else
            targetHealth = GetComponent<IHealth>();
    }

    void OnEnable()
    {
        if (UseAction != null && UseAction.action != null)
            UseAction.action.performed += OnUsePerformed;
    }

    void OnDisable()
    {
        if (UseAction != null && UseAction.action != null)
            UseAction.action.performed -= OnUsePerformed;
    }

    private void OnUsePerformed(InputAction.CallbackContext ctx)
    {
        TryUse();
    }

    // Returns true if used successfully (healed > 0)
    public bool TryUse()
    {
        if (targetHealth == null) return false;
        if (targetHealth.IsDead) return false;
        if (!CanUse) return false;

        float healed = targetHealth.Heal(HealAmount);
        if (healed > 0f)
        {
            lastUseTime = Time.time;
            // TODO: play SFX / consume item from inventory
            return true;
        }
        return false;
    }

    // Public cooldown helpers for UI
    public float CooldownRemaining => Mathf.Max(0f, (lastUseTime + Cooldown) - Time.time);
    public bool CanUse => Time.time - lastUseTime >= Cooldown;
}
