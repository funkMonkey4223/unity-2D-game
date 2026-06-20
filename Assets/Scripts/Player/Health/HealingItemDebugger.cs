using UnityEngine;
using UnityEngine.InputSystem;

// HealingItemDebugger: reports why HealingItem.TryUse() succeeds or fails.
// - Assign Item to the HealingItem instance you want to inspect.
// - Optionally assign TestAction (InputActionReference) to trigger the debug check.
// - If TestAction is empty, the fallbackKey (Keyboard.current) is used.
public class HealingItemDebugger : MonoBehaviour
{
    [Tooltip("The HealingItem to inspect")]
    public HealingItem Item;

    [Tooltip("Optional: an InputAction (from your InputActions asset) that triggers the debug check")]
    public InputActionReference TestAction;

    [Tooltip("Fallback key used when TestAction is not assigned (checked via Keyboard.current)")]
    public Key fallbackKey = Key.L;

    void OnEnable()
    {
        if (TestAction != null && TestAction.action != null)
        {
            if (!TestAction.action.enabled) TestAction.action.Enable();
            TestAction.action.performed += OnTestActionPerformed;
        }
    }

    void OnDisable()
    {
        if (TestAction != null && TestAction.action != null)
            TestAction.action.performed -= OnTestActionPerformed;
    }

    private void OnTestActionPerformed(InputAction.CallbackContext ctx)
    {
        PrintStatusAndTry();
    }

    void Update()
    {
        // Fallback to keyboard if no TestAction assigned
        if (TestAction == null || TestAction.action == null)
        {
            if (Keyboard.current != null && Keyboard.current[fallbackKey].wasPressedThisFrame)
                PrintStatusAndTry();
        }
    }

    // Performs the same checks as before and attempts a TryUse(), printing detailed info.
    public void PrintStatusAndTry()
    {
        if (Item == null)
        {
            Debug.LogError("[HealingItemDebugger] Item is null. Assign the HealingItem in inspector.");
            return;
        }

        Debug.Log($"[HealingItemDebugger] Item: {Item.name}, HealAmount={Item.HealAmount}, Cooldown={Item.Cooldown}s");
        Debug.Log($"[HealingItemDebugger] CanUse={Item.CanUse}, CooldownRemaining={Item.CooldownRemaining:F2}s");

        // Report the assigned TargetHealthBehaviour (if any)
        var assigned = Item.TargetHealthBehaviour;
        Debug.Log($"[HealingItemDebugger] TargetHealthBehaviour assigned? {(assigned != null ? assigned.GetType().Name : "null")}");

        // Try to resolve IHealth like HealingItem does internally
        IHealth ihealth = null;
        if (assigned != null && assigned is IHealth) ihealth = assigned as IHealth;
        else
        {
            // Look for any component on the same GameObject that implements IHealth
            var comps = Item.GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c is IHealth) { ihealth = c as IHealth; break; }
            }
        }

        if (ihealth != null)
        {
            Debug.Log($"[HealingItemDebugger] Resolved IHealth -> Type={ihealth.GetType().Name}, Current={ihealth.Current}, Max={ihealth.Maximum}, IsDead={ihealth.IsDead}");
        }
        else
        {
            Debug.LogWarning("[HealingItemDebugger] No IHealth found. HealingItem will fail because targetHealth is null.");
        }

        // Attempt use and report result
        bool result = Item.TryUse();
        Debug.Log($"[HealingItemDebugger] TryUse result = {result}");
    }
}
