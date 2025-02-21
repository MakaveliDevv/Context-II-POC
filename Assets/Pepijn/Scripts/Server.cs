using System;
using Unity.Netcode;
using UnityEngine;

public class Server : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //RequestServerActionServerRpc();
        //PerformActionClientRpc();
        Console.WriteLine("Server starts");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ServerRpc]
    void RequestServerActionServerRpc()
    {
        if(IsServer)
        {
            PerformActionClientRpc();
        }
    }

    [ClientRpc]
    void PerformActionClientRpc()
    {
        Debug.Log("Received action from the server!");
    }
    
}
