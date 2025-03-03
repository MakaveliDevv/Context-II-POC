using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Client : NetworkBehaviour
{
    public Server server;
    public NetworkObject sphere, block, cylinder;

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
        //if (IsOwner) RequestServerActionServerRpc();
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
            NetworkObject newNetworkObject = Instantiate(netObj, _hitPoint + new Vector3(0, 1, 0), Quaternion.identity);
            newNetworkObject.Spawn();
        }
        
    }

    // [ServerRpc]
    // void PlaceObjectOnServerRpc(string _selectedObject, Vector3 _hitPoint)
    // {
    //     Debug.Log("Try send spawn object to client");
    //     if (networkObjectReferences[_selectedObject].TryGet(out NetworkObject netObj))
    //     {
    //         NetworkObject newNetworkObject = Instantiate(netObj, _hitPoint + new Vector3(0, 1, 0), Quaternion.identity);
    //         newNetworkObject.Spawn();
    //     }
    // }

    // public void TrySpawnObjectsOnClient(string _selectedObject, Vector3 _hitPoint)
    // {
    //     //if(IsServer) PlaceObjectOnClientRpc( _selectedObject, _hitPoint);
    //     if (networkObjectReferences[_selectedObject].TryGet(out NetworkObject netObj))
    //     {
    //         NetworkObject newNetworkObject = Instantiate(netObj, _hitPoint + new Vector3(0, 1, 0), Quaternion.identity);
    //         newNetworkObject.Spawn();
    //     }
    // }
}
