using System.Collections;
using UnityEngine;

public class NPCFollower
{
    [Header("References")]
    public Transform target;
    private Transform transform;
    private CharacterController controller;
    private CrowdPlayerManager Player;

    [Header("Movement Parameters")]
    public float smoothSpeed = 5f;
    public float stoppingThreshold = 0.5f;
    public float rotationSpeed = 10f;
    Vector3 previousPlayerPos = Vector3.zero;
    
    [Header("Positioning Parameters")]
    public float minDistanceBehindTarget = 3f;
    public float maxDistanceBehindTarget = 6f;
    public float minNPCDistance = .5f;
    public float maxNPCDistance = 2f;
    public float spreadFactor = 1f;
    public float fixedYPosition;

    // Personal space parameters
    public float personalSpaceRadius = 2f;
    public float personalSpaceStrength = 3f;

    private Vector3 targetPosition;
    private Vector3 lastValidPosition;
    private bool isStuck = false;
    private float stuckTimer = 0f;
    private readonly float stuckTimeThreshold = 0.8f;
    private Vector3 velocity;
    private readonly float gravity = 9.81f;

    // Smoothing parameters
    public Vector3 currentVelocity = Vector3.zero;
    private const float smoothTime = 0.5f;
    private float velocitySmoothTime = 0.2f;

    // Flocking parameters
    private Vector3 centerOfMass;
    public int npcIndex;

    // Formation parameters
    private NPCFormationManager formationManager;
    private bool useFormation = false;

    // Delayed repositioning parameters
    private float repositionTimer = 0f;
    private readonly float movementSpeed;
    private readonly float repositionInterval = 2f;

    private float movementSpeedMultiplier;

    public NPCFollower
    (
        Transform transform,
        float smoothSpeed,
        float stoppingThreshold,
        float minDistanceBehindTarget,
        float maxDistanceBehindTarget,
        float minNPCDistance,
        float maxNPCDistance,
        float spreadFactor,
        float fixedYPosition,
        float movementSpeed
    ) 
    {
        this.transform = transform;
        this.smoothSpeed = smoothSpeed;
        this.stoppingThreshold = stoppingThreshold;
        this.minDistanceBehindTarget = minDistanceBehindTarget;
        this.maxDistanceBehindTarget = maxDistanceBehindTarget;
        this.minNPCDistance = minNPCDistance;
        this.maxNPCDistance = maxNPCDistance;
        this.spreadFactor = spreadFactor;
        this.fixedYPosition = fixedYPosition;
        this.lastValidPosition = transform.position;
        this.movementSpeed = movementSpeed;

        movementSpeedMultiplier = Random.Range(0.8f, 1.2f);
    }

    public void Start() 
    {
        controller = transform.GetChild(0).GetComponent<CharacterController>();
        
        // Initialize with a random offset to prevent bunching at startup
        Random.InitState(System.DateTime.Now.Millisecond + npcIndex * 100);
        currentVelocity = Vector3.zero;

        movementSpeedMultiplier = Random.Range(0.8f, 1.2f);
    }

    public IEnumerator FindPlayer(NPCManager npc) 
    {
        yield return new WaitForSeconds(.5f);

        // Ensure target is set
        if (target == null)
        {
            if(npc != null) 
            {
                target = npc.transform.parent.GetChild(0).transform;
                Player = target.GetComponentInParent<CrowdPlayerManager>();
            }
            else 
            {
                Debug.LogError("No npc found!");
            }
        }

        formationManager = target.parent.GetComponent<NPCFormationManager>();

        // Get index in NPC list for unique positioning
        CrowdPlayerManager player = target.parent.GetComponent<CrowdPlayerManager>();
        if(player.playerController.npcs != null) 
        {
            npcIndex = player.playerController.npcs.IndexOf(npc.gameObject);
        }
    }

    public void Update(NPCManager npc)
    {
        if (target == null || MGameManager.instance.allNPCs == null) return;

        // Calculate center of mass for all NPCs
        CalculateCenterOfMass();
        
        // Increment reposition timer
        repositionTimer += Time.deltaTime;
        
        // Check if we should use formation positioning
        if (formationManager != null && formationManager.currentFormation != FormationType.Follow)
        {
            useFormation = true;
            UpdateFormationTargetPosition(target);
        }
        else
        {
            useFormation = false;

            // Determine if player is moving
            bool playerIsMoving = IsPlayerMoving();
            
            // If player is stopped, completely stop or occasionally reposition
            if (!playerIsMoving || repositionTimer >= repositionInterval)
            {
                repositionTimer = 0f;
                LockToStationaryPosition(npc);
            }
            else 
            {
                UpdateMovingTargetPosition(npc);
                ApplyMovement();
            }

            previousPlayerPos = target.position;
        }

        // Check if NPC is stuck
        CheckIfStuck();
        
        // Apply gravity
        ApplyGravity();
        
        // Update last valid position if not stuck
        if (!isStuck)
        {
            lastValidPosition = controller.transform.position;
        }
    }

    private bool IsPlayerMoving()
    {
        const float movementThreshold = 0.05f;
        return Vector3.Distance(previousPlayerPos, target.position) > movementThreshold;
    }

    private void LockToStationaryPosition(NPCManager npc)
    {
        // Completely stop all movement
        currentVelocity = Vector3.zero;
        targetPosition = controller.transform.position;
        
        // Optional: Minimal alignment with player if very close
        if (Vector3.Distance(controller.transform.position, target.position) < minDistanceBehindTarget * 1.5f)
        {
            Vector3 toPlayer = (target.position - controller.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(toPlayer.x, 0, toPlayer.z));
            controller.transform.rotation = Quaternion.Slerp(
                controller.transform.rotation, 
                targetRotation, 
                Time.deltaTime * rotationSpeed * 0.1f
            );
        }
    }

    private void ApplyGravity() 
    {
        // If controller not grounded
        if(!controller.isGrounded) velocity.y -= gravity * Time.deltaTime;
        else if(InputActionHandler.IsJumping()) velocity.y = Player.jumpForce * 1.5f;
        	
        // Move controller
        controller.Move(velocity * Time.deltaTime);
    }

    void CheckIfStuck()
    {
        float movementMagnitude = currentVelocity.magnitude;
        
        // If velocity is very low but we're not at the target position
        if (movementMagnitude < 0.1f && Vector3.Distance(controller.transform.position, targetPosition) > 1.0f)
        {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer > stuckTimeThreshold)
            {
                isStuck = true;
                
                // Try to find an alternate path
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                targetPosition = controller.transform.position + randomDirection * 2f;
            }
        }
        else
        {
            stuckTimer = 0f;
            isStuck = false;
        }
    }

    void ApplyMovement()
    {
        // Only apply movement if target position is significantly different
        if (Vector3.Distance(controller.transform.position, targetPosition) > 0.01f)
        {
            // Calculate the desired movement
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                controller.transform.position, 
                targetPosition, 
                ref currentVelocity, 
                smoothTime, 
                smoothSpeed
            );

            // Calculate movement delta
            Vector3 movementDelta = smoothedPosition - controller.transform.position;

            // Only apply Y position based on target if we're not using the fixed Y
            if (fixedYPosition <= 0)
            {
                movementDelta.y = target.position.y - controller.transform.position.y;
            }
            else
            {
                movementDelta.y = fixedYPosition - controller.transform.position.y;
            }

            // Move the character using the controller
            controller.Move(movementSpeed * Time.deltaTime * movementDelta);

            // Handle rotation smoothly to face movement direction
            if (movementDelta.magnitude > 0.05f)
            {
                // Create a rotation that looks in the direction of movement
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movementDelta.x, 0, movementDelta.z));
                
                // Smoothly rotate towards that direction
                controller.transform.rotation = Quaternion.Lerp(
                    controller.transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeed
                );
            }
        }
        else
        {
            // When extremely close to target, zero out velocity
            currentVelocity = Vector3.zero;
        }
    }

    void UpdateFormationTargetPosition(Transform target)
    {
        ApplyMovement();

        CrowdPlayerManager playerManager = target.parent.GetComponent<CrowdPlayerManager>();
        
        // Get the desired position in the formation
        int totalNPCs = MGameManager.instance.allNPCs.Count;
        Vector3 formationPos = formationManager.GetFormationPosition(npcIndex, totalNPCs);
        
        // If a valid formation position was returned
        if (formationPos != Vector3.zero)
        {
            targetPosition = formationPos;
            
            if (fixedYPosition <= 0)
            {
                targetPosition.y = target.position.y;
            }
            else
            {
                targetPosition.y = fixedYPosition;
            }
            
            if (useFormation)
            {
                // Add minimal avoidance to prevent exact overlap if NPCs are crowded
                Vector3 avoidanceVector = Vector3.zero;
                
                foreach (var otherNPC in playerManager.playerController.npcs)
                {
                    if (otherNPC == null || otherNPC.transform == transform) continue;
                    
                    float distance = Vector3.Distance(transform.position, otherNPC.transform.GetChild(0).position);
                    
                    // Only apply minimal avoidance in formation
                    if (distance < minNPCDistance * 0.8f)
                    {
                        Vector3 separationVector = controller.transform.position - otherNPC.transform.GetChild(0).position;
                        float repulsionStrength = Mathf.Pow(1f - (distance / (minNPCDistance * 0.8f)), 2) * 3f;
                        avoidanceVector += separationVector.normalized * repulsionStrength;
                    }
                }

                // Apply reduced avoidance
                targetPosition += avoidanceVector * 0.3f;
                
                Debug.DrawLine(controller.transform.position, targetPosition, Color.green);
                
                // If we've reached the formation position, gradually reduce velocity
                if (formationManager.staticFormations && 
                    Vector3.Distance(controller.transform.position, targetPosition) < 0.2f)
                {
                    currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * 5f);
                }
            }
        }
    }

    void CalculateCenterOfMass()
    {
        // Calculate the center of mass of all NPCs
        centerOfMass = Vector3.zero;
        int activeNPCs = 0;

        foreach (NPCManager npc in MGameManager.instance.allNPCs)
        {
            if (npc != null)
            {
                centerOfMass += npc.transform.GetChild(0).position;
                activeNPCs++;
            }
        }

        if (activeNPCs > 0)
        {
            centerOfMass /= activeNPCs;
        }
    }

    public void UpdateMovingTargetPosition(NPCManager npc)
    {
        // Calculate base position behind target with more natural spacing
        Vector3 targetForward = target.forward;
        Vector3 targetRight = Vector3.Cross(Vector3.up, targetForward).normalized;

        // Randomize base distance with less geometric precision
        float baseDistanceVariation = Random.Range(-1f, 1f);
        float distanceBehind = Mathf.Lerp(
            minDistanceBehindTarget, 
            maxDistanceBehindTarget, 
            0.5f + baseDistanceVariation * 0.3f
        );
        
        // Base position slightly behind and offset from target
        Vector3 basePosition = target.position 
            - targetForward * distanceBehind 
            + targetRight * Random.Range(-minNPCDistance, minNPCDistance);
        
        // Set target position with more organic positioning
        targetPosition = basePosition;

        // Add some small vertical and horizontal jitter
        float jitterX = Mathf.PerlinNoise(npcIndex * 0.1f, Time.time * 0.2f) - 0.5f;
        float jitterZ = Mathf.PerlinNoise(npcIndex * 0.1f + 100, Time.time * 0.2f) - 0.5f;
        targetPosition += targetRight * (jitterX * minNPCDistance * 0.5f) 
                        + targetForward * (jitterZ * minNPCDistance * 0.5f);

        // Ensure minimum distance from other NPCs
        targetPosition += CalculateAvoidanceVector(npc) * 1.2f;
        
        // Ensure minimum distance from player
        Vector3 toPlayer = controller.transform.position - target.position;
        float playerDistance = toPlayer.magnitude;
        if (playerDistance < minDistanceBehindTarget * 0.8f)
        {
            targetPosition += toPlayer.normalized * (minDistanceBehindTarget * 0.8f - playerDistance);
        }
        
        // Set Y position
        if (fixedYPosition <= 0)
        {
            targetPosition.y = target.position.y;
        }
        else
        {
            targetPosition.y = fixedYPosition;
        }
    }

    private Vector3 CalculateAvoidanceVector(NPCManager npc)
    {
        Vector3 avoidanceSum = Vector3.zero;
        
        // Avoid other NPCs
        foreach (NPCManager otherNPC in MGameManager.instance.allNPCs)
        {
            if (otherNPC == null || otherNPC == npc) continue;
            
            Vector3 otherPos = otherNPC.transform.GetChild(0).position;
            float distance = Vector3.Distance(controller.transform.position, otherPos);
            
            // Strong repulsion when too close
            if (distance < minNPCDistance)
            {
                Vector3 separationVector = controller.transform.position - otherPos;
                if (separationVector.magnitude > 0.001f) // Prevent normalization of zero vector
                {
                    // Exponential repulsion force that increases as distance decreases
                    float repulsionStrength = Mathf.Pow(1f - (distance / minNPCDistance), 2) * personalSpaceStrength;
                    avoidanceSum += separationVector.normalized * repulsionStrength;
                }
            }
            
            // Mild attraction to maintain group cohesion when too far
            if (distance > maxNPCDistance)
            {
                Vector3 attractionVector = otherPos - controller.transform.position;
                float attractionStrength = (distance - maxNPCDistance) * 0.05f;
                avoidanceSum += attractionVector.normalized * attractionStrength;
            }
        }
        
        // Avoid getting too close to player (stronger priority)
        float playerDistance = Vector3.Distance(controller.transform.position, target.position);
        if (playerDistance < minDistanceBehindTarget)
        {
            Vector3 awayFromPlayer = controller.transform.position - target.position;
            if (awayFromPlayer.magnitude > 0.001f)
            {
                float repulsionStrength = (1f - (playerDistance / minDistanceBehindTarget)) * personalSpaceStrength * 1.5f;
                avoidanceSum += awayFromPlayer.normalized * repulsionStrength;
            }
        }
        
        // Apply a slight pull toward the center of mass to maintain group cohesion
        Vector3 toCenterOfMass = centerOfMass - controller.transform.position;
        float distanceToCenterOfMass = toCenterOfMass.magnitude;
        if (distanceToCenterOfMass > maxNPCDistance * 1.5f)
        {
            avoidanceSum += toCenterOfMass.normalized * 0.2f;
        }
        
        // Limit maximum avoidance force to prevent extreme movements
        if (avoidanceSum.magnitude > 3f)
        {
            avoidanceSum = avoidanceSum.normalized * 3f;
        }
        
        return avoidanceSum;
    }
}


// using System.Collections;
// using UnityEngine;

// public class NPCFollower
// {
//     [Header("References")]
//     public Transform target;
//     private CrowdPlayerManager Player;
//     public Transform transform;
//     private CharacterController controller;

//     [Header("Movement Parameters")]
//     public float smoothSpeed = 5f;
//     public float stoppingThreshold = 0.5f;
//     public float rotationSpeed = 10f;
//     Vector3 previousPlayerPos = Vector3.zero;
    
//     [Header("Positioning Parameters")]
//     public float minDistanceBehindTarget = 3f;
//     public float maxDistanceBehindTarget = 6f;
//     public float minNPCDistance = 1.5f;
//     public float maxNPCDistance = 4f;
//     public float spreadFactor = 1.5f;
//     public float fixedYPosition;

//     // Personal space parameters
//     public float personalSpaceRadius = 2f;
//     public float personalSpaceStrength = 3f;

//     private Vector3 targetPosition;
//     private Vector3 lastValidPosition;
//     private bool isStuck = false;
//     private float stuckTimer = 0f;
//     private readonly float stuckTimeThreshold = 0.8f;
//     private Vector3 velocity;
//     private readonly float gravity = 9.81f;

//     // Smoothing parameters
//     public Vector3 currentVelocity;
//     private const float smoothTime = 0.5f; // Increased for smoother movement
//     private float velocitySmoothTime = 0.2f;

//     // Flocking parameters
//     private Vector3 centerOfMass;
//     public int npcIndex;

//     // Formation parameters
//     private NPCFormationManager formationManager;
//     private bool useFormation = false;

//     // Delayed repositioning parameters
//     private float repositionTimer = 0f;
//     private readonly float movementSpeed;
//     private readonly float repositionInterval = 2f;

//     public NPCFollower
//     (
//         Transform transform,
//         float smoothSpeed,
//         float stoppingThreshold,
//         float minDistanceBehindTarget,
//         float maxDistanceBehindTarget,
//         float minNPCDistance,
//         float maxNPCDistance,
//         float spreadFactor,
//         float fixedYPosition,
//         float movementSpeed
//     ) 
//     {
//         this.transform = transform;
//         this.smoothSpeed = smoothSpeed;
//         this.stoppingThreshold = stoppingThreshold;
//         this.minDistanceBehindTarget = minDistanceBehindTarget;
//         this.maxDistanceBehindTarget = maxDistanceBehindTarget;
//         this.minNPCDistance = minNPCDistance;
//         this.maxNPCDistance = maxNPCDistance;
//         this.spreadFactor = spreadFactor;
//         this.fixedYPosition = fixedYPosition;
//         this.lastValidPosition = transform.position;
//         this.movementSpeed = movementSpeed;
//     }

//     public void Start() 
//     {
//         // Debug.Log($"child transform: {transform.GetChild(0)}");
//         controller = transform.GetChild(0).GetComponent<CharacterController>();
        
//         // Initialize with a random offset to prevent bunching at startup
//         Random.InitState(System.DateTime.Now.Millisecond + npcIndex * 100);
//         currentVelocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 0.5f;
//     }

//     public IEnumerator FindPlayer(NPCManager npc) 
//     {
//         yield return new WaitForSeconds(.5f);

//         // Ensure target is set
//         if (target == null)
//         {
//             if(npc != null) 
//             {
//                 target = npc.transform.parent.GetChild(0).transform;
//                 Player = target.GetComponent<CrowdPlayerManager>();
//             }
//             else 
//             {
//                 Debug.LogError("No npc found!");
//             }
//         }

//         formationManager = target.parent.GetComponent<NPCFormationManager>();

//         // Get index in NPC list for unique positioning
//         CrowdPlayerManager player = target.parent.GetComponent<CrowdPlayerManager>();
//         if(player.playerController.npcs != null) 
//         {
//             npcIndex = player.playerController.npcs.IndexOf(npc.gameObject);
//         }
//     }

//     public void Update(NPCManager npc)
//     {
//         if (target == null || MGameManager.instance.allNPCs == null) return;

//         // Calculate center of mass for all NPCs
//         CalculateCenterOfMass();
        
//         // Increment reposition timer
//         repositionTimer += Time.deltaTime;
        
//         // Check if we should use formation positioning
//         if (formationManager != null && formationManager.currentFormation != FormationType.Follow)
//         {
//             useFormation = true;
//             UpdateFormationTargetPosition(target);
//         }
//         else
//         {
//             useFormation = false;

//             float distanceToTarget = Vector3.Distance(target.position, controller.transform.position);
//             float playerMovement = Vector3.Distance(previousPlayerPos, target.position);

//             // Check if player has moved significantly
//             bool playerIsMoving = playerMovement > 0.05f;
            
//             // If player is stopped or moving very slowly, occasionally reposition NPCs to prevent crowding
//             if (!playerIsMoving || repositionTimer >= repositionInterval)
//             {
//                 repositionTimer = 0f;
//                 UpdateStoppedTargetPosition();
//             }
//             else 
//             {
//                 UpdateMovingTargetPosition(npc);
//             }

//             previousPlayerPos = target.position;
//         }

//         // Check if NPC is stuck
//         CheckIfStuck();

//         // Apply movement
//         ApplyMovement(npc);
//         ApplyGravity();
        
//         // Update last valid position if not stuck
//         if (!isStuck)
//         {
//             lastValidPosition = controller.transform.position;
//         }
//     }

//     private void ApplyGravity() 
//     {
//         // If controller not grounded
//         if(!controller.isGrounded) velocity.y -= gravity * Time.deltaTime;
//         else if(InputActionHandler.IsJumping()) velocity.y = Player.jumpForce * 1.5f;
        	
//         // Move controller
//         controller.Move(velocity * Time.deltaTime);
//     }

//     void CheckIfStuck()
//     {
//         float movementMagnitude = currentVelocity.magnitude;
        
//         // If velocity is very low but we're not at the target position
//         if (movementMagnitude < 0.1f && Vector3.Distance(controller.transform.position, targetPosition) > 1.0f)
//         {
//             stuckTimer += Time.deltaTime;
            
//             if (stuckTimer > stuckTimeThreshold)
//             {
//                 isStuck = true;
                
//                 // Try to find an alternate path
//                 Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
//                 targetPosition = controller.transform.position + randomDirection * 2f;
//             }
//         }
//         else
//         {
//             stuckTimer = 0f;
//             isStuck = false;
//         }
//     }

//     void ApplyMovement(NPCManager npc)
//     {
//         // Calculate the desired movement
//         Vector3 smoothedPosition = Vector3.SmoothDamp(
//             controller.transform.position, 
//             targetPosition, 
//             ref currentVelocity, 
//             smoothTime, 
//             smoothSpeed
//         );

//         // Calculate movement delta
//         Vector3 movementDelta = smoothedPosition - controller.transform.position;

//         // Only apply Y position based on target if we're not using the fixed Y
//         if (fixedYPosition <= 0)
//         {
//             movementDelta.y = target.position.y - controller.transform.position.y;
//         }
//         else
//         {
//             movementDelta.y = fixedYPosition - controller.transform.position.y;
//         }

//         // Move the character using the controller
//         controller.Move(movementDelta * Time.deltaTime * movementSpeed);

//         // Handle rotation smoothly to face movement direction
//         if (movementDelta.magnitude > 0.05f)
//         {
//             // Create a rotation that looks in the direction of movement
//             Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movementDelta.x, 0, movementDelta.z));
            
//             // Smoothly rotate towards that direction
//             controller.transform.rotation = Quaternion.Lerp(
//                 controller.transform.rotation,
//                 targetRotation,
//                 Time.deltaTime * rotationSpeed
//             );
//         }
//         else if (Vector3.Distance(controller.transform.position, target.position) < minDistanceBehindTarget * 1.5f)
//         {
//             // If close to the player and not moving much, look at the player
//             Vector3 lookDirection = target.position - controller.transform.position;
//             if (lookDirection.magnitude > 0.1f)
//             {
//                 Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
//                 controller.transform.rotation = Quaternion.Lerp(
//                     controller.transform.rotation,
//                     lookRotation,
//                     Time.deltaTime * rotationSpeed * 0.5f
//                 );
//             }
//         }
//     }

//     void UpdateFormationTargetPosition(Transform target)
//     {
//         CrowdPlayerManager playerManager = target.parent.GetComponent<CrowdPlayerManager>();
        
//         // Get the desired position in the formation
//         int totalNPCs = MGameManager.instance.allNPCs.Count;
//         Vector3 formationPos = formationManager.GetFormationPosition(npcIndex, totalNPCs);
        
//         // If a valid formation position was returned
//         if (formationPos != Vector3.zero)
//         {
//             targetPosition = formationPos;
            
//             if (fixedYPosition <= 0)
//             {
//                 targetPosition.y = target.position.y;
//             }
//             else
//             {
//                 targetPosition.y = fixedYPosition;
//             }
            
//             if (useFormation)
//             {
//                 // Add minimal avoidance to prevent exact overlap if NPCs are crowded
//                 Vector3 avoidanceVector = Vector3.zero;
                
//                 foreach (var otherNPC in playerManager.playerController.npcs)
//                 {
//                     if (otherNPC == null || otherNPC.transform == transform) continue;
                    
//                     float distance = Vector3.Distance(transform.position, otherNPC.transform.GetChild(0).position);
                    
//                     // Only apply minimal avoidance in formation
//                     if (distance < minNPCDistance * 0.8f)
//                     {
//                         Vector3 separationVector = controller.transform.position - otherNPC.transform.GetChild(0).position;
//                         float repulsionStrength = Mathf.Pow(1f - (distance / (minNPCDistance * 0.8f)), 2) * 3f;
//                         avoidanceVector += separationVector.normalized * repulsionStrength;
//                     }
//                 }

//                 // Apply reduced avoidance
//                 targetPosition += avoidanceVector * 0.3f;
                
//                 Debug.DrawLine(controller.transform.position, targetPosition, Color.green);
                
//                 // If we've reached the formation position, gradually reduce velocity
//                 if (formationManager.staticFormations && 
//                     Vector3.Distance(controller.transform.position, targetPosition) < 0.2f)
//                 {
//                     currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * 5f);
//                 }
//             }
//         }
//     }

//     void CalculateCenterOfMass()
//     {
//         // Calculate the center of mass of all NPCs
//         centerOfMass = Vector3.zero;
//         int activeNPCs = 0;

//         foreach (NPCManager npc in MGameManager.instance.allNPCs)
//         {
//             if (npc != null)
//             {
//                 centerOfMass += npc.transform.GetChild(0).position;
//                 activeNPCs++;
//             }
//         }

//         if (activeNPCs > 0)
//         {
//             centerOfMass /= activeNPCs;
//         }
//     }

//     public void UpdateMovingTargetPosition(NPCManager npc)
//     {
//         Vector3 targetForward = target.forward;
//         Vector3 targetRight = Vector3.Cross(Vector3.up, targetForward).normalized;

//         // Seed random with consistent value for this NPC
//         Random.InitState(npcIndex * 1000);

//         // Calculate a unique angle for this NPC based on index
//         float totalNPCs = Mathf.Max(1, MGameManager.instance.allNPCs.Count);
//         float angleStep = 360f / totalNPCs;
//         float angle = npcIndex * angleStep + (Time.time * 0.2f) % 360f; // Slight rotation over time
        
//         // Convert angle to radians
//         float angleRad = angle * Mathf.Deg2Rad;
        
//         // Calculate distance based on NPC index with some variance
//         float distanceMultiplier = 0.8f + (npcIndex % 3) * 0.2f; // Creates layers of followers
//         float distanceBehind = Mathf.Lerp(minDistanceBehindTarget, maxDistanceBehindTarget, 
//             distanceMultiplier);
        
//         // Base position behind target
//         Vector3 basePosition = target.position - targetForward * distanceBehind;
        
//         // Add circular distribution with radius based on NPC count
//         float radius = minNPCDistance * spreadFactor * (1f + (totalNPCs / 10f));
//         float xOffset = Mathf.Sin(angleRad) * radius;
//         float zOffset = Mathf.Cos(angleRad) * radius;
        
//         // Apply offset using target's coordinate system
//         targetPosition = basePosition 
//             + targetRight * xOffset 
//             + targetForward * zOffset;

//         // Ensure minimum distance from other NPCs
//         targetPosition += CalculateAvoidanceVector(npc) * 1.2f;
        
//         // Ensure minimum distance from player
//         Vector3 toPlayer = controller.transform.position - target.position;
//         float playerDistance = toPlayer.magnitude;
//         if (playerDistance < minDistanceBehindTarget * 0.8f)
//         {
//             targetPosition += toPlayer.normalized * (minDistanceBehindTarget * 0.8f - playerDistance);
//         }
        
//         // Set Y position
//         if (fixedYPosition <= 0)
//         {
//             targetPosition.y = target.position.y;
//         }
//         else
//         {
//             targetPosition.y = fixedYPosition;
//         }
//     }

//     private void UpdateStoppedTargetPosition()
//     {
//         // Target's forward and right vectors
//         Vector3 targetForward = target.forward;
//         Vector3 targetRight = Vector3.Cross(Vector3.up, targetForward).normalized;

//         // Define parameters for semicircular formation when stopped
//         float totalNPCs = Mathf.Max(1, MGameManager.instance.allNPCs.Count);
//         float angleRange = 160f; // Degrees spread for the semicircle
//         float baseAngle = -angleRange / 2f;
//         float angleStep = angleRange / totalNPCs;
        
//         // Position each NPC along the semicircle
//         float angle = baseAngle + (npcIndex * angleStep);
//         float angleRad = angle * Mathf.Deg2Rad;
        
//         // Base distance adjusted by NPC count
//         float baseDistance = Mathf.Lerp(minDistanceBehindTarget, maxDistanceBehindTarget, 
//             0.5f + 0.5f * (totalNPCs / 10f));
        
//         // Calculate base position behind player
//         Vector3 basePosition = target.position - targetForward * baseDistance;
        
//         // Apply semicircular distribution
//         float radius = minNPCDistance * spreadFactor * (1.5f + (totalNPCs / 15f));
//         float xOffset = Mathf.Sin(angleRad) * radius;
//         float zOffset = -Mathf.Abs(Mathf.Cos(angleRad) * radius); // Negative to keep behind
        
//         // Calculate position in world space
//         targetPosition = basePosition 
//             + targetRight * xOffset 
//             + targetForward * zOffset;
        
//         // Add small random variation to prevent perfect alignment
//         Vector2 randomOffset = new Vector2(
//             Mathf.PerlinNoise(npcIndex * 0.5f, Time.time * 0.1f) - 0.5f,
//             Mathf.PerlinNoise(npcIndex * 0.5f + 100, Time.time * 0.1f) - 0.5f
//         ) * minNPCDistance * 0.5f;
        
//         targetPosition += targetRight * randomOffset.x + targetForward * randomOffset.y;
        
//         // Set Y position
//         if (fixedYPosition <= 0)
//         {
//             targetPosition.y = target.position.y;
//         }
//         else
//         {
//             targetPosition.y = fixedYPosition;
//         }
//     }

//     private Vector3 CalculateAvoidanceVector(NPCManager npc)
//     {
//         Vector3 avoidanceSum = Vector3.zero;
        
//         // Avoid other NPCs
//         foreach (NPCManager otherNPC in MGameManager.instance.allNPCs)
//         {
//             if (otherNPC == null || otherNPC == npc) continue;
            
//             Vector3 otherPos = otherNPC.transform.GetChild(0).position;
//             float distance = Vector3.Distance(controller.transform.position, otherPos);
            
//             // Strong repulsion when too close
//             if (distance < minNPCDistance)
//             {
//                 Vector3 separationVector = controller.transform.position - otherPos;
//                 if (separationVector.magnitude > 0.001f) // Prevent normalization of zero vector
//                 {
//                     // Exponential repulsion force that increases as distance decreases
//                     float repulsionStrength = Mathf.Pow(1f - (distance / minNPCDistance), 2) * personalSpaceStrength;
//                     avoidanceSum += separationVector.normalized * repulsionStrength;
//                 }
//             }
            
//             // Mild attraction to maintain group cohesion when too far
//             if (distance > maxNPCDistance)
//             {
//                 Vector3 attractionVector = otherPos - controller.transform.position;
//                 float attractionStrength = (distance - maxNPCDistance) * 0.05f;
//                 avoidanceSum += attractionVector.normalized * attractionStrength;
//             }
//         }
        
//         // Avoid getting too close to player (stronger priority)
//         float playerDistance = Vector3.Distance(controller.transform.position, target.position);
//         if (playerDistance < minDistanceBehindTarget)
//         {
//             Vector3 awayFromPlayer = controller.transform.position - target.position;
//             if (awayFromPlayer.magnitude > 0.001f)
//             {
//                 float repulsionStrength = (1f - (playerDistance / minDistanceBehindTarget)) * personalSpaceStrength * 1.5f;
//                 avoidanceSum += awayFromPlayer.normalized * repulsionStrength;
//             }
//         }
        
//         // Apply a slight pull toward the center of mass to maintain group cohesion
//         Vector3 toCenterOfMass = centerOfMass - controller.transform.position;
//         float distanceToCenterOfMass = toCenterOfMass.magnitude;
//         if (distanceToCenterOfMass > maxNPCDistance * 1.5f)
//         {
//             avoidanceSum += toCenterOfMass.normalized * 0.2f;
//         }
        
//         // Limit maximum avoidance force to prevent extreme movements
//         if (avoidanceSum.magnitude > 3f)
//         {
//             avoidanceSum = avoidanceSum.normalized * 3f;
//         }
        
//         return avoidanceSum;
//     }
// }