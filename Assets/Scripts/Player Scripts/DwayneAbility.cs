using UnityEngine;

public class DwayneAbility : MonoBehaviour
{
    private float _climbSpeed = 4f; // Climbing speed
    private Collider2D _climbableWall;
    private Rigidbody2D _rigidbody;
    private float _gravityScale;
    private bool _isClimbActive;

    public bool IsClimbActive => _isClimbActive;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _gravityScale = _rigidbody.gravityScale; // Save gravity so it can be restored when climbing ends
    }

    public void ActivateClimb()
    {
        // Pressing F again releases climb
        if (_isClimbActive)
        {
            ResetClimb();
            return;
        }

        // Only start climbing while touching a wall
        if (_climbableWall == null)
        {
            return;
        }

        _isClimbActive = true;
        _rigidbody.gravityScale = 0f;
        _rigidbody.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!_isClimbActive)
        {
            return;
        }

        if (!GameManager.allowMovement)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        // The Vertical axis maps to W/S
        float climbDirection = Input.GetAxisRaw("Vertical");
        _rigidbody.linearVelocity = new Vector2(0f, climbDirection * _climbSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateWallContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateWallContact(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == _climbableWall)
        {
            ClearClimbableWall();
        }
    }

    private void UpdateWallContact(Collision2D collision)
    {
        // Pushable objects and buttons are not climbable
        if (collision.gameObject.CompareTag("pushable") || collision.gameObject.CompareTag("button"))
        {
            if (collision.collider == _climbableWall)
            {
                ClearClimbableWall();
            }

            return;
        }

        // A horizontal contact normal means the player is touching a wall
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (Mathf.Abs(collision.GetContact(i).normal.x) > 0.5f)
            {
                _climbableWall = collision.collider;
                return;
            }
        }

        if (collision.collider == _climbableWall)
        {
            ClearClimbableWall();
        }
    }

    private void ClearClimbableWall()
    {
        _climbableWall = null;
        if (_isClimbActive)
        {
            ResetClimb();
        }
    }

    private void ResetClimb()
    {
        _isClimbActive = false;
        _rigidbody.gravityScale = _gravityScale;
        _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, 0f);
    }
}
