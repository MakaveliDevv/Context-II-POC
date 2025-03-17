using Unity.Netcode;
using UnityEngine;

public class CrowdRpcBehaviour : NetworkBehaviour
{
    public CharacterController controller;
    public int npcCount;
    public GameObject npc;
    public Vector3 npcSpawnOffset;
    CrowdPlayerController crowdPlayerController;

    public void SetCorrectReferences(CharacterController _controller, int _npcCount, GameObject _npc, Vector3 _npcSpawnOffset, CrowdPlayerController _crowdPlayerController)
    {
        controller = _controller;
        npcCount = _npcCount;
        npc = _npc;
        npcSpawnOffset = _npcSpawnOffset;
        crowdPlayerController = _crowdPlayerController;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCrowdServerRpc(ulong _clientID)
    {
        if(!ClientServerRefs.instance.isServer) return;
        Transform npcContainer = controller.transform.parent.transform.GetChild(3); // Empty game object to store the npcs
        Transform npcArea = controller.transform.GetChild(1);

        for(int i = 0; i < npcCount; i++)
        {
            GameObject newNPC = MGameManager.instance.InstantiatePrefab(npc, npcArea.position + npcSpawnOffset, npc.transform.rotation, npcContainer);
            NetworkObject newNPCInstance = newNPC.GetComponent<NetworkObject>();
            newNPCInstance.Spawn();
            newNPCInstance.gameObject.GetComponent<CustomNetworkBehaviour>().UpdateClientID(_clientID);
            
            NotifyClientOfSpawnClientRpc(newNPCInstance.NetworkObjectId);
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
}
