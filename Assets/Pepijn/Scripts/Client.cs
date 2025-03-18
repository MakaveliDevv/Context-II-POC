using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Client : NetworkBehaviour
{
    public Server server;
    public NetworkObject sphere, block, cylinder;
    [SerializeField] GameObject lion, crowd;
    NetworkObject lionInstance, crowdInstance; 
    [SerializeField] Vector3 spawnLocation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        server = GameObject.Find("Server(Clone)").GetComponent<Server>();
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

    [ServerRpc]
    public void SpawnCrowdAtRandomPositionServerRPC(ulong _clientID)
    {
        // Get available spawn positions
        List<Transform> availablePositions = new(MGameManager.instance.playersSpawnPositions);
        
        // Select a random position for the crowd
        int randomIndex = Random.Range(0, availablePositions.Count);
        Vector3 spawnLocation = availablePositions[randomIndex].localPosition;

        Debug.Log($"Spawn location ->  {spawnLocation}");
        
        // Instantiate and spawn the crowd at the random position
        GameObject crowdInstance = Instantiate(crowd, spawnLocation, Quaternion.identity);
        NetworkObject crowdNetworkInstance = crowdInstance.GetComponent<NetworkObject>();
        crowdNetworkInstance.SpawnWithOwnership(_clientID);
        
        // Update client IDs
        crowdNetworkInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
        crowdNetworkInstance.transform.GetChild(0).gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
        
        // Log child position
        Debug.Log("Child Position = " + crowdNetworkInstance.transform.GetChild(0).position);
    }

    // private void PlayersSpawnPosition()
    // {        
    //     List<Transform> availablePositions = new(MGameManager.instance.playersSpawnPositions);
    //     List<CrowdPlayerManager> shuffledPlayers = new(MGameManager.instance.allCrowdPlayers);        
    //     int playersToSpawn = Mathf.Min(availablePositions.Count, shuffledPlayers.Count);
        
    //     for (int i = 0; i < playersToSpawn; i++)
    //     {
    //         int randomIndex = Random.Range(0, availablePositions.Count);            
    //         var player = shuffledPlayers[i];            
    //         player.transform.position = availablePositions[randomIndex].position;            
    //         availablePositions.RemoveAt(randomIndex);
    //     }        
    // }
}
