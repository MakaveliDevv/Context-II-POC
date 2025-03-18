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
        // clientManager = FindFirstObjectByType<ClientManager>();
        // isLion = clientManager.isLion;

        if(!serverManager.serverBuild)
        {
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
    }


    void InstantiateLion()
    {
        StartCoroutine(TrySpawnPlayers(true));
    }

    void InstantiateCrowd()
    {
        StartCoroutine(TrySpawnPlayers(false));
    }

    IEnumerator TrySpawnPlayers(bool asLion)
    {
        bool success = false;
        while(!success)
        {
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
                success = true;
            }
            yield return null;
        }
        yield return null;
    }
}
