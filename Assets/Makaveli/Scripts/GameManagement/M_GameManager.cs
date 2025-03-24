using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public Lion lion;
    public bool lionPlacedObject;
    public TaskLocation currentInteractableLocation;
    private GameManagerRpcBehaviour gameManagerRpcBehaviour;
    private bool taskStarted = false; 
    private bool penaltyApplied = false; 

    [Header("Point System Management")]
    public float currentPoint = 0;
    public float maxPoints;
    public TextMeshProUGUI pointsText;

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

    private bool spawnLocation;
    void Update()
    {
        switch (gamePlayManagement)
        {
            case GamePlayManagement.START:
                gameManagerRpcBehaviour.GameStateManagement("START");
            break;

            case GamePlayManagement.SPAWN_LOCATIONS:
                if(!spawnLocation) 
                {
                    gameManagerRpcBehaviour.GameStateManagement("SPAWN_LOCATIONS");
                    spawnLocation = true;
                }

            break;

            case GamePlayManagement.CROWD_TURN:
                spawnLocation = false;
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
    
    // POINTS NOT GETTING UPDATED FOR CROWD PLAYER
    // THE POSSIBLE TASKS AND COMPLETE TASKS LIST IS NOT GETTING UPDATED
    public void SolvingTaskState()
    {
        if (taskStarted || penaltyApplied) return; // Prevents multiple executions

        stateChange = false;
                
        if(lionPlacedObject && !taskStarted) 
        {
            taskStarted = true;
            
            // Fetch the location
            Transform taskLocation = lion.taskLocation;
            if(taskLocation == null) return;
            currentInteractableLocation = lion.taskLocationRef;

            Debug.Log($"From SolvingTaskState method: {taskLocation.gameObject.name}");
            Debug.Log($"current interactable location: {currentInteractableLocation.gameObject.name}");

            // Check if the object placed is the same task as one of the tasks on the location
            if(lion.lastObjectTask != null) 
            {
                foreach (var task in currentInteractableLocation.tasks)
                {
                    Debug.Log("last object task:" + lion.lastObjectTask.name);
                    Debug.Log("Task:" + task.name);
                    if(lion.lastObjectTask.taskName == task.taskName) 
                    {
                        currentInteractableLocation.locationFixed = true;
                        currentInteractableLocation.fixable = false;

                        // Add the task to complete task
                        for (int i = 0; i < possibleTasks.Count; i++)
                        {
                            if (possibleTasks[i].taskName == task.taskName) 
                            {
                                completeTasks.Add(possibleTasks[i]);
                                possibleTasks.RemoveAt(i); 
                                break;
                            }
                        }

                        taskComplete = true;
                        //currentPoint += 1f;
                        UpdatePoints(1);
                        Debug.Log($"Adding +1 point to {currentPoint}");
                    }    
                    else 
                    {
                        //currentPoint += 0;
                        Debug.Log($"Adding +0 point to {currentPoint}");
                    }
                    
                }

                StartCoroutine(DisplayEndRound());
            }
            
            // StartCoroutine(DisplayEndRound(lion));
        }
        else
        {
            Debug.Log($"SolvingTaskState: lionPlacedObject: {lionPlacedObject}, taskStarted {taskStarted}");
        }
    }


    public void UpdatePoints(int _points)
    {
        if(!lion.customNetworkBehaviour.CustomIsOwner()) return;
        gameManagerRpcBehaviour.UpdatePoints(_points);
    }

    private IEnumerator DisplayEndRound() 
    {
        Debug.Log("End Round Started...");

        if(taskComplete) 
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

    // private IEnumerator DisplayEndRound(Lion lion) 
    // {
    //     Debug.Log("End Round Started...");

    //     if(lion.correctTask) 
    //     {
    //         Debug.Log("Correct task objet");
    //     }
    //     else 
    //     {
    //         Debug.Log("Incorrect task objet");
    //     }

    //     yield return new WaitForSeconds(5f);

    //     // then turn state to end state
    //     gamePlayManagement = GamePlayManagement.END;
    // }

    public void EndState()
    {
        if(!stateChange) 
        {
            stateChange = true;
            StartCoroutine(ResetState());
        }
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

        // Check if the task already exist in the task location done list
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

        // If not continue
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

        taskStarted = false; 
        penaltyApplied = false; 
        
        yield return new WaitForSeconds(3f);
        gamePlayManagement = GamePlayManagement.START;

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