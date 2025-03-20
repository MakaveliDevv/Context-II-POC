using System.Collections;
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

    // void LateUpdate()
    // {
    //     Transform child = crowd.transform.GetChild(0);
    //     child.localPosition = Vector3.zero;
    //     Debug.Log($"[Fix Attempt] Child Local Pos Reset to: {child.localPosition}");
    //     Debug.Log($"[Final Override] Child World Pos: {transform.position}, Local Pos: {transform.localPosition}");
    // }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsOwner)
        {
            GameObject.Find("ClientServerRefs").GetComponent<ClientServerRefs>().localClient = this;
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

    [ServerRpc]
    public void SpawnCrowdAtRandomPositionServerRPC(ulong _clientID)
    {
        // Get available spawn positions
        // List<Transform> availablePositions = new(MGameManager.instance.playersSpawnPositions);
        
        // // Select a random position for the crowd
        // int randomIndex = Random.Range(0, availablePositions.Count);
        // Vector3 spawnLocation = availablePositions[randomIndex].localPosition;
        // Debug.Log($"Spawning at: {spawnLocation}");
        
        // Instantiate and spawn the crowd at the random position
        GameObject crowdInstance = Instantiate(crowd, /*spawnLocation*/new Vector3(-9.28299999f, 12.9219999f, -31.0100002f), Quaternion.identity);
        NetworkObject crowdNetworkInstance = crowdInstance.GetComponent<NetworkObject>();
        crowdNetworkInstance.SpawnWithOwnership(_clientID);
        
        // Update client IDs
        crowdNetworkInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
        crowdNetworkInstance.transform.GetChild(0).gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);

        // Force child position after spawn
        Transform childTransform = crowdNetworkInstance.transform.GetChild(0);
        childTransform.localPosition = Vector3.zero;
        childTransform.position = crowdNetworkInstance.transform.position;

        StartCoroutine(FixChildPosition(crowdNetworkInstance.transform));

        
        // Debug.Log($"Parent Position: {crowdNetworkInstance.transform.position}");
        // Debug.Log($"Child Position: {crowdNetworkInstance.transform.GetChild(0).position}");

        // Debug.Log($"Child Local Position: {crowdNetworkInstance.transform.GetChild(0).localPosition}");

        // Debug.Log($"Child Parent: {crowdNetworkInstance.transform.GetChild(0).parent.name}");
    }

    IEnumerator FixChildPosition(Transform parent)
    {
        yield return new WaitForSeconds(0.5f); // Delay to let everything initialize

        Transform child = parent.GetChild(0);
        child.localPosition = Vector3.zero;
        child.position = parent.position;

        // Debug.Log($"[Fix] Parent Pos: {parent.position}, Child World Pos: {child.position}, Child Local Pos: {child.localPosition}");
    }

}
