using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody2D))]
public class Player2DController : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 7f;

    private Rigidbody2D rb;
    public bool isGrounded = false;
    private float horizontal;
    private bool isFacingRight = true;

    [Header("Interaction Settings")]
    public Transform interactPoint; //this is referenced to an empty child in front of the player
    public float interactRange = 1f;
    private InteractableObject grabbedObject = null;

    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private bool networkFacingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
        networkFacingRight = isFacingRight;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine) //for the local player
        {
            if (GameManager.allowMovement)
            {
                HandleMovement();
                HandleInteraction();
            }
            else
            {
                horizontal = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else //for the remote players
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);

            if (isFacingRight != networkFacingRight)
                SetFacing(networkFacingRight);
        }
    }

    // Horizontal velocity is applied in FixedUpdate (from PlayerMovement)
    void FixedUpdate()
    {
        if (!photonView.IsMine || !GameManager.allowMovement) return;

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    void HandleMovement()
    {
        // Read input (from PlayerMovement)
        horizontal = Input.GetAxisRaw("Horizontal");

        // Flip sprite based on direction (from PlayerMovement)
        Flip();

        // Jump (original Player2DController jump, unchanged)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            SetFacing(!isFacingRight);
        }
    }

    // Applies the facing direction via localScale.x. Also used by remote players.
    private void SetFacing(bool faceRight)
    {
        isFacingRight = faceRight;

        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (faceRight ? 1f : -1f);
        transform.localScale = localScale;
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E)) //this is the key for grab or release
        {
            if (grabbedObject == null)
            {
                Collider2D hit = Physics2D.OverlapCircle(interactPoint.position, interactRange);
                if (hit != null && hit.GetComponent<InteractableObject>())
                {
                    grabbedObject = hit.GetComponent<InteractableObject>();
                    grabbedObject.photonView.RPC("RPC_SetGrabbed", RpcTarget.AllBuffered, photonView.ViewID);
                }
            }
            else
            {
                grabbedObject.photonView.RPC("RPC_Release", RpcTarget.AllBuffered);
                grabbedObject = null;
            }
        }

        if (grabbedObject != null)
        {
            grabbedObject.transform.position = Vector3.Lerp(
                grabbedObject.transform.position,
                interactPoint.position,
                Time.deltaTime * 10f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //simple ground check
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    // 🔹 Photon built-in sync method
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Local player → send data
        {
            stream.SendNext(transform.position);
            stream.SendNext(isFacingRight);
        }
        else // Remote player → receive data
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkFacingRight = (bool)stream.ReceiveNext();
        }
    }
}