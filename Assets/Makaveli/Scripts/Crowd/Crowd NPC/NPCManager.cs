using UnityEngine;
public class NPCManager : MonoBehaviour
{
    public Vector3 TargetPosition => nPCPatrol != null ? nPCPatrol.TargetPosition : Vector3.zero;
   
    private NPCPatrol nPCPatrol;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float obstacleAvoidanceDistance = 1.5f;
    [SerializeField] private float raycastLength = 2f;
    [SerializeField] private float spacing = 1f;
    
    // Visual debugging
    [SerializeField] private bool showDebugVisuals = true;
    
    void Awake()
    {
        MGameManager.instance.allNPCs.Add(this);
       
        nPCPatrol = new NPCPatrol
        (
            transform,
            layerMask,
            transform.position, // Start with current position as target
            patrolSpeed,
            obstacleAvoidanceDistance,
            raycastLength,
            spacing
        );
    }
    
    void Update()
    {
        nPCPatrol.MoveNPC();
        
        // Visual debugging
        if (showDebugVisuals)
        {
            // Show the target position
            Debug.DrawLine(transform.position, TargetPosition, Color.blue);
            // Show a sphere at current position
            Debug.DrawRay(transform.position, Vector3.up * 0.5f, Color.red);
        }
    }
    
    private void OnDestroy()
    {
        if (MGameManager.instance != null && MGameManager.instance.allNPCs.Contains(this))
        {
            MGameManager.instance.allNPCs.Remove(this);
        }
        
        NPCsManagement.UnregisterTriggerMovement(nPCPatrol);
    }
}