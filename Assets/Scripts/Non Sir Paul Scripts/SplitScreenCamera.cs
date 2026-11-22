using UnityEngine;
using Photon.Pun;

// Attach this to the ROOT of BOTH player prefabs (Player1 and Player2).
// It keeps every player's camera enabled (so both halves render on every client),
// sets the correct half of the screen, and makes sure only the LOCAL player's
public class SplitScreenCamera : MonoBehaviourPun
{
    [SerializeField] private Camera cam;                 
    [SerializeField] private AudioListener listener;     

    void Start()
    {
        Apply();
    }

    void Apply()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>(true);
        if (cam == null)
        {
            Debug.LogWarning(name + ": !! NO PLAYER CAMERA FOUND, IS IT THERE???????????? !!");
            return;
        }

        if (listener == null) listener = cam.GetComponent<AudioListener>();

        cam.gameObject.SetActive(true);
        cam.enabled = true;

        int actor = photonView.Owner != null ? photonView.Owner.ActorNumber : 1;

        if (actor == 1)
            cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // top half
        else
            cam.rect = new Rect(0f, 0f, 1f, 0.5f);   // bottom half

        // only the local player hears audio
        if (listener != null)
            listener.enabled = photonView.IsMine;
    }
}