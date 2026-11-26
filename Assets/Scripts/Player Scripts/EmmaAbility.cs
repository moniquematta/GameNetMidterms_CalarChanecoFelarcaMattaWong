using UnityEngine;

public class EmmaAbility : MonoBehaviour
{
    private bool _isCollided = false;
    private float _dashSpeed = 10f; // Speed of the dash
    private float _dashDuration = 0.4f; // Duration of the dash in seconds

    public void ActivateDash()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left; // Determine direction based on facing
        rb.linearVelocity = (dashDirection * _dashSpeed) * Time.deltaTime;

        if (_isCollided) // Check if the player has collided with a wall during the dash
        {
            ResetDash();
        } else
            Invoke("ResetDash", _dashDuration); // Reset after dash duration
    }

    private void ResetDash()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        _isCollided = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collided object is not pushable or a button
        if (!collision.gameObject.CompareTag("pushable") && !collision.gameObject.CompareTag("button"))
        {
            _isCollided = true;
            Debug.Log("Emma has hit a wall! Cancelling dash.");
        }
    }
}
