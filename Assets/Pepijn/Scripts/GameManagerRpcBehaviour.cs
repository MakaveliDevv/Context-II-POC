using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerRpcBehaviour : NetworkBehaviour
{
    MGameManager mGameManager;
    ClientManager clientManager;
    [SerializeField] CustomNetworkBehaviour customNetworkBehaviour;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        clientManager = FindFirstObjectByType<ClientManager>();
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
        if(mGameManager == null) return;

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

    public void UpdatePoints(int _points)
    {
        UpdatePointsServerRpc(_points);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePointsServerRpc(int _points)
    {
        MGameManager.instance.currentPoint += _points;
        UpdatePointsClientRpc(_points);
    }
    [ClientRpc]
    void UpdatePointsClientRpc(int _points)
    {
        MGameManager.instance.currentPoint += _points;
        MGameManager.instance.pointsText.text = "Points: " + MGameManager.instance.currentPoint;
    }

    public void StartGameTimer()
    {
        //UpdateGameTimerServerRpc();
        StartCoroutine(GameTimer());
    }

    IEnumerator GameTimer()
    {
        Debug.Log("Started game timer");
        if(mGameManager == null) mGameManager = FindFirstObjectByType<MGameManager>();
        UpdateGameTimerClientRpc(mGameManager.elapsedTime);

        while(mGameManager.elapsedTime <= mGameManager.roundTime)
        {
            mGameManager.timeText.text = "Time: " + (mGameManager.roundTime - mGameManager.elapsedTime).ToString();
            yield return new WaitForSeconds(1);
            mGameManager.elapsedTime++;
            if(mGameManager.roundTime - mGameManager.elapsedTime >= 0) UpdateGameTimerClientRpc(mGameManager.elapsedTime);
        }

        EndGameClientRpc();

        Destroy(mGameManager.lion.gameObject);
        foreach(var _crowdPlayer in mGameManager.allCrowdPlayers)
        {
            foreach(var _npc in _crowdPlayer.playerController.npcs) Destroy(_npc.gameObject);
            Destroy(_crowdPlayer.gameObject);
        }

        yield return new WaitForSeconds(10);

        ReturnToLobby();

        //FindFirstObjectByType<ClientManager>().LoadSceneWithoutDontDestroy("Lobby");
    }
    [ClientRpc]
    void UpdateGameTimerClientRpc(int elapsedTime)
    {
        if(mGameManager == null) mGameManager = FindFirstObjectByType<MGameManager>();
        Debug.Log("Updating time on client");
        mGameManager.timeText.text = "Time: " + (mGameManager.roundTime - elapsedTime).ToString();
    }

    [ClientRpc]
    void EndGameClientRpc()
    {
        mGameManager.timeObj.SetActive(false);
        // mGameManager.lion.gameObject.SetActive(false);
        // foreach(var _crowdPlayer in mGameManager.allCrowdPlayers)
        // {
        //     _crowdPlayer.gameObject.SetActive(false);
        // }
        mGameManager.finishCamera.SetActive(true);
    }

    void ReturnToLobby()
    {
        SceneManager.UnloadSceneAsync("P_GameScene");
        clientManager.scenceLoadedClients.Clear();

        ReturnToLobbyClientRpc();
    }

    [ClientRpc]
    void ReturnToLobbyClientRpc()
    {
        SceneManager.UnloadSceneAsync("P_GameScene");
        clientManager.lobby.SetActive(true);
        clientManager.inLobby = true;
        clientManager.scenceLoadedClients.Clear();
        clientManager.ReadyUp();
    }
}
