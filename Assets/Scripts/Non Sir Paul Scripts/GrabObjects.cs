using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class GrabObjects : MonoBehaviourPun
{
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform rayPoint;
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private LayerMask grabbableLayers;
    [SerializeField] private string objectGrabPointName = "grabPoint";

    private GameObject grabbedObject;
    private Collider2D[] playerColliders;

    private void Awake()
    {
        playerColliders = GetComponentsInChildren<Collider2D>();
    }

    private void Update()
    {
        // Only the local player reads input. Everyone else's copy just receives the RPCs.
        if (!photonView.IsMine) return;

        Vector2 dir = transform.right;
        Debug.DrawRay(rayPoint.position, dir * rayDistance, Color.red);

        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        if (grabbedObject != null)
        {
            photonView.RPC(nameof(RPC_Release), RpcTarget.All);
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(rayPoint.position, dir, rayDistance, grabbableLayers);
        if (hit.collider == null) return;

        PhotonView boxView = hit.collider.GetComponentInParent<PhotonView>();
        if (boxView != null)
        {
            photonView.RPC(nameof(RPC_Grab), RpcTarget.All, boxView.ViewID);
        }
    }

    // Runs on EVERY client, so everyone sees the box held the same way(Avoids the schizophrenic box)
    [PunRPC]
    public void RPC_Grab(int boxViewID)
    {
        PhotonView boxView = PhotonView.Find(boxViewID);
        if (boxView == null || grabbedObject != null) return;

        GameObject obj = boxView.gameObject;
        if (obj.transform.parent != null) return; // someone is already holding it

        // The grabber becomes the owner, so they simulate the box after dropping it
        if (photonView.IsMine) boxView.RequestOwnership();

        grabbedObject = obj;

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;   
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        SetSyncEnabled(obj, false); 
        SetIgnoreCollision(obj, true);

        Transform objGrabPoint = obj.transform.Find(objectGrabPointName);
        Vector3 offset = objGrabPoint != null
            ? obj.transform.position - objGrabPoint.position
            : Vector3.zero;

        obj.transform.SetParent(grabPoint);
        obj.transform.position = grabPoint.position + offset;
    }

    // basically the same as RPC_Grab but for releasing the box, so everyone sees it drop the same way and it doesnt try to harass me anymore :(
    [PunRPC]
    public void RPC_Release()
    {
        if (grabbedObject == null) return;

        GameObject obj = grabbedObject;
        grabbedObject = null;

        obj.transform.SetParent(null);
        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        SetSyncEnabled(obj, true);

        StartCoroutine(ReenableCollisionWhenClear(obj));
    }

    private IEnumerator ReenableCollisionWhenClear(GameObject obj)
    {
        Collider2D[] boxCols = obj.GetComponents<Collider2D>();
        bool overlapping = true;

        while (overlapping)
        {
            overlapping = false;
            foreach (Collider2D b in boxCols)
                foreach (Collider2D p in playerColliders)
                    if (b.Distance(p).isOverlapped) overlapping = true;

            if (overlapping) yield return null;
        }

        // DONT BE LIKE ME AND FUCK UP, this is a hacky way to make sure the box isn't parented to something else before re-enabling collisions.
        if (obj != null && obj.transform.parent == null)
            SetIgnoreCollision(obj, false);
    }

    private void SetSyncEnabled(GameObject obj, bool enabled)
    {
        PhotonRigidbody2DView sync = obj.GetComponent<PhotonRigidbody2DView>();
        if (sync != null) sync.enabled = enabled;
    }

    // this makes it so the player and box arent making out when the box is being held
    private void SetIgnoreCollision(GameObject obj, bool ignore)
    {
        foreach (Collider2D boxCol in obj.GetComponents<Collider2D>())
            foreach (Collider2D playerCol in playerColliders)
                if (playerCol != boxCol)
                    Physics2D.IgnoreCollision(boxCol, playerCol, ignore);
    }
}