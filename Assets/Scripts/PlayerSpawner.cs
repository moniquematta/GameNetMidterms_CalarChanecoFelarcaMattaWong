using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    public GameObject[] playerPrefab; // must be in Resources folder
    private int playerId;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    public void Start()
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

    IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(0.2f);
        SpawnPlayer();
        Debug.Log("Spawned player: " + playerId);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room, spawning player...");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing in inspector!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("playerID: " + playerId);

        Transform spawnLocation;

        if (playerId == 1)
        {
            spawnLocation = spawnPoints[0];
        }
        else if (playerId == 2 && spawnPoints.Length > 1)
        {
            spawnLocation = spawnPoints[1];
        }
        else
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            spawnLocation = spawnPoints[randomIndex];
        }

        // Destroy old player if it exists (prevents sticky players)
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            GameObject oldPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            if (oldPlayer != null)
            {
                Destroy(oldPlayer);
            }
        }

        // Instantiate networked player
        GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab[playerId - 1].name, spawnLocation.position, spawnLocation.rotation);

        // Store reference
        PhotonNetwork.LocalPlayer.TagObject = newPlayer;

        Debug.Log("Spawned player " + playerId + " at " + spawnLocation.name);
    }
}
