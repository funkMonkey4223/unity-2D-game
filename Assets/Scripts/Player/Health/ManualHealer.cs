using UnityEngine;
using UnityEngine.InputSystem;

// Manual healer compatible with the new Input System.
// - Preferred: assign a TestAction (InputActionReference) in the inspector and bind it to a key/button.
// - Fallback: if no TestAction is assigned, it listens for Key.K via Keyboard.current.
public class ManualHealer : MonoBehaviour
{
    [Tooltip("Drag the HealingItem component here in the inspector")]
    public HealingItem Item;

    [Tooltip("Optional: assign an InputAction (from your InputActions asset) to trigger the test.")]
    public InputActionReference TestAction;

    [Tooltip("Fallback key used when TestAction is not assigned")]
    public InputSystemKey fallbackKey = new InputSystemKey { key = Key.K };

    void OnEnable()
    {
        if (TestAction != null && TestAction.action != null)
        {
            TestAction.action.performed += OnTestActionPerformed;
            if (!TestAction.action.enabled) TestAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (TestAction != null && TestAction.action != null)
            TestAction.action.performed -= OnTestActionPerformed;
    }

    private void OnTestActionPerformed(InputAction.CallbackContext ctx)
    {
        TryUse();
    }

    void Update()
    {
        // If no TestAction assigned, use InputSystem keyboard fallback
        if (TestAction == null || TestAction.action == null)
        {
            if (Keyboard.current != null && Keyboard.current[fallbackKey.key].wasPressedThisFrame)
                TryUse();
        }
    }

    void TryUse()
    {
        Debug.Log("[ManualHealer] Calling TryUse()");
        bool ok = Item != null && Item.TryUse();
        Debug.Log($"[ManualHealer] TryUse result = {ok}");
    }

    [System.Serializable]
    public struct InputSystemKey
    {
        public Key key;
    }
}
