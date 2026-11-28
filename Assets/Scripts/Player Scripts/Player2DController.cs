﻿﻿﻿﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player2DController : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 7f;

    private Rigidbody2D rb;
    public SpriteRenderer Renderer;
    public bool isGrounded = false;
    private bool isDashing = false;
    private float horizontal;
    private bool isFacingRight = true;

    [SerializeField] private MonoBehaviour abilityScript;
    // Reference to the ability script (DwayneAbility or EmmaAbility)
    // pls pls PLS make sure to assign only these scripts ^

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

    // basically the same as update but for physics stuff, like movement
    void FixedUpdate()
    {
        if (!photonView.IsMine || !GameManager.allowMovement) return;

        if (!isDashing && !IsClimbing()) // Prevent movement during dash or climb
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    void HandleMovement()
    {
        bool isClimbing = IsClimbing();

        // Read input (the shiny new one that doesnt fuck up our shit)
        if (!isDashing && !isClimbing)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }
        else if (isClimbing)
        {
            horizontal = 0f;
        }

        // Flip sprite based on direction (from PlayerMovement)
        Flip();

        // Jump (took this from sir cause I dont wanna add ground point)
        if (Input.GetButtonDown("Jump") && isGrounded && !isClimbing)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }

        // Dash ability (for Emma) -- Shift key
        if (gameObject.name == "Player1(Clone)" && Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartCoroutine(Dash());
        }
        // Climb ability (for Dwayne) -- F key
        if (gameObject.name == "Player2(Clone)" && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(Climb());
        }
    }

    private bool IsClimbing()
    {
        return gameObject.name == "Player2(Clone)" &&
            abilityScript.GetComponent<DwayneAbility>().IsClimbActive;
    }

    IEnumerator Climb()
    {
        DwayneAbility dwayneAbility = abilityScript.GetComponent<DwayneAbility>();
        dwayneAbility.ActivateClimb();
        yield return new WaitUntil(() => !dwayneAbility.IsClimbActive);
    }

    IEnumerator Dash()
    {
        isDashing = true;
        Renderer.color = Color.orange;
        abilityScript.GetComponent<EmmaAbility>().ActivateDash();

        yield return new WaitForSeconds(0.2f); // Replace with actual dash duration
        isDashing = false;
        Renderer.color = Color.white;
    }

    private void Flip() //flips the fucker
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

    //  Photon built-in sync method cause I aint touching that
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Local player will send data
        {
            stream.SendNext(transform.position);
            stream.SendNext(isFacingRight);
        }
        else // makes sure that the remote players receive the data
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkFacingRight = (bool)stream.ReceiveNext();
        }
    }
}