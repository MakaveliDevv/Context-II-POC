using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ExternallyMovingObject : NetworkBehaviour
{
    CustomNetworkBehaviour customNetworkBehaviour;
    public bool dontRequest = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        customNetworkBehaviour = GetComponent<CustomNetworkBehaviour>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if(customNetworkBehaviour.CustomIsOwner()) customNetworkBehaviour.RequestMoveServerRpc(transform.position, transform.rotation, transform.localScale);
        StartCoroutine(Delay());
        
    }
    
    // void LateUpdate()
    // {
    //     Debug.Log($"[Child Transform] World Pos: {transform.position}, Local Pos: {transform.localPosition}");
    // }

    
    private IEnumerator Delay() 
    {
        yield return new WaitForSeconds(0.1f);
        if(customNetworkBehaviour.CustomIsOwner() && !dontRequest) customNetworkBehaviour.RequestMoveServerRpc(transform.position, transform.rotation, transform.localScale);
    }
}
