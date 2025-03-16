using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Server : NetworkBehaviour
{
    public NetworkObjectReference sphereNetRef, blockNetRef, cylinderNetRef;
    public Dictionary<string, NetworkObjectReference> networkObjectReferences;
    public bool instantiatedPrefabs;

    // Update is called once per frame

    [ClientRpc]
    void PerformActionClientRpc()
    {
        Debug.Log("Received action from the server!");
    }

    public void InstantiateNetworkPrefabs(NetworkObject sphere, NetworkObject block, NetworkObject cylinder)
    {
        NetworkObject sphereGO = Instantiate(sphere, new Vector3(1000, 1000, 1000), Quaternion.identity);
        NetworkObject blockGO = Instantiate(block, new Vector3(1000, 1000, 1000), Quaternion.identity);
        NetworkObject cylinderGO = Instantiate(cylinder, new Vector3(1000, 1000, 1000), Quaternion.identity);

        sphereGO.Spawn();
        blockGO.Spawn();
        cylinderGO.Spawn();

        sphereNetRef = sphereGO;
        blockNetRef = blockGO;
        cylinderNetRef = cylinderGO;

        networkObjectReferences = new()
        {
            ["sphere"] = sphereNetRef,
            ["block"] = blockNetRef,
            ["cylinder"] = cylinderNetRef
        };

        instantiatedPrefabs = true;

        Debug.Log("Instantaited network prefabs");
    }

    // public void TrySpawnObjectsOnClient(string _selectedObject, Vector3 _hitPoint)
    // {
    //     if(IsServer)
    //     {
    //         if (networkObjectReferences[_selectedObject].TryGet(out NetworkObject netObj))
    //         {
    //             NetworkObject newNetworkObject = Instantiate(netObj, _hitPoint + new Vector3(0, 1, 0), Quaternion.identity);
    //             newNetworkObject.Spawn();
    //         }
    //     }
    // }
    


    // [ClientRpc]
    // void PlaceObjectOnClientRpc(string _selectedObject, Vector3 _hitPoint)
    // {
    //     Debug.Log("Try spawn object on client");
    //     if (_selectedObject.TryGet(out NetworkObject networkObject))
    //     {
    //         Debug.Log("Received NetworkObject: " + networkObject.gameObject.name);
    //         // Perform actions on the object
    //         Instantiate(networkObject, _hitPoint + new Vector3(0, 1, 0), Quaternion.identity);
    //     }
    // }
    
}
