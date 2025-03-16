using Unity.Netcode;
using UnityEngine;

public class CustomNetworkBehaviour : NetworkBehaviour
{
    public ulong ownerClientID;
    void Awake()
    {
        ownerClientID = 1000;   
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveServerRpc(Vector3 _position, Quaternion _rotation, Vector3 _localScale)
    {
        // Set the position on the server
        transform.position = _position;
        transform.rotation = _rotation;
        transform.localScale =_localScale;

        UpdatePositionClientRpc(_position, _rotation, _localScale);
    }

    [ClientRpc]
    public void UpdatePositionClientRpc(Vector3 _position, Quaternion _rotation, Vector3 _localScale)
    {
        if(!CustomIsOwner())
        {
            // Set the position on the server
            transform.position = _position;
            transform.rotation = _rotation;
            transform.localScale =_localScale;
        }
    }

    public void UpdateClientID(ulong _clientID)
    {
        Debug.Log("Update Client ID on server");
        ownerClientID = _clientID;
        UpdateClientIDClientRpc(_clientID);
    }

    [ClientRpc]
    public void UpdateClientIDClientRpc(ulong _clientID)
    {
        ownerClientID = _clientID;
        Debug.Log("Updated Client ID on client: " + ownerClientID);
    }

    public bool CustomIsOwner()
    {
        Debug.Log("owner ID: " + ownerClientID + ", local ID: " + NetworkManager.Singleton.LocalClientId);
        if(ownerClientID == NetworkManager.Singleton.LocalClientId) return true;
        else return false;
    }
}
