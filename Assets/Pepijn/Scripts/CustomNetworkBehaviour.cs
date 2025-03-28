using Unity.Netcode;
using UnityEngine;

public class CustomNetworkBehaviour : NetworkBehaviour
{
    public NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    public ulong ownerClientID;
    void Awake()
    {
        ownerClientID = 1000;   
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveServerRpc(Vector3 _position, Quaternion _rotation, Vector3 _localScale)
    {
        netPosition.Value = _position;
        Debug.Log($"Reqeusting move on server: {gameObject.name} to {_position}");
        // Set the position on the server
        transform.SetPositionAndRotation(_position, _rotation);
        transform.localScale =_localScale;

        UpdatePositionClientRpc(_position, _rotation, _localScale);
    }

    [ClientRpc]
    public void UpdatePositionClientRpc(Vector3 _position, Quaternion _rotation, Vector3 _localScale)
    {
        if(!CustomIsOwner())
        {
            transform.position = netPosition.Value;
            // Set the position on the server
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
        // Debug.Log("Updated Client ID on client: " + ownerClientID);
    }

    public bool CustomIsOwner()
    {
        // Debug.Log("owner ID: " + ownerClientID + ", local ID: " + NetworkManager.Singleton.LocalClientId);
        if( NetworkManager.Singleton == null) return false;
        if(ownerClientID == NetworkManager.Singleton.LocalClientId) return true;
        else return false;
    }
}
