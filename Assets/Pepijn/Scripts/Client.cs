using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : NetworkBehaviour
{
    public Server server;
    public NetworkObject sphere, block, cylinder;
    [SerializeField] GameObject lion, crowd;
    NetworkObject lionInstance, crowdInstance; 
    [SerializeField] Vector3 spawnLocation;
    ClientManager clientManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(clientManager == null) clientManager = FindFirstObjectByType<ClientManager>();
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnClientLoadedScene;
        server = GameObject.Find("Server(Clone)").GetComponent<Server>();
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsOwner)
        {
            GameObject.Find("ClientServerRefs").GetComponent<ClientServerRefs>().localClient = this;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnClientLoadedScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(IsOwner) 
        {
            Debug.Log($"Client {clientId} has finished loading scene: {sceneName}");
            clientManager.AddClientToLoadedListServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc]
    void RequestServerActionServerRpc()
    {
        Debug.Log("Received action from client");
    }

    public void SendObjectToServer(string _selectedObject, Vector3 _hitPoint)
    {
        if(IsOwner) 
        {
            Debug.Log("Send object to server");
            SpawnObjectsOnClientsServerRpc(_selectedObject,  _hitPoint);
        }
    }

    [ServerRpc]
    public void SpawnObjectsOnClientsServerRpc(string _selectedObject, Vector3 _hitPoint)
    {
        if(!server.instantiatedPrefabs)
        {
            server.InstantiateNetworkPrefabs(sphere, block, cylinder);
        }
        if (server.networkObjectReferences[_selectedObject].TryGet(out NetworkObject netObj))
        {
            NetworkObject newNetworkObject = Instantiate(netObj, _hitPoint, Quaternion.identity);
            newNetworkObject.Spawn();
        }
    }

    [ServerRpc]
    public void InstantiateLionServerRpc(ulong _clientID)
    {
        GameObject lionInstance = Instantiate(lion, spawnLocation, Quaternion.identity);
        NetworkObject lionNetworkInstance = lionInstance.GetComponent<NetworkObject>();
        lionNetworkInstance.SpawnWithOwnership(_clientID);

        lionNetworkInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
    }

    [ServerRpc]
    public void InstantiateCrowdServerRpc(ulong _clientID)
    {
        GameObject crowdInstance = Instantiate(crowd, spawnLocation, Quaternion.identity);
        NetworkObject crowdNetworkInstance = crowdInstance.GetComponent<NetworkObject>();
        crowdNetworkInstance.SpawnWithOwnership(_clientID);

        crowdNetworkInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
        crowdNetworkInstance.transform.GetChild(0).gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
        Debug.Log("Child Position = " + crowdNetworkInstance.transform.GetChild(0).position);
    }
}
