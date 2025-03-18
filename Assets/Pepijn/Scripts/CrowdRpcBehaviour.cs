using Unity.Netcode;
using UnityEngine;

public class CrowdRpcBehaviour : NetworkBehaviour
{
    public CharacterController controller;
    public int npcCount;
    public GameObject npc;
    public Vector3 npcSpawnOffset;
    public CrowdPlayerController crowdPlayerController;
    public Transform spawnPoint;
    public Transform npcContainer;

    public void SetCorrectReferences(CharacterController _controller, int _npcCount, GameObject _npc, Vector3 _npcSpawnOffset, CrowdPlayerController _crowdPlayerController/*, Transform spawnPoint, Transform npcContainer*/)
    {
        controller = _controller;
        npcCount = _npcCount;
        npc = _npc;
        npcSpawnOffset = _npcSpawnOffset;
        crowdPlayerController = _crowdPlayerController;
        // this.spawnPoint = spawnPoint;
        // this.npcContainer = npcContainer;

        // npc.transform.SetParent(npcContainer);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCrowdServerRpc(ulong _clientID)
    {
        if(!ClientServerRefs.instance.isServer) return;
        // Transform npcContainer = controller.transform.parent.transform.GetChild(3); // Empty game object to store the npcs
        Transform npcContainer = transform;
        Transform spawnPoint = transform.GetChild(0).GetChild(5);
        // Transform spawnPoint = transform.Find("NPC Spawnpoint");

        if(npcContainer == null) 
        {
            Debug.LogError("No npc container found!");
            return;
        }
        else 
        {
            Debug.Log($"npc container -> {npcContainer.name}");
        }

        if(spawnPoint == null) 
        {
            Debug.LogError("No npc spawnPoint found!");
            return;
        }
        else 
        {
            Debug.Log($"npc spawnPoint -> {spawnPoint.name}");
        }

        for(int i = 0; i < npcCount; i++)
        {
            GameObject newNPC = MGameManager.instance.InstantiatePrefab(npc, spawnPoint.position + npcSpawnOffset, npc.transform.rotation, npcContainer);
            newNPC.transform.SetParent(npcContainer, true);
            NetworkObject newNPCInstance = newNPC.GetComponent<NetworkObject>();

            newNPCInstance.Spawn();
            newNPCInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
            newNPCInstance.transform.SetParent(npcContainer);
            
            NotifyClientOfSpawnClientRpc(newNPCInstance.NetworkObjectId);
        }

        if(npcContainer.childCount <= 0) 
        {
            Debug.LogError("Couldn't place the npcs in the npc container");
        }
        else 
        {
            Debug.Log($"npc container count -> {npcContainer.childCount}");
        }
    }

    [ClientRpc]
    void NotifyClientOfSpawnClientRpc(ulong spawnedObjectId)
    {
        Debug.Log("add object to list client rpc");
        // Find the spawned object by ID
        NetworkObject spawnedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[spawnedObjectId];
        Debug.Log("Spawned Object: " + spawnedObject.gameObject.name);

        // Add it to the client's list
        crowdPlayerController.npcs.Add(spawnedObject.gameObject);
    }


    // [ServerRpc(RequireOwnership = false)]
    // public void SpawnCrowdServerRpc(ulong _clientID)
    // {
    //     if(!ClientServerRefs.instance.isServer) return;
    //     Transform npcContainer = controller.transform.parent; // Empty game object to store the npcs
    //     Transform npcArea = controller.transform.Find("NPC Spawnpoint");

    //     Debug.Log("npc container found: " + npcContainer.name);
    //     Debug.Log("npc area found: " + npcArea.name);

    //     for(int i = 0; i < npcCount; i++)
    //     {
    //         GameObject newNPC = MGameManager.instance.InstantiatePrefab(npc, npcArea.position + npcSpawnOffset, npc.transform.rotation, npcContainer);
    //         NetworkObject newNPCInstance = newNPC.GetComponent<NetworkObject>();
    //         newNPCInstance.Spawn();
    //         newNPCInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
    //         newNPCInstance.transform.SetParent(npcContainer);

    //         NotifyClientOfSpawnClientRpc(newNPCInstance.NetworkObjectId);
    //     }
    // }
}
