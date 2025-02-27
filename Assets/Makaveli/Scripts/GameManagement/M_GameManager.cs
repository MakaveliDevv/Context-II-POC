using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    public enum GamePlayManagement { NOTHING, SPAWN_OBSTACLE, DISCUSS_LOCATION, DISCUSS_SHAPE, WAITING };
    public static MGameManager instance;
    
    [Header("States")]
    public GamePlayManagement gamePlayManagement; 

    [Header("Obstacle Location Trackers")]
    public List<GameObject> markers = new();
    public GameObject trackableObject;
    public Transform trackableObjectParent;
    [HideInInspector] public List<GameObject> objectsToTrack = new();
    private bool obstaclesSpawned = false;

    // NPC related stuff
    [Header("NPC Related Stuff")]
    // public GameObject patrolArea;
    public Transform walkableArea;
    public int trackableObjectAmount;
    [HideInInspector] public List<NPCManager> allNPCs = new();

    [SerializeField] private float discussionTimer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.parent.gameObject); 
        }
        else
        {
            Destroy(transform.parent.gameObject); 
        }
    }

    void Start()
    {
        gamePlayManagement = GamePlayManagement.NOTHING;

       
    }

    private bool discussionStarted = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            SpawnObstacleLocation();
        }
        
        // if(obstaclesSpawned && !discussionStarted)
        // {
        //     discussionStarted = true;
        //     StartCoroutine(StartDiscussingLocation(discussionTimer));
        // }
    }

    public GameObject InstantiatePrefab(GameObject prefab, Transform parent) 
    {
        GameObject newGameObject = Instantiate(prefab);
        newGameObject.transform.SetParent(parent, true);

        return newGameObject;
    }

    private void SpawnObstacleLocation() 
    {
        gamePlayManagement = GamePlayManagement.SPAWN_OBSTACLE;
        for (int i = 0; i < trackableObjectAmount; i++)
        {
            InstantiatePrefab(trackableObject, trackableObjectParent);
        }

        obstaclesSpawned = true;

        float timer = 2f;
        timer -= Time.deltaTime;

        if(timer <= -0) 
        {
            obstaclesSpawned = false;
        }
    }

    private IEnumerator StartDiscussingLocation(float discussionTime)
    {
        // Your existing coroutine code
        Debug.Log("Objects spawned. start choosing a location");
        yield return new WaitForSeconds(2f);
        gamePlayManagement = GamePlayManagement.DISCUSS_LOCATION;
        yield return new WaitForSeconds(discussionTime);
        Debug.Log("Timer ran out");
        
        // Reset game state after discussion ends
        gamePlayManagement = GamePlayManagement.NOTHING; // Or whatever state should come next
        obstaclesSpawned = false;
        discussionStarted = false;
    }
}
        // Start timer for discussion

        // If timer ran out before choosing a location, a random location is chosen and store it as 'chosen location'

        // If player did choose a location, then store it as 'chosen location'

        // Fetch the 'chosen location' from each player and add it to the list of 'AllChosenLocations'

        // Check which location appears the most

        // Store that location

public interface ITriggerMovement 
{
    public void TriggerMovement(Transform transform);
}