using System.Collections;
using Unity.Netcode;
using UnityEngine;
public class NPCManager : NetworkBehaviour
{   
    // private NPCPatrol nPCPatrol;
    // [SerializeField] private LayerMask layerMask;
    // [SerializeField] private float patrolSpeed = 2f;
    // [SerializeField] private float obstacleAvoidanceDistance = 1.5f;
    // [SerializeField] private float raycastLength = 2f;
    // [SerializeField] private float spacing = 1f;
    
    // Visual debugging
    // [SerializeField] private bool showDebugVisuals = true;

    // NPC Follow
    public NPCFollower nPCFollower;

    [Header("Movement Parameters")]
    public float smoothSpeed = 5f;
    public float stoppingThreshold = 0.1f;
    
    [Header("Positioning Parameters")]
    public float minDistanceBehindTarget = 2f;
    public float maxDistanceBehindTarget = 6f;
    public float minNPCDistance = 1.2f;
    public float maxNPCDistance = 3f;
    public float spreadFactor = 1.5f;
    public float fixedYPosition = 1f;
    public bool moveable;

    public bool signal;

    void Awake()
    {
        MGameManager.instance.allNPCs.Add(this);
       
        // nPCPatrol = new NPCPatrol
        // (
        //     transform,
        //     layerMask,
        //     transform.position, // Start with current position as target
        //     patrolSpeed,
        //     obstacleAvoidanceDistance,
        //     raycastLength,
        //     spacing
        // );

        nPCFollower = new
        (
            transform,
            smoothSpeed,
            stoppingThreshold,
            minDistanceBehindTarget,
            maxDistanceBehindTarget,
            minNPCDistance,
            maxNPCDistance,
            spreadFactor,
            fixedYPosition
        );

        moveable = true;
    }

    void Start()
    {
        nPCFollower.Start(this);
    }

    void Update()
    {
        // nPCPatrol.MoveNPC();
        if(moveable) 
        {
            nPCFollower.Update(this);
        }      
    }

    public IEnumerator Signal(float signalTimer) 
    {
        Debug.Log("Start signaling");
        // Show UI on top of the npc
        //gameObject.transform.GetChild(0).gameObject.SetActive(true);
        SignalServerRpc(true);

        yield return new WaitForSeconds(signalTimer);

        SignalServerRpc(false);
        //gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }


    [ServerRpc(RequireOwnership = false)]
    void SignalServerRpc(bool active)
    {
        Debug.Log("Signal on server");
        gameObject.transform.GetChild(0).gameObject.SetActive(active);
        SignalClientRpc(active);
    }
    [ClientRpc]
    void SignalClientRpc(bool active)
    {
        Debug.Log("Signal on client");
        gameObject.transform.GetChild(0).gameObject.SetActive(active);
    }

    private void Emote() 
    {

    }
    
    private void OnDestroy()
    {
        if (MGameManager.instance != null && MGameManager.instance.allNPCs.Contains(this))
        {
            MGameManager.instance.allNPCs.Remove(this);
        }
        
        // NPCsManagement.UnregisterTriggerMovement(nPCPatrol);
    }

    // void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying) return;

    //     // Draw center of mass
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(centerOfMass, 0.5f);

    //     // Draw NPC position
    //     Gizmos.color = isSpreading ? Color.yellow : Color.green;
    //     Gizmos.DrawWireSphere(transform.position, 0.5f);

    //     // Draw target position
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawSphere(targetPosition, 0.2f);
    // }
}