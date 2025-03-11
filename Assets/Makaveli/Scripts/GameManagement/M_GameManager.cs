using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    // public enum GamePlayManagement { NOTHING, TRAVELING, CHOOSE_LOCATION, CHOOSE_SHAPE, SIGNAL };
    public enum GamePlayManagement { SPAWN_LOCATIONS, PLAYER_TURN, REMOVE_LOCATIONS }
    public static MGameManager instance;
    
    [Header("States")]
    public GamePlayManagement gamePlayManagement; 

    [Header("Obstacle Location Trackers")]
    public List<GameObject> markers = new();
    public GameObject trackableObject;
    public Transform trackableObjectParent;
    [HideInInspector] public List<GameObject> objectsToTrack = new();

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
                StartCoroutine(SpawnInLocations(spawnInTimer));

            break;

            case GamePlayManagement.PLAYER_TURN:

            break;

            case GamePlayManagement.REMOVE_LOCATIONS:

            break;
        }
    }

    // private IEnumerator CloseShapePanel() 
    // {
    //     Debug.Log("DisplayShapePanel Coroutine Running");
    //     yield return new WaitForSeconds(1f);
        
    //     // Show the UI for each player independent
    //     foreach (var player in playerShapeUI)
    //     {
    //         player.Key.playerController.CloseShapePanel();
    //         player.Key.inUIMode = false;   
            
    //         break;
    //     }

    //     yield break;
    // }

    // private IEnumerator DisplayShapePanel() 
    // {
    //     Debug.Log("DisplayShapePanel Coroutine Running");
    //     yield return new WaitForSeconds(1f);
        
    //     // Show the UI for each player independent
    //     foreach (var player in playerShapeUI)
    //     {
    //         player.Key.playerController.OpenShapePanel();
    //         player.Key.inUIMode = true;   
            
    //         break;
    //     }

    //     yield break;
    // }

    public GameObject InstantiatePrefab(GameObject prefab, Transform parent) 
    {
        GameObject newGameObject = Instantiate(prefab);
        newGameObject.transform.SetParent(parent, true);

        return newGameObject;
    }

    private void SpawnInLocation() 
    {
        for (int i = 0; i < trackableObjectAmount; i++)
        {
            InstantiatePrefab(trackableObject, trackableObjectParent);
        }
    }

    private IEnumerator SpawnInLocations(float spawnInTimer) 
    {
        yield return new WaitForSeconds(spawnInTimer);

        // Spawn in the locations
        SpawnInLocation();

        gamePlayManagement = GamePlayManagement.PLAYER_TURN;

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

        // Convert dictionary to list entries and avoid duplicates
        foreach (var element in chosenLocations)
        {
            var entry = new DictionaryEntry<CrowdPlayerManager, Transform> 
            {
                Key = element.Key,
                Value = element.Value
            };

            if (!ChosenLocations.Contains(entry)) 
            {
                ChosenLocations.Add(entry);
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