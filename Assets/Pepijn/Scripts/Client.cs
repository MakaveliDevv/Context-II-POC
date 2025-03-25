using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : NetworkBehaviour
{
    public Server server;
    public NetworkObject sphere, block, cylinder;
    [SerializeField] GameObject lion, crowd;
    public Vector3 spawnLocation;
    private ClientManager clientManager;

    void Start()
    {
        if(clientManager == null) clientManager = FindFirstObjectByType<ClientManager>();
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
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
            Debug.Log(NetworkManager.Singleton.LocalClientId);
        }
    }


    private void OnClientLoadedScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(IsOwner) 
        {
            Debug.Log($"Client {clientId} has finished loading scene: {sceneName}");
            clientManager.AddClientToLoadedListServerRpc(NetworkManager.Singleton.LocalClientId);
        }
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

    public override void OnDestroy()
    {
        base.OnDestroy();
        // Unsubscribe to prevent errors when the object is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    [ServerRpc]
    public void SpawnCrowdAtRandomPositionServerRPC(ulong _clientID)
    {
        // Get available spawn positions
        List<Transform> availablePositions = new(MGameManager.instance.playersSpawnPositions);
        
        for (int i = 0; i < availablePositions.Count; i++)
        {
            Debug.Log($"location object name: {availablePositions[i].gameObject.name} || local position: {availablePositions[i].localPosition} || position: {availablePositions[i].position}");
        }
        
        // Select a random position for the crowd
        int randomIndex = Random.Range(0, availablePositions.Count);
        Vector3 spawnLocation = availablePositions[randomIndex].position;
        spawnLocation.y += 10f;

        Debug.Log($"Spawning at: {spawnLocation}");
        
        // Instantiate and spawn the crowd at the random position
        GameObject crowdInstance = Instantiate(crowd, spawnLocation, Quaternion.identity);
        NetworkObject crowdNetworkInstance = crowdInstance.GetComponent<NetworkObject>();
        crowdNetworkInstance.SpawnWithOwnership(_clientID);
        
        // Update client IDs
        crowdNetworkInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
        crowdNetworkInstance.transform.GetChild(0).gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);

        // Force child position after spawn
        StartCoroutine(FixChildPosition(crowdNetworkInstance.transform));
    }

    IEnumerator FixChildPosition(Transform parent)
    {
        yield return new WaitForSeconds(0.5f); // Delay to let everything initialize

        Transform child = parent.GetChild(0);
        child.localPosition = Vector3.zero;
        child.position = parent.position;

        // Debug.Log($"[Fix] Parent Pos: {parent.position}, Child World Pos: {child.position}, Child Local Pos: {child.localPosition}");
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var obj in FindObjectsOfType<NetworkObject>())
            {
                if (obj.IsSpawned && obj.OwnerClientId == clientId)
                {
                    obj.Despawn(true);  // Remove from all clients
                    Destroy(obj.gameObject);  // Ensure removal
                }
            }
        }
    }

}
