using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NPCFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float minDistanceBetweenNPCs = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stopThreshold = 0.1f;
    
    [Header("Formation Settings")]
    [SerializeField] private float arcWidth = 120f; // Arc angle in degrees behind player
    [SerializeField] private int maxPositionAttempts = 30;
    [SerializeField] private bool useFixedPositions = false; // If true, each NPC gets a fixed slot in the formation
    [SerializeField] private int myPositionIndex = 0; // Only used if useFixedPositions is true
    
    [Header("Debug")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color debugTargetColor = Color.green;
    [SerializeField] private Color debugPathColor = Color.yellow;

    private Vector3 targetPosition;
    private Vector3 previousPlayerPosition;
    private bool isMoving = false;
    private int uniqueId;
    private static int nextId = 0;
    private static List<NPCFollower> allFollowers = new List<NPCFollower>();

    private void Awake()
    {
        uniqueId = nextId++;
        allFollowers.Add(this);
    }

    private void OnDestroy()
    {
        allFollowers.Remove(this);
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("NPCFollower: No player found! Please assign playerTransform or add 'Player' tag to player.");
                enabled = false;
                return;
            }
        }

        previousPlayerPosition = playerTransform.position;
        CalculateTargetPosition();
    }

    private void Update()
    {
        // Check if player has moved
        isMoving = Vector3.Distance(previousPlayerPosition, playerTransform.position) > stopThreshold;
        
        if (isMoving || Vector3.Distance(transform.position, targetPosition) > stopThreshold)
        {
            // Recalculate position if player moves or if we're not at our target yet
            CalculateTargetPosition();
            MoveToTarget();
        }
        
        // Always update rotation to match player
        UpdateRotation();
        
        // Update previous player position
        if (isMoving)
        {
            previousPlayerPosition = playerTransform.position;
        }
    }

    private void CalculateTargetPosition()
    {
        if (useFixedPositions)
        {
            CalculateFixedFormationPosition();
        }
        else
        {
            CalculateDynamicPosition();
        }
    }

    private void CalculateFixedFormationPosition()
    {
        // Get all active followers and sort by their ID to ensure consistent ordering
        List<NPCFollower> sortedFollowers = allFollowers.OrderBy(f => f.uniqueId).ToList();
        int totalFollowers = sortedFollowers.Count;
        
        // Calculate this NPC's position in the formation
        int myIndex = sortedFollowers.IndexOf(this);
        
        // Position NPCs in an arc behind the player
        float angleStep = arcWidth / Mathf.Max(1, totalFollowers - 1);
        float startAngle = -arcWidth / 2;
        
        // My angle in the formation
        float myAngle = startAngle + (myIndex * angleStep);
        
        // Convert to radians
        float angleRad = myAngle * Mathf.Deg2Rad;
        
        // Calculate position relative to player's orientation
        Vector3 offset = new Vector3(
            Mathf.Sin(angleRad) * followDistance,
            0,
            -Mathf.Cos(angleRad) * followDistance  // Negative Z to position behind
        );
        
        // Transform offset to world space based on player's rotation
        Vector3 worldOffset = playerTransform.TransformDirection(offset);
        
        // Set target behind player
        targetPosition = playerTransform.position + worldOffset;
    }

    private void CalculateDynamicPosition()
    {
        // Start with a position directly behind the player
        Vector3 bestPosition = playerTransform.position - playerTransform.forward * followDistance;
        float bestScore = float.MinValue;
        
        // Try multiple positions in an arc behind the player
        for (int i = 0; i < maxPositionAttempts; i++)
        {
            // Calculate a position in an arc behind the player
            float angle = Random.Range(-arcWidth/2, arcWidth/2) * Mathf.Deg2Rad;
            float distance = Random.Range(followDistance * 0.8f, followDistance * 1.2f);
            
            // Calculate position in player's local space
            Vector3 localOffset = new Vector3(
                Mathf.Sin(angle) * distance,
                0,
                -Mathf.Cos(angle) * distance  // Negative to be behind
            );
            
            // Convert to world space
            Vector3 candidatePosition = playerTransform.position + playerTransform.TransformDirection(localOffset);
            
            // Score this position
            float score = ScorePosition(candidatePosition);
            
            // Keep the best position found
            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = candidatePosition;
            }
            
            // If this position is good enough, stop searching
            if (score > 0.8f)
                break;
        }
        
        targetPosition = bestPosition;
    }

    private float ScorePosition(Vector3 position)
    {
        float score = 1.0f;
        
        // Check if behind player (highest priority)
        Vector3 playerToPos = position - playerTransform.position;
        float behindFactor = Vector3.Dot(playerToPos.normalized, -playerTransform.forward);
        
        // Less than 0 means not behind at all, should be heavily penalized
        if (behindFactor < 0)
            return -1000; // Very bad position, not behind player
        
        // Strong weight for being directly behind
        score += behindFactor * 3.0f;
        
        // Penalize for being too far from desired distance
        float distanceToPlayer = Vector3.Distance(position, playerTransform.position);
        float distanceScore = 1.0f - Mathf.Abs(distanceToPlayer - followDistance) / followDistance;
        score += distanceScore;
        
        // Penalize for being too close to other NPCs
        foreach (NPCFollower other in allFollowers)
        {
            if (other != this)
            {
                float distanceToOther = Vector3.Distance(position, other.transform.position);
                if (distanceToOther < minDistanceBetweenNPCs)
                {
                    // Severe penalty for being too close
                    score -= (minDistanceBetweenNPCs - distanceToOther) * 5.0f;
                }
            }
        }
        
        return score;
    }

    private void MoveToTarget()
    {
        // Only move if the player is moving or if we're far from our target
        if (isMoving || Vector3.Distance(transform.position, targetPosition) > stopThreshold)
        {
            // Move towards target position
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            
            // Speed scales with distance (move faster when further away)
            float currentSpeed = moveSpeed * Mathf.Min(1.0f, distanceToTarget / 2.0f);
            
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }
    }

    private void UpdateRotation()
    {
        // Make NPC face the same direction as the player
        Quaternion targetRotation = playerTransform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugVisuals || !Application.isPlaying) return;
        
        // Draw the target position
        Gizmos.color = debugTargetColor;
        Gizmos.DrawSphere(targetPosition, 0.2f);
        
        // Draw line to target
        Gizmos.color = debugPathColor;
        Gizmos.DrawLine(transform.position, targetPosition);
        
        // Draw safe distance around NPC
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistanceBetweenNPCs / 2);
        
        // Visualize the arc behind player
        if (playerTransform != null)
        {
            Gizmos.color = Color.blue;
            int segments = 20;
            float angleStep = arcWidth / segments;
            Vector3 previousPoint = Vector3.zero;
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = (-arcWidth/2 + i * angleStep) * Mathf.Deg2Rad;
                Vector3 localOffset = new Vector3(
                    Mathf.Sin(angle) * followDistance,
                    0,
                    -Mathf.Cos(angle) * followDistance
                );
                
                Vector3 worldPoint = playerTransform.position + playerTransform.TransformDirection(localOffset);
                
                if (i > 0)
                    Gizmos.DrawLine(previousPoint, worldPoint);
                
                previousPoint = worldPoint;
            }
        }
    }
}