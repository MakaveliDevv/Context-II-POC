using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientManager : NetworkBehaviour
{
    public static ClientManager instance;
    public int lionClientId;
    public List<ulong> connectedClients = new();
    public List<ulong> scenceLoadedClients = new();
    public List<ulong> readiedClients = new();
    [SerializeField] List<TextMeshProUGUI> lobbyClientTexts;
    [SerializeField] TextMeshProUGUI clientsConnectedText;
    public bool isLion;
    [SerializeField] public GameObject startButton, readyButton, clientConnectedTexts;
    public Image readyButtonImg;
    public Sprite readySprite, unreadySprite;
    bool isReady;
    [SerializeField] GameObject lobby;
    bool inLobby;
    void Awake()
    {
        instance = this;
        inLobby = true;
    }
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Lobby") 
        {
            connectedClients = new();
            scenceLoadedClients = new();
            readiedClients = new();
        }
        

        //DontDestroyOnLoad(gameObject);
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        scenceLoadedClients.Clear();
    }

    public void ReadyUp()
    {
        if(!isReady)
        {
            readyButtonImg.sprite = readySprite;
            isReady = true;
            AddClientToReadyListServerRpc(NetworkManager.LocalClientId, true);
        }
        else
        {
            readyButtonImg.sprite = unreadySprite;
            isReady = false;
            AddClientToReadyListServerRpc(NetworkManager.LocalClientId, false);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if(!IsServer) return;

        Debug.Log($"Client {clientId} connected to the server.");
        connectedClients.Add(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyTextServerRpc();

        OnClientConnectedClientRpc(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if(!IsServer) return;

        Debug.Log($"Client {clientId} disconnected from the server.");
        connectedClients.Remove(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyTextServerRpc();

        OnClientDisconnectedClientRpc(clientId);

        if(connectedClients.Count == 0)
        {
            LoadSceneWithoutDontDestroy("Lobby");
            //NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateLobbyTextServerRpc()
    {
        UpdateLobbyTextClientRpc(connectedClients.ToArray());

        for(int i = 0; i < lobbyClientTexts.Count; i++)
        {
            if(i < connectedClients.Count) lobbyClientTexts[i].text = "Client " + connectedClients[i];
            else lobbyClientTexts[i].text = "";
        }

        clientsConnectedText.text = $"Connected Clients ({connectedClients.Count}/6)";
    }

    public void LoadSceneWithoutDontDestroy(string sceneName)
    {
        // Find all objects marked as DontDestroyOnLoad
        List<GameObject> dontDestroyObjects = FindAllDontDestroyOnLoadObjects();

        // Destroy them
        foreach (GameObject obj in dontDestroyObjects)
        {
            Destroy(obj);
        }

        // Load the new scene
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    [ClientRpc]
    void UpdateLobbyTextClientRpc(ulong[] _connectedClients)
    {
        for(int i = 0; i < lobbyClientTexts.Count; i++)
        {
            if(i < _connectedClients.Length) lobbyClientTexts[i].text = "Client " + _connectedClients[i];
            else lobbyClientTexts[i].text = "";
        }

        clientsConnectedText.text = $"Connected Clients ({_connectedClients.Length}/6)";
    }

    // public void LoadGameScene()
    // {
    //     LoadGameSceneServerRpc();
    // }

    public void QuitGame()
    {
        Application.Quit();
    }

    // [ServerRpc(RequireOwnership = false)]
    // void LoadGameSceneServerRpc()
    // {
    //     NetworkManager.Singleton.SceneManager.LoadScene("P_GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    // }
    
    [ClientRpc]
    void OnClientConnectedClientRpc(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected to the server.");
        connectedClients.Add(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyTextServerRpc();
    }

    [ClientRpc]
    void OnClientDisconnectedClientRpc(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected from the server.");
        readiedClients.Remove(clientId); 
        connectedClients.Remove(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyTextServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void AddClientToReadyListServerRpc(ulong _clientID, bool _add)
    {
        if(_add) { readiedClients.Add(_clientID); }    
        else readiedClients.Remove(_clientID); 

        if((readiedClients.Count == connectedClients.Count) && (readiedClients.Count > 1))
        {
            //NetworkManager.Singleton.SceneManager.LoadScene("P_GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            NetworkManager.Singleton.SceneManager.LoadScene("P_GameScene", LoadSceneMode.Additive);
            inLobby = false;
            lobby.SetActive(false);
            DisableLobbyClientRpc();
        }
    }

    [ClientRpc]
    void DisableLobbyClientRpc()    
    {
        inLobby = false;
        lobby.SetActive(false);
    }

    // private IEnumerator SwitchScene(string newSceneName)
    // {
    //     // Get the current active scene
    //     Scene oldScene = SceneManager.GetActiveScene();

    //     // Load the new scene additively
    //     NetworkManager.Singleton.SceneManager.LoadScene(newSceneName, LoadSceneMode.Additive);

    //     // Set the new scene as active
    //     //NetworkManager.Singleton.SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));

    //     // Unload the old scene, but keep DontDestroyOnLoad objects
    //     SceneManager.UnloadSceneAsync(oldScene);
    // }

    [ServerRpc(RequireOwnership = false)]
    public void AddClientToLoadedListServerRpc(ulong _clientID)
    {
        scenceLoadedClients.Add(_clientID);
        Debug.Log($"Loaded client {_clientID}");
        if(!inLobby)
        {
            if(scenceLoadedClients.Count == connectedClients.Count)
            {
                Debug.Log("All clients loaded");
                //DecideLionServerRpc();
                //ActivateSceneClientRpc();
                ulong lionID;
                if (lionClientId == 0) 
                {
                    lionID = connectedClients[Random.Range(0, connectedClients.Count)];
                }
                else
                {
                    lionID = (ulong)lionClientId;
                }
                DecideLionClientRpc(lionID);
            }
            else
            {
                Debug.Log($"{scenceLoadedClients.Count} / {connectedClients.Count} connected");
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void ActivateSceneServerRpc()
    {
        Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "SceneObj").SetActive(true); 
        ActivateSceneClientRpc();
    }
    [ClientRpc]
    void ActivateSceneClientRpc()
    {
        Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "SceneObj").SetActive(true);  
    }

    [ServerRpc(RequireOwnership = false)]
    void DecideLionServerRpc()
    {
        ulong lionID = 0;
        if(lionClientId == 0) 
        {
            lionID = connectedClients[Random.Range(0, connectedClients.Count)];
        }
        else
        {
            lionID = (ulong)lionClientId;
        }
        DecideLionClientRpc(lionID);
    }

    [ClientRpc]
    void DecideLionClientRpc(ulong _lionID)
    {
        if(NetworkManager.LocalClientId == _lionID)
        {
            Debug.Log("Client set to lion");
            isLion = true;
            ActivateSceneServerRpc();
        }
        else
        {
            Debug.Log($"Client not set to lion: {NetworkManager.LocalClientId}, not {_lionID}");
        }
    }

        private List<GameObject> FindAllDontDestroyOnLoadObjects()
        {
            List<GameObject> dontDestroyObjects = new List<GameObject>();

            // Create a temporary scene to move DontDestroyOnLoad objects into
            Scene tempScene = SceneManager.CreateScene("TempScene");
            
            // Move all root objects from DontDestroyOnLoad to the new scene
            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (obj.scene.name == null || obj.scene.name == "DontDestroyOnLoad")
                {
                    dontDestroyObjects.Add(obj);
                    SceneManager.MoveGameObjectToScene(obj, tempScene);
                }
            }

            return dontDestroyObjects;
        }
}
