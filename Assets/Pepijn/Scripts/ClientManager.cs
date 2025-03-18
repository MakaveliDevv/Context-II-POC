using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : NetworkBehaviour
{
    public bool isLion;
    public List<ulong> connectedClients;
    [SerializeField] List<TextMeshProUGUI> lobbyClientTexts;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if(!IsServer) return;

        Debug.Log($"Client {clientId} connected to the server.");
        connectedClients.Add(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyText();

        OnClientConnectedClientRpc(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if(!IsServer) return;

        Debug.Log($"Client {clientId} disconnected from the server.");
        connectedClients.Remove(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyText();

        OnClientDisconnectedClientRpc(clientId);
    }

    void UpdateLobbyText()
    {
        for(int i = 0; i < lobbyClientTexts.Count; i++)
        {
            if(i < connectedClients.Count) lobbyClientTexts[i].text = "Client " + connectedClients[i];
            else lobbyClientTexts[i].text = "";
        }
    }

    public void LoadGameScene()
    {
        LoadGameSceneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void LoadGameSceneServerRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    [ClientRpc]
    void OnClientConnectedClientRpc(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected to the server.");
        connectedClients.Add(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyText();
    }

    [ClientRpc]
    void OnClientDisconnectedClientRpc(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected from the server.");
        connectedClients.Remove(clientId);
        if(SceneManager.GetActiveScene().name == "Lobby") UpdateLobbyText();
    }
}
