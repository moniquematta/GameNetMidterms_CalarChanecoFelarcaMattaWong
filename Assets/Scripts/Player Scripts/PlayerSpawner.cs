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

    // this one starts the split screen routine after spawning a player, so that the split is applied to all cameras in the scene
    [Header("Split Screen")]
    [Tooltip("Unchecked = top/bottom split. Checked = left/right (vertical) split.")]
    public bool verticalSplit = false;


    // this one starts the split screen routine after spawning a player, so that the split is applied to all cameras in the scene
    [Header("Camera Framing (per level)")]
    public float zoomSize = 5f;          
    public bool overrideHeight = true;   
    public float heightOffset = 3.5f;    


    // this one starts the split screen routine after spawning a player, so that the split is applied to all cameras in the scene
    void Awake()
    {
        SplitScreenCamera.VerticalSplit = verticalSplit;
    }

#if UNITY_EDITOR

    // this is for developing purposes cause i want to see the split screen in the editor without having to run the game
    // Let me have this, okay PLEAAAAAAAAAAASE
    void OnValidate()
    {
        SplitScreenCamera.VerticalSplit = verticalSplit;

        if (Application.isPlaying) return;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            foreach (var c in FindObjectsByType<SplitScreenCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                c.RefreshPreview();
        };
    }
#endif

    //this is for the owner of the server (player who created the game)
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
        yield return new WaitForSeconds(0.2f); // wait a frame or two
        SpawnPlayer();
        Debug.Log("Spawned player: " + playerId);
    }

    //this is accessed by players who joined the server
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room, spawning player...");
        SpawnPlayer();
    }


    // when another player joins, re-apply the split once their player has spawned
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        StartCoroutine(SplitScreenRoutine());
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

        /*// Prevent double-spawning if the player already exists
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log("Player already spawned, skipping.");
            return;
        }*/

        playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("playerID: " + playerId);

        Transform spawnLocation; //this gets the position of location transform

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

        // The prefab MUST be in a folder called "Resources"
        // Instantiate networked player
        GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab[playerId - 1].name, spawnLocation.position, spawnLocation.rotation);

        // Store a reference so Photon knows this player exists
        PhotonNetwork.LocalPlayer.TagObject = newPlayer;

        Debug.Log("Spawned player " + playerId + " at " + spawnLocation.name);

    
        StartCoroutine(SplitScreenRoutine());
    }


    // waits briefly so remote players have time to spawn, then applies the split
    IEnumerator SplitScreenRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        ApplySplitScreen();
    }


    // This decides which half of the screen each player sees, and applies it to all cameras in the scene.
    void ApplySplitScreen()
    {
        PhotonView[] views = FindObjectsByType<PhotonView>(FindObjectsSortMode.None);

        foreach (PhotonView view in views)
        {
            if (view.Owner == null) continue;

            Camera cam = view.GetComponentInChildren<Camera>(true);
            if (cam == null) continue;

            cam.gameObject.SetActive(true);
            cam.enabled = true;

            // Set the camera's viewport rect based on the player's actor number
            cam.rect = SplitScreenCamera.GetSplitRect(view.Owner.ActorNumber);
        }
    }
}