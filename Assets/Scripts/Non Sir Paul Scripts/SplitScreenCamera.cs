// === NEW SHIT === (this whole script is new)
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Attach this to the ROOT of BOTH player prefabs (Player1 and Player2).
// It keeps every player's camera enabled (so both halves render on every client),
// sets the correct half of the screen, and makes sure only the LOCAL player's
// AudioListener is active (Unity complains if there are two).
public class SplitScreenCamera : MonoBehaviourPun
{
    [SerializeField] private Camera cam;                 
    [SerializeField] private AudioListener listener;     

    // === NEW SHIT ===
    [Header("Zoom / Height (set the SAME values on both Player prefabs)")]
    [SerializeField] private float zoomSize = 0f;            // camera Size (bigger = more zoomed out). 0 = leave the camera's own size alone
    [SerializeField] private bool overrideHeight = false;    
    [SerializeField] private float heightOffset = 0f;        

    // === NEW SHIT ===
    // Level 2 only: vertical split (left/right). Every other level: top/bottom.
    // Both this script and PlayerSpawner call this so they never disagree.
    public static Rect GetSplitRect(int actorNumber)
    {
        bool vertical = SceneManager.GetActiveScene().name == "Lvl2"; // must match the scene name exactly

        if (vertical)
            return actorNumber == 1 ? new Rect(0f, 0f, 0.5f, 1f)     // Player 1: left half
                                    : new Rect(0.5f, 0f, 0.5f, 1f);  // Player 2: right half

        return actorNumber == 1 ? new Rect(0f, 0.5f, 1f, 0.5f)       // Player 1: top half
                                : new Rect(0f, 0f, 1f, 0.5f);        // Player 2: bottom half
    }

    void Start()
    {
        Apply();
    }

    void Apply()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>(true);
        if (cam == null)
        {
            Debug.LogWarning(name + ": no Camera found on this player prefab. Did you Apply to Prefab?");
            return;
        }

        if (listener == null) listener = cam.GetComponent<AudioListener>();

        cam.gameObject.SetActive(true);
        cam.enabled = true;

        int actor = photonView.Owner != null ? photonView.Owner.ActorNumber : 1;

        // === NEW SHIT ===
        // set the camera's viewport rect to the correct half of the screen
        cam.rect = GetSplitRect(actor);

        // === NEW SHIT ===
        // if zoomSize is set to a positive value, override the camera's own size
        if (cam.orthographic && zoomSize > 0f)
            cam.orthographicSize = zoomSize;

        // === NEW SHIT ===
        // if overrideHeight is true, move the camera up by heightOffset world units
        // this is useful for making both cameras look at the same height, even if the players are on different terrain heights
        if (overrideHeight)
        {
            Vector3 p = cam.transform.position;
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + heightOffset, p.z);
        }

        // only the local player hears audio
        if (listener != null)
            listener.enabled = photonView.IsMine;
    }
}