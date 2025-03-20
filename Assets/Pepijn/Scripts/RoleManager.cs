using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoleManager : NetworkBehaviour
{
    [SerializeField] public bool isLion;
    [SerializeField] ClientServerRefs clientServerRefs;
    [SerializeField] ServerManager serverManager;
    ClientManager clientManager;

    void Start()
    {
        Debug.Log("Rolemanager Start");
        clientManager = FindFirstObjectByType<ClientManager>();
        serverManager = FindFirstObjectByType<ServerManager>();
        clientServerRefs = FindFirstObjectByType<ClientServerRefs>();
        
        isLion = clientManager.isLion;

        if(!serverManager.serverBuild)
        {
            Debug.Log("Not a server build");
            if(isLion) 
            {
                InstantiateLion();
                clientServerRefs.isLion = true;
            }
            else 
            {
                InstantiateCrowd(); 
                clientServerRefs.isCrowd = true;
            }
        }
        else
        {
            Debug.Log("server build");
        }
    }


    void InstantiateLion()
    {
        Debug.Log("Starting coroutine");
        StartCoroutine(TrySpawnPlayers(true));
    }

    void InstantiateCrowd()
    {
        Debug.Log("Starting coroutine");
        StartCoroutine(TrySpawnPlayers(false));
    }

    IEnumerator TrySpawnPlayers(bool asLion)
    {
        bool success = false;
        while(!success)
        {
            Debug.Log("Trying to spawn as lion: " + asLion);
            if(clientServerRefs.localClient != null)
            {
                if(asLion)
                {
                    clientServerRefs.localClient.InstantiateLionServerRpc(NetworkManager.Singleton.LocalClientId);
                }
                else
                {
                    clientServerRefs.localClient.SpawnCrowdAtRandomPositionServerRPC(NetworkManager.Singleton.LocalClientId);
                }
                Debug.Log("Spawned as lion: " + asLion);
                success = true;
            }
            yield return null;
        }
        yield return null;
    }
}
