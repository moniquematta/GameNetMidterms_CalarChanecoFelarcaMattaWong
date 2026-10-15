using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinWarp : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private Collider2D warpCollider;
    // Static so multiple WinWarp objects cannot request the same transition.
    private static bool sceneLoadStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneChangeCallback()
    {
        // Remove first in case domain reload is disabled in the Unity Editor.
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private static void OnActiveSceneChanged(Scene previousScene, Scene nextScene)
    {
        // Allow the next level's win warp to start a new transition.
        sceneLoadStarted = false;
    }

    private void Awake()
    {
        warpCollider = GetComponent<Collider2D>();

        if (warpCollider == null)
        {
            Debug.LogError("WinWarp needs a Collider2D component.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the Master Client decides when the room changes scenes.
        if (sceneLoadStarted || !PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (warpCollider == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("WinWarp has no destination scene name assigned.", this);
            return;
        }

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Wait until every player in the room has a spawned Player object.
        if (players.Length < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("Waiting for all room players to spawn before loading the next scene.");
            return;
        }

        // Every spawned player must be inside this warp.
        foreach (GameObject player in players)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();

            if (playerCollider == null || !playerCollider.IsTouching(warpCollider))
            {
                return;
            }
        }

        // Set the shared guard before requesting the scene transition.
        sceneLoadStarted = true;
        Debug.Log($"All players are in the win warp. Loading {sceneName}...");
        PhotonNetwork.LoadLevel(sceneName);
    }
}
