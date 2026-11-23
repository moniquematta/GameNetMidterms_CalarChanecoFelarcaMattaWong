using System.Collections; 
using UnityEngine;
using Photon.Pun;

// summary: this script is attached to the player prefab, and it controls the camera and audio listener for that player. It also handles the split screen logic for multiplayer games.
// I made it so that the split screen can be previewed in edit mode, and it will automatically apply the split screen settings from the PlayerSpawner script when the game is running.
// the setup is that the PlayerSpawner script controls the split screen settings for the entire scene.
// the SplitScreenCamera script is attached to the player prefab, and it controls the camera and audio listener for that player. It also handles the split screen logic for multiplayer games.

// === NEW SHIT === runs in edit mode too, so you can preview the split in the Game view
[ExecuteAlways]
public class SplitScreenCamera : MonoBehaviourPun
{
    [SerializeField] private Camera cam;                 
    [SerializeField] private AudioListener listener;

    // this one is for the PlayerSpawner to set the split for all cameras in the scene
    // the entire scene's split is controlled by the PlayerSpawner, so that it can be changed in edit mode and applied to all cameras

    // this one is for the PlayerSpawner to set the split for all cameras in the scene
    [Header("Editor Preview")]
    [Tooltip("Which half to preview in edit mode. 0 = auto (uses '2' in the object name, else Player 1).")]
    [SerializeField] private int previewPlayerNumber = 0;

    // for the PlayerSpawner to set the split for all cameras in the scene
    public static bool VerticalSplit = false;

    // this one is for the PlayerSpawner to call, so it can set the split for all cameras in the scene
    public static Rect GetSplitRect(int actorNumber)
    {
        return GetSplitRect(actorNumber, VerticalSplit);
    }

    public static Rect GetSplitRect(int actorNumber, bool vertical)
    {
        if (vertical)
            return actorNumber == 1 ? new Rect(0f, 0f, 0.5f, 1f)     // Player 1: left half
                                    : new Rect(0.5f, 0f, 0.5f, 1f);  // Player 2: right half

        return actorNumber == 1 ? new Rect(0f, 0.5f, 1f, 0.5f)       // Player 1: top half
                                : new Rect(0f, 0f, 1f, 0.5f);        // Player 2: bottom half
    }

    // this helps with the split screen logic, so that the camera and audio listener are only active for the local player
    IEnumerator Start()
    {
        if (!Application.isPlaying)
        {
            Apply();
            yield break;
        }

        yield return null; // wait a frame so Photon has flagged spawned players

        if (photonView.InstantiationId == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        Apply();
    }

    // for developing purposes, so you can preview the split screen in edit mode without having to run the game
    public void RefreshPreview()
    {
        Apply();
    }

#if UNITY_EDITOR
    // again, this is for developing purposes, so you can preview the split screen in edit mode without having to run the game
    void OnValidate()
    {
        if (Application.isPlaying) return;
        UnityEditor.EditorApplication.delayCall += () => { if (this != null) Apply(); };
    }
#endif

    void Apply()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>(true);
        if (cam == null)
        {
            Debug.LogWarning(name + ": no Camera found on this player prefab. Did you Apply to Prefab?");
            return;
        }

        if (listener == null) listener = cam.GetComponent<AudioListener>();

        // apply the split screen settings from the PlayerSpawner script, if it exists. Otherwise, use the default values.
        var spawner = FindFirstObjectByType<PlayerSpawner>();
        bool vertical = spawner != null ? spawner.verticalSplit : VerticalSplit;
        float zoom = spawner != null ? spawner.zoomSize : 0f;
        bool overrideH = spawner != null && spawner.overrideHeight;
        float offset = spawner != null ? spawner.heightOffset : 0f;

        // the bool, its for whether the game is running or not, so we can handle the camera and audio listener differently in edit mode vs play mode
        bool playing = Application.isPlaying;
        int actor;

        if (playing)
        {
            cam.gameObject.SetActive(true);
            cam.enabled = true;
            actor = photonView.Owner != null ? photonView.Owner.ActorNumber : 1;
        }
        else
        {
            // the funny actor thing, 
            // if the previewPlayerNumber is set to 0, it will use the name of the object to determine which half of the screen to show.
            actor = previewPlayerNumber > 0 ? previewPlayerNumber
                  : (name.Contains("2") ? 2 : 1);
        }

        // this one gets the split rect for the camera based on the actor number and whether the split is vertical or horizontal
        cam.rect = GetSplitRect(actor, vertical);

        // if the camera is orthographic and the zoom is greater than 0, set the orthographic size to the zoom value
        if (cam.orthographic && zoom > 0f && !Mathf.Approximately(cam.orthographicSize, zoom))
            cam.orthographicSize = zoom;

        // if the override height is true, set the camera's position to the player's position plus the height offset. This is for top-down games where you want the camera to be above the player.
        if (overrideH)
        {
            Vector3 p = cam.transform.position;
            Vector3 target = new Vector3(transform.position.x, transform.position.y + offset, p.z);
            if (p != target) cam.transform.position = target;
        }

        //this one is for the audio listener, so that only the local player's audio listener is active. This is important for multiplayer games, so that you don't have multiple audio listeners in the scene.
        if (playing && listener != null)
            listener.enabled = photonView.IsMine;
    }
}