using UnityEngine;

public class PlatformToggle : MonoBehaviour
{
    [SerializeField] private PressurePlate plate;

    [Tooltip("ticks shit to be on and off lmao")]
    [SerializeField] private bool visibleWhenPressed = true;

    private bool? _lastState;

    void Update()
    {
        if (plate == null) return;

        bool show = plate.IsPressed == visibleWhenPressed;

        // Only touch the children when the state actually changes
        if (_lastState == show) return;
        _lastState = show;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(show);
        }
    }
}