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
    CustomNetworkBehaviour customNetworkBehaviour;

    [Header("Movement Parameters")]
    public float smoothSpeed = 5f;
    public float stoppingThreshold = 0.1f;
    public float movementSpeed;
    
    [Header("Positioning Parameters")]
    public float minDistanceBehindTarget = 2f;
    public float maxDistanceBehindTarget = 6f;
    public float minNPCDistance = 1.2f;
    public float maxNPCDistance = 3f;
    public float spreadFactor = 1.5f;
    public float fixedYPosition = 1f;
    public bool moveable;

    // Anim
    private Animator animator;


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
            fixedYPosition,
            movementSpeed
        );

        moveable = true;
    }

    void Start()
    {
        // nPCFollower.Start(this);
        nPCFollower.Start();
        customNetworkBehaviour = GetComponent<CustomNetworkBehaviour>();
        StartCoroutine(nPCFollower.FindPlayer(this));

        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    void Update()
    {
        // nPCPatrol.MoveNPC();
        if(customNetworkBehaviour.CustomIsOwner())
        {
            if(moveable) 
            {
                nPCFollower.Update(this);
            }      
        }
    }

    public void HappyEmote() 
    {
        Debug.Log("Happy Emote");
        animator.SetTrigger("Happy");
    }

    public void SadEmote() 
    {
        Debug.Log("Sad Emote");
        animator.SetTrigger("Sad");
    }

    public void ThrowingEmote() 
    {
        Debug.Log("Throwing Emote");
        animator.SetTrigger("Tomato");
    }
    
    // public void HappyEmote() 
    // {
    //     Debug.Log("Happy Emote");
    //     animator.SetBool("Happy", true);

    //     // Turn off the others
    //     animator.SetBool("Sad", false);
    //     animator.SetBool("Tomato", false);
    // }

    // public void SadEmote() 
    // {
    //     Debug.Log("Sad Emote");
    //     animator.SetBool("Sad", true);

    //     // Turn off the others
    //     animator.SetBool("Happy", false);
    //     animator.SetBool("Tomato", false);
    // }

    // public void ThrowingEmote() 
    // {
    //     Debug.Log("Throwing Emote");

    //     animator.SetBool("Tomato", true);

    //     // Turn off the others
    //     animator.SetBool("Happy", false);
    //     animator.SetBool("Sad", false);
    // }

    public void EmoteNPC(string emoteName) 
    {
        if(emoteName.Contains("Happy")) 
        {
            HappyEmote();
        }
        else if (emoteName.Contains("Sad")) 
        {
            SadEmote();
        }
        else if(emoteName.Contains("Tomato")) 
        {
            ThrowingEmote();
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
        gameObject.transform.GetChild(1).gameObject.SetActive(active);
        SignalClientRpc(active);
    }
    [ClientRpc]
    void SignalClientRpc(bool active)
    {
        Debug.Log("Signal on client");
        gameObject.transform.GetChild(1).gameObject.SetActive(active);
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        
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