using UnityEngine;

public class EmmaAbility : MonoBehaviour
{
    private bool _isCollided = false;
    private bool _isDashActive = false;
    private float _dashSpeed = 20f; // Speed of the dash
    private float _dashDuration = 0.2f; // Duration of the dash in seconds
    public SpriteRenderer Renderer;

    public void ActivateDash()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left; // Determine direction based on facing
        rb.linearVelocity = new Vector2(dashDirection.x * _dashSpeed, rb.linearVelocity.y);
        _isDashActive = true; // Dash started

        if (_isCollided) // Check if the player has collided with a wall during the dash
        {
            ResetDash();
        } else
            Invoke("ResetDash", _dashDuration); // Reset after dash duration
    }

    private void ResetDash()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        _isCollided = false;
        _isDashActive = false; // Dash ended
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isDashActive)
        {
            return;
        }

        // Check if the collided object is not pushable or a button
        if (!collision.gameObject.CompareTag("pushable") && !collision.gameObject.CompareTag("button"))
        {
            _isCollided = true;
            Debug.Log("Emma has hit a wall! Cancelling dash.");
        }
    }
}
