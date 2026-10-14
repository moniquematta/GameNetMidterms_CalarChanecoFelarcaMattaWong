using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static bool allowMovement = false; //this identifies of playes can move

    [Header("UI")]
    public GameObject gameOverScreen; //this is referenced to the game over screen object

    void Awake()
    {
        // Make sure this applies before joining any room
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allowMovement = false; //locked by default
        CheckPlayers(); //this calls the checkplayes function
    }

    void Update()
    {
        // Toggle movement with backslash key for testing purposes
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            allowMovement = !allowMovement;
            Debug.Log($"Movement toggled: {allowMovement}");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckPlayers();
    }

    //this checks if the required no of players is met
    void CheckPlayers()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) //change player count depending on the required no of players
        {
            allowMovement = true;
            Debug.Log("Two players in room → Movement enabled!");
        }
        else
        {
            allowMovement = false;
            Debug.Log("Waiting for 2 players → Movement disabled.");
        }

    }
}
