using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    public enum GamePlayManagement { NOTHING, TRAVELING, CHOOSE_LOCATION, CHOOSE_SHAPE, SIGNAL };
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
    public bool navigationUI = false;

    // NPC related stuff
    [Header("NPC Stuff")]
    // public GameObject patrolArea;
    public Transform walkableArea;
    public int trackableObjectAmount;
    [HideInInspector] public List<NPCManager> allNPCs = new();

    [Header("Round Management")]
    [SerializeField] private float chooseLocationTimer;
    public bool showLocationCards = false;
    public bool roundEnd = false;
    public bool allPlayersAtLocation = true;

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

        StartCoroutine(StartRound());
        chosenLocations.Clear();
        ChosenLocations.Clear();
    }

    void Update()
    {
        // if(gamePlayManagement == GamePlayManagement.CHOOSE_LOCATION) 
        // {
        //     // Initialize the locations
        //     StartCoroutine(InitializeLocations());
        // }
        // else 
        // {
        //     StopCoroutine(InitializeLocations());
        // }

        switch (gamePlayManagement)
        {
            case GamePlayManagement.CHOOSE_LOCATION:
                StartCoroutine(InitializeLocations());

            break; 

            case GamePlayManagement.TRAVELING:
                StopCoroutine(InitializeLocations());

                // Check if each player has reached the destination
                for (int i = 0; i < chosenLocations.Count; i++)
                {
                    var element = chosenLocations.ElementAt(i);
                    Vector3 playerPosition = element.Key.transform.position;
                    Vector3 locationPosition = element.Value.position;

                    Debug.Log($"Player position: {playerPosition}");
                    Debug.Log($"Location position: {locationPosition}");
                    
                    if((playerPosition - locationPosition).sqrMagnitude >= .5f)
                    {
                        allPlayersAtLocation = false;
                        break;
                    }

                    if(allPlayersAtLocation && chosenLocations.Count > 0)
                    {
                        gamePlayManagement = GamePlayManagement.CHOOSE_SHAPE;
                    }
                }

            break;

            case GamePlayManagement.CHOOSE_SHAPE:
                StartCoroutine(InitializeLocations());

            break;

            default:

            break;
        }
    }

    public GameObject InstantiatePrefab(GameObject prefab, Transform parent) 
    {
        GameObject newGameObject = Instantiate(prefab);
        newGameObject.transform.SetParent(parent, true);

        return newGameObject;
    }

    private void SpawnObstacleLocation() 
    {
        for (int i = 0; i < trackableObjectAmount; i++)
        {
            InstantiatePrefab(trackableObject, trackableObjectParent);
        }
    }

    private IEnumerator StartRound() 
    {
        yield return new WaitForSeconds(3f);

        // Spawn in the locations
        SpawnObstacleLocation();

        // Wait a few seconds
        yield return new WaitForSeconds(2f);

        gamePlayManagement = GamePlayManagement.CHOOSE_LOCATION;

        // Spawn in the UI for the players
        showLocationCards = true;

        // Start timer to choose location
        yield return new WaitForSeconds(chooseLocationTimer);

        showLocationCards = false;

        gamePlayManagement = GamePlayManagement.TRAVELING;

        yield break;
    }

    private IEnumerator InitializeLocations()
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

    private void ShowNavigationUI() 
    {

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