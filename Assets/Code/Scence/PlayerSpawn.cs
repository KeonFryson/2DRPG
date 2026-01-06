using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    private GameObject player;
    [SerializeField]bool isGameStart = false;
    [SerializeField]GameObject playerSpawnPoint;


    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");

        if (isGameStart)
        {
            isGameStart = false;
            Debug.Log("Game Start - Skip Player Spawn");
            Debug.Log("isGameStart:" + isGameStart);
            return;
        }

        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.transform.position;
        }
        else
        {
            Debug.LogError("Player not found  .");
        }
    }

    void Start()
    {
      

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
