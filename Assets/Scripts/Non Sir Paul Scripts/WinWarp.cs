using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinWarp : MonoBehaviour
{
    GameObject[] players;
    bool allPlayersInZone = false;

    [SerializeField] private string sceneName;

    void Start()
    {
       players = GameObject.FindGameObjectsWithTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if all players are in the trigger zone, then load the next level
        allPlayersInZone = true;
        foreach (GameObject player in players)
        {
            if (!player.GetComponent<Collider2D>().IsTouching(GetComponent<Collider2D>()))
            {
                allPlayersInZone = false;
                break;
            }
        }

        if (allPlayersInZone)
        {
            Debug.Log("All players are in the trigger zone! Loading next level...");
            SceneManager.LoadScene(sceneName);
        }
    }
}
