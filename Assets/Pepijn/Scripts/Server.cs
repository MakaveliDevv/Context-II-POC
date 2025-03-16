using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Server : NetworkBehaviour
{
    public NetworkObjectReference sphereNetRef, blockNetRef, cylinderNetRef;
    public Dictionary<string, NetworkObjectReference> networkObjectReferences;
    public Dictionary<string, NetworkObjectReference> playerReferences = new();
    public bool instantiatedPrefabs;
    [SerializeField] List<ulong> clientIDs;

    // Update is called once per frame
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
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

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        clientIDs.Add(clientId);
    }
}
