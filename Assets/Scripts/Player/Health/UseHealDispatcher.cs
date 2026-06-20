using UnityEngine;
using UnityEngine.InputSystem;

// Attach to the player (or a central input receiver). Use with PlayerInput -> "Invoke Unity Events"
// In PlayerInput's Event for the UseHeal action, set this object and choose UseHealDispatcher.OnUseHeal.
public class UseHealDispatcher : MonoBehaviour
{
    public HealingItem healingItem;

    // PlayerInput will pass an InputAction.CallbackContext
    public void OnUseHeal(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && healingItem != null)
            healingItem.TryUse();
    }
}
