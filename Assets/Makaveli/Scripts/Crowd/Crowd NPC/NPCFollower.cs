using UnityEngine;

public class NPCFollower
{
    [Header("References")]
    public Transform target;
    public Transform transform;

    [Header("Movement Parameters")]
    public float smoothSpeed;
    public float stoppingThreshold;
    
    [Header("Positioning Parameters")]
    public float minDistanceBehindTarget;
    public float maxDistanceBehindTarget;
    public float minNPCDistance;
    public float maxNPCDistance;
    public float spreadFactor;
    public float fixedYPosition;

    private Vector3 targetPosition;

    // Smoothing parameters
    public Vector3 currentVelocity;
    private const float smoothTime = 0.3f;

    // Flocking parameters
    private Vector3 centerOfMass;
    public int npcIndex;

    // Formation parameters
    private NPCFormationManager formationManager;
    private bool useFormation = false;

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
        float fixedYPosition
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
    }

    public void Start(NPCManager npc)
    {
        // Ensure target is set
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        formationManager = target.parent.GetComponent<NPCFormationManager>();

        // Get index in NPC list for unique positioning
        if (MGameManager.instance.allNPCs != null)
        {
            npcIndex = MGameManager.instance.allNPCs.IndexOf(npc);
        }
    }

    public void Update(NPCManager npc)
    {
        if (target == null || MGameManager.instance.allNPCs == null) return;

        // Calculate center of mass for all NPCs
        CalculateCenterOfMass();
        
        // Check if we should use formation positioning
        if (formationManager != null && formationManager.currentFormation != FormationType.Follow)
        {
            useFormation = true;
            UpdateFormationTargetPosition(target);
        }
        else
        {
            useFormation = false;
            
            // Use original flocking behavior
            if (target.position.magnitude > 0.2f)
            {
                // Moving phase - tight formation
                UpdateMovingTargetPosition(npc);
            }
            else
            {
                // Just stopped - initiate spreading
                UpdateStoppedTargetPosition();
            }
        }

        // Smooth movement
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime, 
            smoothSpeed
        );

        // Maintain fixed Y position
        smoothedPosition.y = target.position.y;
        transform.position = smoothedPosition;
    }

    void UpdateFormationTargetPosition(Transform target)
    {
        // Get the desired position in the formation
        int totalNPCs = MGameManager.instance.allNPCs.Count;
        Vector3 formationPos = formationManager.GetFormationPosition(npcIndex, totalNPCs);
        
        // If a valid formation position was returned
        if (formationPos != Vector3.zero)
        {
            targetPosition = formationPos;
            targetPosition.y = target.position.y;
            
            if (useFormation)
            {
                // Add minimal avoidance to prevent exact overlap if NPCs are crowded
                Vector3 avoidanceVector = Vector3.zero;
                
                foreach (NPCManager otherNPC in MGameManager.instance.allNPCs)
                {
                    if (otherNPC == null || otherNPC.transform == transform) continue;
                    
                    float distance = Vector3.Distance(transform.position, otherNPC.transform.position);
                    
                    // Only apply minimal avoidance in formation
                    if (distance < minNPCDistance * 0.5f)
                    {
                        Vector3 separationVector = transform.position - otherNPC.transform.position;
                        float repulsionStrength = Mathf.Pow(1f - (distance / (minNPCDistance * 0.5f)), 2) * 2f;
                        avoidanceVector += separationVector.normalized * repulsionStrength;
                    }
                }
                
                // Apply reduced avoidance
                targetPosition += avoidanceVector * 0.2f;
                // Debug.Log($"Formation Mode: {(formationManager.staticFormations ? "Static" : "Following")}, Player Pos: {target.position}, Target Pos: {targetPosition}");
                
                // Optional: Add debug information
                Debug.DrawLine(transform.position, targetPosition, Color.green);
                
                // If we've reached the formation position, stop movement completely (optional)
                if (formationManager.staticFormations && 
                    Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    currentVelocity = Vector3.zero; // Stop movement once in position
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
                centerOfMass += npc.transform.position;
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
        Vector3 targetForward = target.forward;
        Vector3 targetRight = Vector3.Cross(Vector3.up, targetForward).normalized;

        // Seed random with unique value for each NPC
        Random.State oldState = Random.state;
        Random.InitState(npcIndex * 1000 + Time.frameCount);

        // Calculate base position behind target with some randomness
        float distanceBehind = Mathf.Lerp(minDistanceBehindTarget, maxDistanceBehindTarget, 
            Mathf.Clamp01((float)npcIndex / Mathf.Max(1, MGameManager.instance.allNPCs.Count - 1)));
        
        Vector3 basePosition = target.position - targetForward * distanceBehind;

        // Add some randomness to the position
        Vector2 randomOffset = Random.insideUnitCircle * (minNPCDistance * 0.5f);
        
        // Apply offset using target's right vector
        targetPosition = basePosition 
            + targetRight * randomOffset.x 
            + Vector3.Cross(Vector3.up, targetRight) * randomOffset.y;

        // Restore random state
        Random.state = oldState;

        // Gravitate towards center of mass while maintaining relative positions
        Vector3 centerOffset = (centerOfMass - transform.position) * 0.1f;
        targetPosition += centerOffset;
        targetPosition.y = fixedYPosition;

        // Apply avoidance
        targetPosition += CalculateAvoidanceVector(npc);
    }

    private void UpdateStoppedTargetPosition()
    {
        // Target's forward and right vectors
        Vector3 targetForward = target.forward;
        Vector3 targetRight = Vector3.Cross(Vector3.up, targetForward).normalized;

        // Define the bounds of the random spread area
        float minSpreadBehind = minDistanceBehindTarget;
        float maxSpreadBehind = maxDistanceBehindTarget;
        float maxSpreadSide = maxDistanceBehindTarget;

        // Seed random with unique value for each NPC to ensure consistent but random positioning
        Random.State oldState = Random.state;
        Random.InitState(npcIndex * 1000 + Time.frameCount);

        // Base position behind the target with random distance
        float randomDistanceBehind = Random.Range(minSpreadBehind, maxSpreadBehind);
        Vector3 basePosition = target.position - targetForward * randomDistanceBehind;

        // Random 2D offset within a circular area
        Vector2 randomOffset = Random.insideUnitCircle * maxSpreadSide;
        
        // Apply offset using target's right vector
        targetPosition = basePosition 
            + targetRight * randomOffset.x 
            + Vector3.Cross(Vector3.up, targetRight) * randomOffset.y;

        // Restore random state
        Random.state = oldState;

        targetPosition.y = fixedYPosition;
    }

    // float CalculateGroupSideOffset()
    // {
    //     // Distribute NPCs across a line perpendicular to target's movement
    //     float spacing = minNPCDistance;
    //     int npcCount = MGameManager.instance.allNPCss.Count;
        
    //     // Center the group around the target's path
    //     float centerOffset = (npcCount - 1) * spacing * 0.5f;
        
    //     // Calculate individual NPC's offset
    //     float individualOffset = npcIndex * spacing - centerOffset;
        
    //     return individualOffset;
    // }

    private Vector3 CalculateAvoidanceVector(NPCManager npc)
    {
        Vector3 avoidanceSum = Vector3.zero;
        
        foreach (NPCManager otherNPC in MGameManager.instance.allNPCs)
        {
            if (otherNPC == npc) continue;
            
            float distance = Vector3.Distance(transform.position, otherNPC.transform.position);
            
            // Repulsion when too close
            if (distance < minNPCDistance)
            {
                Vector3 separationVector = transform.position - otherNPC.transform.position;
                float repulsionStrength = Mathf.Pow(1f - (distance / minNPCDistance), 3) * 5f;
                avoidanceSum += separationVector.normalized * repulsionStrength;
            }
            
            // Attraction when too far
            if (distance > maxNPCDistance)
            {
                Vector3 attractionVector = otherNPC.transform.position - transform.position;
                float attractionStrength = (distance - maxNPCDistance) * 0.1f;
                avoidanceSum += attractionVector.normalized * attractionStrength;
            }
        }
        
        return avoidanceSum;
    }
}