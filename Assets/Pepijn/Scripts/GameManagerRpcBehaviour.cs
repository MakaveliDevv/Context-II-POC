using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManagerRpcBehaviour : NetworkBehaviour
{
    MGameManager mGameManager;
    [SerializeField] CustomNetworkBehaviour customNetworkBehaviour;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void GameStateManagement(string _state)
    {
        GameStateManagementServerRpc(_state);
    }

    [ServerRpc(RequireOwnership = false)]
    void GameStateManagementServerRpc(string _state)
    {
        GameStateManagementClientRpc(_state);
    }
    
    [ClientRpc]
    void GameStateManagementClientRpc(string _state)
    {
        if(mGameManager == null) mGameManager = FindFirstObjectByType<MGameManager>();

        Debug.Log($"GameManager -> {mGameManager.gameObject.name}");

        switch (_state)
        {
            case "START":
                mGameManager.StartState();
                break;

            case "SPAWN_LOCATIONS":
                mGameManager.SpawnLocationsState();
                break;

            case "CROWD_TURN":
                mGameManager.CrowdTurnState();
                break;

            case "SOLVING_TASK":
                mGameManager.SolvingTaskState();
                break;

            case "END":
                mGameManager.EndState();
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void InitiliazeLocationServerRpc()
    {
        InitiliazeLocationClientRpc();
    }

    [ClientRpc]
    void InitiliazeLocationClientRpc()
    {
        if (mGameManager.allCrowdPlayers == null || mGameManager.allCrowdPlayers.Count == 0)
        {
            Debug.Log("No players found!");
            return;
        }

        // Populate chosenLocations dictionary
        foreach (var player in mGameManager.allCrowdPlayers)
        {
            if (player.playerController.chosenLocation != null && !mGameManager.chosenLocations.ContainsKey(player))
            {
                mGameManager.chosenLocations[player] = player.playerController.chosenLocation;
            }
        }

        foreach (var element in mGameManager.chosenLocations)
        {
            if (!mGameManager.ChosenLocations.Any(e => e.Key == element.Key)) // Check by player reference
            {
                mGameManager.ChosenLocations.Add(new DictionaryEntry<CrowdPlayerManager, Transform>
                {
                    Key = element.Key,
                    Value = element.Value
                });
            }
        }
    }
}
