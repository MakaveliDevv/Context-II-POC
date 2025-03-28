using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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

        Debug.Log($"Spawning at: {spawnLocation}");
        
        // Instantiate and spawn the crowd at the random position
        GameObject crowdInstance = Instantiate(crowd, spawnLocation, Quaternion.identity);
        Debug.Log($"Spawned Crowd Instance: {crowdInstance.gameObject.name}");

        NetworkObject crowdNetworkInstance = crowdInstance.GetComponent<NetworkObject>();
        crowdNetworkInstance.SpawnWithOwnership(_clientID);
        
        // Update client IDs
        Debug.Log($"Spawned Crowd NetworkInstance: {crowdNetworkInstance.gameObject.name}");
        crowdNetworkInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
        crowdNetworkInstance.transform.GetChild(0).gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);

        // Force child position after spawn
        FixChildPositionClientRpc(_clientID, crowdNetworkInstance.NetworkObjectId);
    }

    [ClientRpc]
    void FixChildPositionClientRpc(ulong _clientID, ulong spawnedObjectId)
    {
        if(NetworkManager.Singleton.LocalClientId != _clientID) return;
        NetworkObject spawnedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[spawnedObjectId];
        StartCoroutine(FixChildPosition(spawnedObject.gameObject.transform.GetChild(0)));
    }

    IEnumerator FixChildPosition(Transform _child)
    {
        yield return new WaitForSeconds(0.5f);
        while(_child.localPosition != Vector3.zero)
        {
            _child.localPosition = Vector3.zero;
            Debug.Log($"Setting {_child.name} to {_child.localPosition}");
            yield return null;
        }

        _child.transform.parent.GetComponent<CrowdPlayerManager>().spawnedSuccesfully = true;
        yield return new WaitForSeconds(0.2f);
        GameObject.Find("LoadingCanvas").SetActive(false);
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
