using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    // public enum GamePlayManagement { NOTHING, TRAVELING, CHOOSE_LOCATION, CHOOSE_SHAPE, SIGNAL };
    public enum GamePlayManagement { SPAWN_LOCATIONS, CROWD_TURN, REMOVE_LOCATIONS }
    public static MGameManager instance;
    
    [Header("States")]
    public GamePlayManagement gamePlayManagement; 

    [Header("Obstacle Location Trackers")]
    public List<GameObject> markers = new();
    public GameObject trackableObject;
    public Transform trackableObjectParent;
    [HideInInspector] public List<GameObject> objectsToTrack = new();
    public List<Transform> locations = new();

    [Header("Crowd Player Stuff")]
    public List<CrowdPlayerManager> allCrowdPlayers = new(); 
    private readonly Dictionary<CrowdPlayerManager, Transform> chosenLocations = new();
    [SerializeField] private List<DictionaryEntry<CrowdPlayerManager, Transform>> ChosenLocations = new();
    public Dictionary<CrowdPlayerManager, GameObject> playerShapeUI = new();
    public List<DictionaryEntry<CrowdPlayerManager, GameObject>> PlayerShapeUI = new();
    public bool navigationUI = false;

    // NPC related stuff
    [Header("NPC Stuff")]
    // public GameObject patrolArea;
    public Transform walkableArea;
    public int trackableObjectAmount;
    public List<NPCManager> allNPCs = new();

    [Header("Round Management")]
    [SerializeField] private float spawnInTimer;
    public float chooseLocationTimer;
    public bool showLocationCards = false;
    public bool roundEnd = false;
    public bool allPlayersAtLocation = false;
    private bool spawn;

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
        gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;

        chosenLocations.Clear();
        ChosenLocations.Clear();
    }

    void Update()
    {
        // switch (gamePlayManagement)
        // {
        //     case GamePlayManagement.CHOOSE_LOCATION:
        //         StartCoroutine(InitializeLocations());

        //     break; 

        //     case GamePlayManagement.TRAVELING:
        //         StopCoroutine(InitializeLocations());

        //         int c = 0;
        //         for (int i = 0; i < allCrowdPlayers.Count; i++)
        //         {
        //             CrowdPlayerManager player = allCrowdPlayers[i].GetComponent<CrowdPlayerManager>();
        //             Transform playerTransform = player.gameObject.transform.GetChild(0);
        //             player.playerController.CheckPlayerPosition(playerTransform);

        //             if(player.playerController.isAtLocation) 
        //             {
        //                 c++;
        //             }
        //         }

        //         if(c == allCrowdPlayers.Count) 
        //         {
        //             gamePlayManagement = GamePlayManagement.CHOOSE_SHAPE;
        //             Debug.Log("All players have reached their location point");
        //         }
            
        //     break;

        //     case GamePlayManagement.CHOOSE_SHAPE:
        //         // Show UI to choose a shape
        //         StartCoroutine(DisplayShapePanel());

        //     break;

        //     case GamePlayManagement.SIGNAL:
        //         StartCoroutine(CloseShapePanel());

        //     break;

        //     default:

        //     break;
        // }

        switch (gamePlayManagement)
        {
            case GamePlayManagement.SPAWN_LOCATIONS:
                if(!spawn) 
                {
                    StartCoroutine(SpawnInLocations(spawnInTimer));
                    spawn = true;
                }

            break;

            case GamePlayManagement.CROWD_TURN:
                spawn = false;
                
                if(allCrowdPlayers.All(p => p.hasSignaled)) 
                {
                    gamePlayManagement = GamePlayManagement.REMOVE_LOCATIONS;
                }

            break;

            case GamePlayManagement.REMOVE_LOCATIONS:
                foreach (var player in allCrowdPlayers)
                {
                    player.hasSignaled = false;
                }

                StartCoroutine(ResetState());

            break;
        }
    }

    private IEnumerator ResetState() 
    {
        yield return new WaitForSeconds(2f);
        
        for (int i = 0; i < locations.Count; i++)
        {
            GameObject location = locations[i].gameObject;
            Destroy(location);    
        }

        locations.Clear();
        gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;

        yield break;
    }

    public GameObject InstantiatePrefab
    (
        GameObject original,
        Vector3 position,
        Quaternion rotation, 
        Transform parent
    ) 
    {
        GameObject newGameObject = Instantiate(original, position, rotation, parent);

        return newGameObject;
    }

    private void SpawnInLocation() 
    {
        if (locations == null || locations.Count == 0) 
        {
            Debug.LogError("locations array is null or empty!");
            return;
        }

        // Shuffle the list to get random locations
        List<Transform> shuffledLocations = locations.OrderBy(x => Random.value).ToList();
        
        int spawnCount = Mathf.Min(trackableObjectAmount, shuffledLocations.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Transform randomLocation = shuffledLocations[i]; // Pick a random location
            Debug.Log($"Spawning at: {randomLocation.localPosition}");

            GameObject obj = InstantiatePrefab(trackableObject, randomLocation.position, trackableObject.transform.rotation, null);

            if (obj.TryGetComponent<ObjectToTrack>(out var objScript))
            {
                objScript.InitializePosition(randomLocation); // Pass location for bounds check
            }
        }
    }

    private IEnumerator SpawnInLocations(float spawnInTimer) 
    {
        yield return new WaitForSeconds(spawnInTimer);

        // Spawn in the locations
        SpawnInLocation();

        gamePlayManagement = GamePlayManagement.CROWD_TURN;

        // Wait a few seconds
        yield return new WaitForSeconds(4f);

        // Spawn in the UI for the players
        showLocationCards = true;
        StartCoroutine(DisplayLocationCardUI());

        yield break;
    }

    private IEnumerator DisplayLocationCardUI() 
    {
        if(showLocationCards) 
        {
            foreach (var player in allCrowdPlayers)
            {
                player.playerState = CrowdPlayerManager.PlayerState.CHOOSE_LOCATION;    
            }
            
            yield return new WaitForSeconds(chooseLocationTimer);

            showLocationCards = false;
        }

        gamePlayManagement = GamePlayManagement.CROWD_TURN;

        yield break;
    }

    public IEnumerator InitializeLocation()
    {
        if (allCrowdPlayers == null || allCrowdPlayers.Count == 0)
        {
            Debug.Log("No players found!");
            yield break;
        }

        // Populate chosenLocations dictionary
        foreach (var player in allCrowdPlayers)
        {
            if (player.playerController.chosenLocation != null && !chosenLocations.ContainsKey(player))
            {
                chosenLocations[player] = player.playerController.chosenLocation;
            }
        }

        foreach (var element in chosenLocations)
        {
            if (!ChosenLocations.Any(e => e.Key == element.Key)) // Check by player reference
            {
                ChosenLocations.Add(new DictionaryEntry<CrowdPlayerManager, Transform>
                {
                    Key = element.Key,
                    Value = element.Value
                });
            }
        }

        yield break;
    }
}

public interface ITriggerMovement 
{
    public void TriggerMovement(Transform transform);
}

[System.Serializable]
public class DictionaryEntry<TKey, TValue> 
{
    public TKey Key;
    public TValue Value;

    public override bool Equals(object obj)
    {
        if (obj is DictionaryEntry<TKey, TValue> other)
        {
            return EqualityComparer<TKey>.Default.Equals(Key, other.Key) &&
                   EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hashKey = Key?.GetHashCode() ?? 0;
        int hashValue = Value?.GetHashCode() ?? 0;
        return hashKey ^ hashValue;
    }
}