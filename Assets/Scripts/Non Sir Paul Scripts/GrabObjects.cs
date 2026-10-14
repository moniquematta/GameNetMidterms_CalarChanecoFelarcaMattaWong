using UnityEngine;
using UnityEngine.InputSystem;

public class GrabObjects : MonoBehaviour
{
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform rayPoint;
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private LayerMask grabbableLayers; // set this to "Objects" in the Inspector

    // Name of the child object on a grabbable that marks where the player holds it.
    // If an object has no child with this name, it is held by its center like before.
    [SerializeField] private string objectGrabPointName = "grabPoint";

    private GameObject grabbedObject;
    private Collider2D[] playerColliders;

    private void Awake()
    {
        // Every collider on the player (and its children) so the held object can ignore all of them
        playerColliders = GetComponentsInChildren<Collider2D>();
    }

    private void Update()
    {
        // transform.right ignores scale. If you flip the player with localScale.x = -1,
        // use this instead: Vector2.right * Mathf.Sign(transform.localScale.x)
        Vector2 dir = transform.right;

        Debug.DrawRay(rayPoint.position, dir * rayDistance, Color.red);

        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        // Already holding something: E always drops it, no raycast needed
        if (grabbedObject != null)
        {
            Release();
            return;
        }

        // Not holding anything: look for something to grab
        RaycastHit2D hit = Physics2D.Raycast(rayPoint.position, dir, rayDistance, grabbableLayers);
        if (hit.collider != null)
        {
            Grab(hit.collider.gameObject);
        }
    }

    private void Grab(GameObject obj)
    {
        grabbedObject = obj;

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;   // use rb.velocity on Unity versions before 6
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Stops the held box from pushing the player (this was causing the gliding)
        SetIgnoreCollision(obj, true);

        //basically, if the object has a child called "grabPoint", the rock will hold it by that point, otherwise rock will hold it by its center
        Transform objGrabPoint = obj.transform.Find(objectGrabPointName);
        Vector3 offset = objGrabPoint != null
            ? obj.transform.position - objGrabPoint.position // changes the "center" of the object to the grabPoint
            : Vector3.zero; // no grab point on the object: hold by the center

        obj.transform.SetParent(grabPoint);
        obj.transform.position = grabPoint.position + offset;
    }

    private void Release()
    {
        SetIgnoreCollision(grabbedObject, false);

        grabbedObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        grabbedObject.transform.SetParent(null);
        grabbedObject = null;
    }

    private void SetIgnoreCollision(GameObject obj, bool ignore)
    {
        foreach (Collider2D boxCol in obj.GetComponents<Collider2D>())
        {
            foreach (Collider2D playerCol in playerColliders)
            {
                // The box is a child of the player while held, so skip its own colliders
                if (playerCol != boxCol)
                {
                    Physics2D.IgnoreCollision(boxCol, playerCol, ignore);
                }
            }
        }
    }
}