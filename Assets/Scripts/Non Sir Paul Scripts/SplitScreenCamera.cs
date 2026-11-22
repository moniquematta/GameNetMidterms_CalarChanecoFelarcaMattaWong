using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class SplitScreenCamera : MonoBehaviourPun
{
    [SerializeField] private Camera cam;                 // optional: auto-found if left empty
    [SerializeField] private AudioListener listener;     // optional: auto-found if left empty

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
        cam.rect = GetSplitRect(actor);

        // only the local player hears audio
        if (listener != null)
            listener.enabled = photonView.IsMine;
    }
}