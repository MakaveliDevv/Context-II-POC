using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    public static MGameManager instance;
    public enum GamePlayManagement { SPAWN_LOCATIONS, CROWD_TURN, SOLVING_PROBLEM, REMOVE_LOCATIONS }
    public GamePlayManagement gamePlayManagement; 

    [Header("Minimap Management")]
    public List<GameObject> markers = new();
    [HideInInspector] public List<GameObject> trackables = new();
    public GameObject trackableGo;
    public Transform trackablesParent;
    public List<Transform> locations = new();
    public int trackableAmount;

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
    [SerializeField] private float spawnInTimer;
    public float chooseLocationTimer;
    public bool showLocationCards = false;
    public bool allPlayersAtLocation = false;
    private bool spawnLocations;

    private bool stateChange;

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
        switch (gamePlayManagement)
        {
            case GamePlayManagement.SPAWN_LOCATIONS:
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

            case GamePlayManagement.SOLVING_PROBLEM:
                // If lion placed object, then start new 'round'      

            break;

            case GamePlayManagement.REMOVE_LOCATIONS:
                foreach (var player in allCrowdPlayers)
                {
                    player.signal = false;
                }

                StartCoroutine(ResetState());

            break;
        }
    }

    // private IEnumerator ResetState() 
    // {
    //     yield return new WaitForSeconds(2f);
    //     GameObject location = null;
        
    //     for (int i = 0; i < locations.Count; i++)
    //     {
    //         location = locations[i].gameObject;
    //     }

    //     Destroy(location);    
    //     locations.Clear();
    //     gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;

    //     yield break;
    // }

    private IEnumerator ResetState() 
    {
        yield return new WaitForSeconds(2f);

        foreach (var location in locations)
        {
            if (location != null) 
            {
                Destroy(location.gameObject);
            }
        }

        locations.Clear(); 
        gamePlayManagement = GamePlayManagement.SPAWN_LOCATIONS;
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
        SpawnLocation();

        // Show UI something like round start


        // Wait a few seconds
        yield return new WaitForSeconds(5f);

        gamePlayManagement = GamePlayManagement.CROWD_TURN;

        // Spawn in the UI for the players
        showLocationCards = true;

        yield break;
    }

    private void SpawnLocation() 
    {
        if (locations == null || locations.Count == 0) 
        {
            Debug.LogError("locations array is null or empty!");
            return;
        }

        // Shuffle the list to get random locations
        List<Transform> shuffledLocations = locations.OrderBy(x => Random.value).ToList();
        
        int spawnCount = Mathf.Min(trackableAmount, shuffledLocations.Count);

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