using Unity.VisualScripting;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    private Vector3 _originalPos;
    private int _objectsOnPlate;  // checks how many valid fuckers are on the plate so it can be activated by multiple objects at once.
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _maxDownDistance = 0.1f;

    [SerializeField] private bool _isWinButton = false; // If this is a win button, it will be yellow when not pressed.

    public bool IsPressed => _objectsOnPlate > 0;

    void Start()
    {
        _originalPos = transform.position;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = _isWinButton ? Color.yellow : Color.red;
    }

    //Pressure plate only goes down when Player or Pushable object is on top of it. 
    //It also checks if it isnt on top of it so it goes back up.
    private bool IsValidObject(Collider2D other)
    {
        return other.CompareTag("Player") || other.CompareTag("pushable");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;

        _objectsOnPlate++;
        _spriteRenderer.color = Color.green;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;

        _objectsOnPlate = Mathf.Max(0, _objectsOnPlate - 1);
        if (!IsPressed)
        {
            _spriteRenderer.color = _isWinButton ? Color.yellow : Color.red;
        }
    }

    void FixedUpdate()
    {
        if (IsPressed && transform.position.y > _originalPos.y - _maxDownDistance)
        {
            transform.Translate(0f, -0.01f, 0f);
        }
        else if (!IsPressed && transform.position.y < _originalPos.y)
        {
            transform.Translate(0f, 0.01f, 0f);
        }
    }
}