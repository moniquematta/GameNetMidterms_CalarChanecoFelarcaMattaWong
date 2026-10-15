using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    public GameObject[] playerPrefab; // Prefabs must be in a Resources folder.
    private int playerId;
    private bool spawnAttempted;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    // Run before the first scene loads so scene synchronization is enabled
    // before the client joins a Photon room.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnablePhotonSceneSynchronization()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            StartCoroutine(DelaySpawn());
        }
        else
        {
            Debug.LogWarning("Not connected to Photon or not in a room yet.");
        }
    }

    private IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(0.2f); // Wait a frame or two.
        SpawnPlayer();
        Debug.Log("Spawned player: " + playerId);
    }

    // Called when this client joins a room.
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room, spawning player...");
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        // Start() and OnJoinedRoom() can both call this method.
        if (spawnAttempted)
        {
            Debug.Log("Spawn already attempted by this spawner; skipping.");
            return;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            Debug.LogWarning("Cannot spawn: the local player is not in a Photon room.");
            return;
        }

        // Also guard against multiple PlayerSpawner objects trying to spawn
        // a player that already exists for this client.
        GameObject existingPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        if (existingPlayer != null)
        {
            Debug.Log("Local player already exists, skipping spawn.");
            spawnAttempted = true;
            return;
        }

        if (playerPrefab == null || playerPrefab.Length == 0)
        {
            Debug.LogError("Player prefabs are missing in the Inspector!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("playerID: " + playerId);

        // This setup expects one prefab entry per actor number.
        if (playerId < 1 || playerId > playerPrefab.Length)
        {
            Debug.LogError($"No player prefab assigned for actor number {playerId}. " +
                           $"The playerPrefab array has {playerPrefab.Length} entries.");
            return;
        }
    }
}
