using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    public static MGameManager instance;
    public enum GamePlayManagement { SPAWN_LOCATIONS, CROWD_TURN, SOLVING_TASK, END }
    public GamePlayManagement gamePlayManagement; 

    [Header("Minimap Management")]
    // public List<GameObject> markers = new();
    public List<GameObject> trackables = new();
    public GameObject trackableGo;
    // public Transform trackablesParent;

    [Header("Crowd Player Management")]
    public List<CrowdPlayerManager> allCrowdPlayers = new(); 
    private readonly Dictionary<CrowdPlayerManager, Transform> chosenLocations = new();
    [SerializeField] private List<DictionaryEntry<CrowdPlayerManager, Transform>> ChosenLocations = new();
    public Dictionary<CrowdPlayerManager, GameObject> playerShapeUI = new();
    public List<DictionaryEntry<CrowdPlayerManager, GameObject>> PlayerShapeUI = new();

    [Header("NPC Management")]
    public Transform walkableArea;
    public List<NPCManager> allNPCs = new();

    [Header("Round Management")]
    public List<Transform> taskLocations = new();
    public List<Transform> taskLocationsDone = new();
    // public List<GameObject> taskToBeDone = new();
    // public List<GameObject> taskThatAreDone = new();
    public int amountOfTasksPerRound;
    [SerializeField] private float spawnInTimer;
    // public float chooseLocationTimer;
    public bool showLocationCards = false;
    // public bool allPlayersAtLocation = false;
    public bool spawnLocations;

    public bool stateChange;


    //---------------------
    public bool lionPlacedObject;
    public TaskLocation currInterLoc;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(transform.parent.gameObject); 
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
        switch (gamePlayManagement)
        {
            case GamePlayManagement.SPAWN_LOCATIONS:
                stateChange = false;
                if(!spawnLocations) 
                {
                    StartCoroutine(StartRound(spawnInTimer));
                    spawnLocations = true;
                }

            break;

            case GamePlayManagement.CROWD_TURN:
                spawnLocations = false;
                
                if(!stateChange) 
                {
                    for (int i = 0; i < allCrowdPlayers.Count; i++)
                    {
                        allCrowdPlayers[i].playerState = CrowdPlayerManager.PlayerState.CHOOSE_LOCATION;
                    }

                    stateChange = true;
                } 

            break;

            case GamePlayManagement.SOLVING_TASK:
                stateChange = false;
                // Method that allows each fixable location to be interactable with the lion
                
                // If lion placed object:
                if(lionPlacedObject) 
                {
                    // Temp code to fetch the selected task as current location
                    foreach (var e in chosenLocations)
                    {
                        Transform chosenLocation = e.Value.transform;
                        TaskLocation taskLocation = chosenLocation.GetComponent<TaskLocation>();
                        currInterLoc = taskLocation;
                    }
                    // It should be fetched through a collision code

                    // Turn the location where the lion interacted with to location fixed
                    currInterLoc.locationFixed = true;
                    currInterLoc.fixable = false;
                    
                    // then turn state to end state
                    gamePlayManagement = GamePlayManagement.END;
                }

            break;

            case GamePlayManagement.END:
                if(!stateChange) 
                {
                    stateChange = true;
                    ResetState();
                    StartCoroutine(TempMethod());
                }

                lionPlacedObject = false;
            break;
        }
    }

    private IEnumerator TempMethod() 
    {
        yield return new WaitForSeconds(3f);
        gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;
        yield break;
    }

    private void ResetState()
    {
        lionPlacedObject = false;

        // Set player state to end state
        for (int i = 0; i < allCrowdPlayers.Count; i++)
        {
            var player = allCrowdPlayers[i];
            player.playerState = CrowdPlayerManager.PlayerState.END;
        }      

        // Update the task locations list
        bool containsName = false;
        foreach(Transform location in taskLocationsDone)
        {
            // Debug.Log($"Location name -> {location.gameObject.name} is the same as the task location name -> {currentInteractableLocation.gameObject.name}.");
            if(location != null && location.gameObject.name == currInterLoc.gameObject.name)
            {
                containsName = true;
                break;
            }
        }

        if(!containsName)
        {
            taskLocationsDone.Add(currInterLoc.transform);
            taskLocations.Remove(currInterLoc.transform);
        }

        chosenLocations.Clear();
        ChosenLocations.Clear();

        // Clear the trackables on the locations
        // foreach (var item in taskLocations)
        // {
        //     GameObject child = item.GetChild(0).gameObject;
        //     Destroy(child);
        // }

        foreach (var trackable in trackables)
        {
            Destroy(trackable);
        }

        trackables.Clear();

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

    private IEnumerator StartRound(float spawnInTimer) 
    {
        yield return new WaitForSeconds(spawnInTimer);

        // Spawn in the locations
        SpawnTaskLocations();

        // Show UI something like round start
        Debug.Log("round started");


        // Wait a few seconds
        yield return new WaitForSeconds(5f);

        // Spawn in the UI for the players
        showLocationCards = true;

        gamePlayManagement = GamePlayManagement.CROWD_TURN;

        yield break;
    }

    private void SpawnTaskLocations() 
    {
        if (taskLocations == null || taskLocations.Count == 0) 
        {
            Debug.LogError("locations array is null or empty!");
            return;
        }

        // Shuffle the list to get random locations
        List<Transform> shuffledLocations = taskLocations.OrderBy(x => Random.value).ToList();
        
        int spawnCount = Mathf.Min(amountOfTasksPerRound, shuffledLocations.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Transform randomLocation = shuffledLocations[i]; // Pick a random location
            // Debug.Log($"Spawning at: {randomLocation.localPosition}");

            GameObject obj = InstantiatePrefab(trackableGo, randomLocation.position, trackableGo.transform.rotation, randomLocation.transform);

            if (obj.TryGetComponent<ObjectToTrack>(out var objScript))
            {
                objScript.InitializePosition(randomLocation); // Pass location for bounds check
            }
        }
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