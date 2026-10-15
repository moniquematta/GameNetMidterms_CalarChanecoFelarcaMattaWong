using UnityEngine;

public class PlatformToggle : MonoBehaviour
{
    [SerializeField] private PressurePlate[] plate;

    [Tooltip("ticks shit to be on and off lmao")]
    [SerializeField] private bool visibleWhenPressed = true;

    private bool? _lastState;
    private bool _allPressed = false;

    void Start()
    {
        if (plate == null || plate.Length == 0)
        {
            Debug.LogWarning("No plates assigned to PlatformToggle script on " + gameObject.name);
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(!visibleWhenPressed);
        }
    }

    void Update()
    {
        if (plate == null) return;

        //If all pressure plates are pressed, then show win platform.
        _allPressed = true;
        foreach (var p in plate)
        {
            //Debug.Log($"Plate {p.name}: IsPressed={p.IsPressed}, visibleWhenPressed={visibleWhenPressed}, _allPressed={_allPressed}");
            if (p.IsPressed != visibleWhenPressed)
            {
                _allPressed = false;
                break;
            }
        }

        //Only show the win platform if all pressure plates are pressed.
        bool show = _allPressed;

        // Only touch the children when the state actually changes
        if (_lastState == show) return;
        _lastState = show;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(show);
        }
    }
}