using Unity.Netcode;
using UnityEngine;

public class ExternallyMovingObject : NetworkBehaviour
{
    CustomNetworkBehaviour customNetworkBehaviour;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        customNetworkBehaviour = GetComponent<CustomNetworkBehaviour>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(customNetworkBehaviour.CustomIsOwner()) customNetworkBehaviour.RequestMoveServerRpc(transform.position, transform.rotation, transform.localScale);
    }
}
