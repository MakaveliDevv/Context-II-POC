using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class MGameManager : NetworkBehaviour
{
    public static MGameManager instance;
    public enum GamePlayManagement { START, SPAWN_LOCATIONS, CROWD_TURN, SOLVING_TASK, END }
    public GamePlayManagement gamePlayManagement; 

    [Header("Prefabs")]
    public GameObject trackableGo;
    public GameObject taskCard;

    [Header("Minimap Management")]
    // public List<GameObject> markers = new();
    public List<GameObject> trackables = new();
    // public Transform trackablesParent;

    [Header("Crowd Player Management")]
    public List<CrowdPlayerManager> allCrowdPlayers = new(); 
    public Dictionary<CrowdPlayerManager, Transform> chosenLocations = new();
    [SerializeField] public List<DictionaryEntry<CrowdPlayerManager, Transform>> ChosenLocations = new();
    public Dictionary<CrowdPlayerManager, GameObject> playerShapeUI = new();
    public List<DictionaryEntry<CrowdPlayerManager, GameObject>> PlayerShapeUI = new();
    public List<Transform> playersSpawnPositions = new();

    [Header("NPC Management")]
    public Transform walkableArea;
    public List<NPCManager> allNPCs = new();

    [Header("Round Management")]
    public List<Transform> taskLocations = new();
    private readonly List<Transform> taskLocationsDone = new();
    public List<Task> possibleTasks = new();
    public List<Task> tasksPerRound = new();
    public List<Task> completeTasks = new();
    public int amountOfTasksPerRound;
    public bool taskComplete;
    public bool showLocationCards = false;
    public bool spawnLocations;
    public bool stateChange;
    public float spawnInTimer;

    // Lion stuff
    public bool lionPlacedObject;
    public TaskLocation currInterLoc;
    public TaskLocation currentInteractableLocation;
    public Lion lion;
    GameManagerRpcBehaviour gameManagerRpcBehaviour;

    [Header("Point System Management")]
    public float currentPoint = 0;
    public float maxPoints;

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
        gameManagerRpcBehaviour = FindFirstObjectByType<GameManagerRpcBehaviour>();
    }

    void Start()
    {
        gamePlayManagement = GamePlayManagement.START;
        // lion = GameObject.FindGameObjectWithTag("Lion").GetComponent<Lion>();

        chosenLocations.Clear();
        ChosenLocations.Clear();
    }

    void Update()
    {
        switch (gamePlayManagement)
        {
            case GamePlayManagement.START:
                gameManagerRpcBehaviour.GameStateManagement("START");
            break;

            case GamePlayManagement.SPAWN_LOCATIONS:
                gameManagerRpcBehaviour.GameStateManagement("SPAWN_LOCATIONS");
            break;

            case GamePlayManagement.CROWD_TURN:
                gameManagerRpcBehaviour.GameStateManagement("CROWD_TURN");
            break;

            case GamePlayManagement.SOLVING_TASK:
                gameManagerRpcBehaviour.GameStateManagement("SOLVING_TASK");
            break;

            case GamePlayManagement.END:
                gameManagerRpcBehaviour.GameStateManagement("END");
            break;
        }
    }

    public void StartState()
    {
        if(allCrowdPlayers.Count > 0) 
        {
            gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;
        }
    }

    public void SpawnLocationsState()
    {
        stateChange = false;
        if(!spawnLocations) 
        {
            StartCoroutine(StartRound(spawnInTimer));
            spawnLocations = true;
        }
    }

    public void CrowdTurnState()
    {
        spawnLocations = false;
                
        if(!stateChange) 
        {
            for (int i = 0; i < allCrowdPlayers.Count; i++)
            {
                allCrowdPlayers[i].playerState = CrowdPlayerManager.PlayerState.CHOOSE_LOCATION;
            }

            stateChange = true;
        } 
    }

    public void SolvingTaskState()
    {
        stateChange = false;
                
        if(taskComplete) 
        {
            // Temp code to fetch the selected task as current location
            foreach (var e in chosenLocations)
            {
                Transform chosenLocation = e.Value.transform;
                TaskLocation taskLocation = chosenLocation.GetComponent<TaskLocation>();
                currentInteractableLocation = taskLocation;
            }
        
            // Turn the location where the lion interacted with to location fixed
            currentInteractableLocation.locationFixed = true;
            currentInteractableLocation.fixable = false;

            // Iterate through the possibleTasksList
            foreach (var task in possibleTasks)
            {
                // If the object the lion placed has the same task as one of th task on the location
                if(lion.lastObjectTask.taskName == task.taskName) 
                {
                    completeTasks.Add(task);
                }
            }
            
            StartCoroutine(DisplayEndRound(lion));
        }
    }

    private IEnumerator DisplayEndRound(Lion lion) 
    {
        Debug.Log("End Round Started...");

        if(lion.correctTask) 
        {
            Debug.Log("Correct task objet");
        }
        else 
        {
            Debug.Log("Incorrect task objet");
        }

        yield return new WaitForSeconds(5f);

        // then turn state to end state
        gamePlayManagement = GamePlayManagement.END;
    }

    public void EndState()
    {
        if(!stateChange) 
        {
            stateChange = true;
            StartCoroutine(ResetState());
            // StartCoroutine(TempMethod());
        }

        lionPlacedObject = false;
    }

    // [ServerRpc(RequireOwnership = false)]
    // void GameStateManagementServerRpc(GamePlayManagement _state)
    // {
    //     GameStateManagementClientRpc(_state);
    // }
    
    // [ClientRpc]
    // void GameStateManagementClientRpc(GamePlayManagement _state)
    // {
    //     switch (_state)
    //     {
    //         case GamePlayManagement.START:
    //            if(allCrowdPlayers.Count > 0) 
    //            {
    //                 gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;
    //            }

    //         break;

    //         case GamePlayManagement.SPAWN_LOCATIONS:
    //             stateChange = false;
    //             if(!spawnLocations) 
    //             {
    //                 StartCoroutine(StartRound(spawnInTimer));
    //                 spawnLocations = true;
    //             }

    //         break;

    //         case GamePlayManagement.CROWD_TURN:
    //             spawnLocations = false;
                
    //             if(!stateChange) 
    //             {
    //                 for (int i = 0; i < allCrowdPlayers.Count; i++)
    //                 {
    //                     allCrowdPlayers[i].playerState = CrowdPlayerManager.PlayerState.CHOOSE_LOCATION;
    //                 }

    //                 stateChange = true;
    //             } 

    //         break;

    //         case GamePlayManagement.SOLVING_TASK:
    //             stateChange = false;
                
    //             if(taskComplete) 
    //             {
    //                 // Temp code to fetch the selected task as current location
    //                 foreach (var e in chosenLocations)
    //                 {
    //                     Transform chosenLocation = e.Value.transform;
    //                     TaskLocation taskLocation = chosenLocation.GetComponent<TaskLocation>();
    //                     currentInteractableLocation = taskLocation;
    //                 }
                
    //                 // Turn the location where the lion interacted with to location fixed
    //                 currentInteractableLocation.locationFixed = true;
    //                 currentInteractableLocation.fixable = false;

    //                 // Iterate through the possibleTasksList
    //                 foreach (var task in possibleTasks)
    //                 {
    //                     // If the object the lion placed has the same task as one of th task on the location
    //                     if(lion.lastObjectTask.taskName == task.taskName) 
    //                     {
    //                         completeTasks.Add(task);
    //                     }
    //                 }
                    
    //                 // then turn state to end state
    //                 gamePlayManagement = GamePlayManagement.END;
    //             }

    //         break;

    //         case GamePlayManagement.END:
    //             if(!stateChange) 
    //             {
    //                 stateChange = true;
    //                 StartCoroutine(ResetState());
    //                 // StartCoroutine(TempMethod());
    //             }

    //             lionPlacedObject = false;
    //         break;
    //     }
    // }

    private IEnumerator TempMethod() 
    {
        yield return new WaitForSeconds(3f);
        gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;
        yield break;
    }

    private IEnumerator ResetState()
    {
        lionPlacedObject = false;

        // Set player state to end state
        for (int i = 0; i < allCrowdPlayers.Count; i++)
        {
            var player = allCrowdPlayers[i];
            player.playerState = CrowdPlayerManager.PlayerState.END;
        }      

        yield return null;

        // Update the task locations list
        bool containsName = false;
        foreach(Transform location in taskLocationsDone)
        {
            // Debug.Log($"Location name -> {location.gameObject.name} is the same as the task location name -> {currentInteractableLocation.gameObject.name}.");
            if(location != null && location.gameObject.name == currentInteractableLocation.gameObject.name)
            {
                containsName = true;
                break;
            }
        }

        yield return null;

        if(!containsName)
        {
            taskLocationsDone.Add(currentInteractableLocation.transform);
            taskLocations.Remove(currentInteractableLocation.transform);
        }

        yield return null;

        foreach (var trackable in trackables)
        {
            Destroy(trackable);
        }

        chosenLocations.Clear();
        ChosenLocations.Clear();
        tasksPerRound.Clear();
        trackables.Clear();
        tasksPerRound.Clear();

        yield return new WaitForSeconds(3f);
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

    private IEnumerator StartRound(float spawnInTimer) 
    {
        yield return new WaitForSeconds(spawnInTimer);

        // Spawn in the locations
        SpawnTaskLocations();

        // Show UI something like round start
        Debug.Log("round started");

        // Wait a few seconds
        yield return new WaitForSeconds(2f);

        foreach (var taskLocation in taskLocations)
        {
            TaskLocation taskLoc = taskLocation.GetComponent<TaskLocation>();
            
            foreach (var task in taskLoc.tasks)
            {
                tasksPerRound.Add(task);
            }
        }

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
        // if (allCrowdPlayers == null || allCrowdPlayers.Count == 0)
        // {
        //     Debug.Log("No players found!");
        //     yield break;
        // }

        // // Populate chosenLocations dictionary
        // foreach (var player in allCrowdPlayers)
        // {
        //     if (player.playerController.chosenLocation != null && !chosenLocations.ContainsKey(player))
        //     {
        //         chosenLocations[player] = player.playerController.chosenLocation;
        //     }
        // }

        // foreach (var element in chosenLocations)
        // {
        //     if (!ChosenLocations.Any(e => e.Key == element.Key)) // Check by player reference
        //     {
        //         ChosenLocations.Add(new DictionaryEntry<CrowdPlayerManager, Transform>
        //         {
        //             Key = element.Key,
        //             Value = element.Value
        //         });
        //     }
        // }

        gameManagerRpcBehaviour.InitiliazeLocationServerRpc();
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