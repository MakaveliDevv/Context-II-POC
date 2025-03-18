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
    }
}
