using UnityEngine;
using System.Collections.Generic;

public class NPCPatrol : ITriggerMovement
{
    private Transform walkableArea;
    private Vector3 targetPosition;
    public Vector3 TargetPosition => targetPosition; // Proper public property
    private readonly Transform transform;
    private readonly LayerMask layerMask;
    private readonly float patrolSpeed;
    private readonly float obstacleAvoidanceDistance;
    private readonly float raycastLength;
    private readonly float spacing;
    private bool isMovingToRandomLocation = false;
    private bool isPatrolling = true;

    public NPCPatrol
    (
        Transform transform,
        LayerMask layerMask,
        Vector3 targetPosition,
        float patrolSpeed,
        float obstacleAvoidanceDistance,
        float raycastLength,
        float spacing
    )
    {
        this.layerMask = layerMask;
        this.transform = transform;
        this.targetPosition = targetPosition;
        this.patrolSpeed = patrolSpeed;
        this.obstacleAvoidanceDistance = obstacleAvoidanceDistance;
        this.raycastLength = raycastLength;
        this.spacing = spacing;

        Initialize();
    }

    public void Initialize()
    {
        walkableArea = MGameManager.instance.walkableArea;
        NPCsManagement.RegisterTriggerMovement(this);
        SetNewTarget();
    }

    public void MoveNPC()
    {
        if (isMovingToRandomLocation)
        {
            MoveToTarget();
        }
        else if (isPatrolling)
        {
            Patrol();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ResumePatrol();
        } 
    }

    private void Patrol()
    {
        if (DetectObstacle())
        {
            SetNewTarget();
        }
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewTarget();
        }
    }

    private bool DetectObstacle()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, raycastLength, layerMask))
        {
            if (hit.distance < obstacleAvoidanceDistance)
            {
                return true;
            }
        }
        return false;
    }

    private void TriggerMovement(Transform location)
    {
        if (location.TryGetComponent<Collider>(out var locationCollider))
        {
            // Get a valid grid position within or around the location bounds
            Vector3 gridPosition = GetAvailablePosition(locationCollider.bounds);
            
            // Only proceed if we found a valid position
            if (gridPosition != Vector3.zero)
            {
                targetPosition = gridPosition;
                isMovingToRandomLocation = true;
                isPatrolling = false;
                
                // Debug visualization to confirm grid positions
                Debug.DrawLine(transform.position, targetPosition, Color.green, 3f);
            }
            else
            {
                Debug.LogWarning("No available grid position found in or around bounds");
            }
        }
    }

    void ITriggerMovement.TriggerMovement(Transform location)
    {
        TriggerMovement(location);
    }

    private Vector3 GetAvailablePosition(Bounds bounds)
    {
        // First try to get a position inside the bounds
        List<Vector3> gridPositions = GenerateGrid(bounds, false);
        
        // Shuffle the positions to avoid always choosing the same corner
        ShufflePositions(gridPositions);
        
        foreach (var position in gridPositions)
        {
            if (!IsPositionOccupied(position)) 
            {
                return position;
            }
        }
        
        // If all inside positions are occupied, try positions around the perimeter
        List<Vector3> perimeterPositions = GeneratePerimeterGrid(bounds);
        ShufflePositions(perimeterPositions);
        
        foreach (var position in perimeterPositions)
        {
            if (!IsPositionOccupied(position)) 
            {
                return position;
            }
        }
        
        Debug.LogWarning("All grid positions (inside and perimeter) are occupied");
        return Vector3.zero;
    }

    private void ShufflePositions(List<Vector3> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            int randomIndex = Random.Range(i, positions.Count);
            Vector3 temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }
    }

    private List<Vector3> GenerateGrid(Bounds bounds, bool visualize = true)
    {
        List<Vector3> positions = new List<Vector3>();
        float startX = bounds.min.x + spacing / 2;
        float startZ = bounds.min.z + spacing / 2;

        for (float x = startX; x <= bounds.max.x - spacing / 2; x += spacing)
        {
            for (float z = startZ; z <= bounds.max.z - spacing / 2; z += spacing)
            {
                positions.Add(new Vector3(x, bounds.center.y + .5f, z));
            }
        }
        
        // Debug the grid
        if (visualize)
        {
            foreach (var pos in positions)
            {
                Debug.DrawRay(pos, Vector3.up * 0.5f, Color.yellow, 3f);
            }
        }
        
        return positions;
    }

    private List<Vector3> GeneratePerimeterGrid(Bounds bounds)
    {
        List<Vector3> perimeterPositions = new List<Vector3>();
        float bufferDistance = spacing; // Distance outside the bounds
        
        // Get the bounds dimensions
        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minZ = bounds.min.z;
        float maxZ = bounds.max.z;
        float y = bounds.center.y + .5f;
        
        // Generate positions on all four sides of the perimeter with spacing
        
        // North side (positive Z)
        for (float x = minX; x <= maxX; x += spacing)
        {
            Vector3 position = new Vector3(x, y, maxZ + bufferDistance);
            perimeterPositions.Add(position);
            if (position != Vector3.zero)
            {
                Debug.DrawRay(position, Vector3.up * 0.5f, Color.red, 3f);
            }
        }
        
        // South side (negative Z)
        for (float x = minX; x <= maxX; x += spacing)
        {
            Vector3 position = new Vector3(x, y, minZ - bufferDistance);
            perimeterPositions.Add(position);
            if (position != Vector3.zero)
            {
                Debug.DrawRay(position, Vector3.up * 0.5f, Color.red, 3f);
            }
        }
        
        // East side (positive X)
        for (float z = minZ; z <= maxZ; z += spacing)
        {
            Vector3 position = new Vector3(maxX + bufferDistance, y, z);
            perimeterPositions.Add(position);
            if (position != Vector3.zero)
            {
                Debug.DrawRay(position, Vector3.up * 0.5f, Color.red, 3f);
            }
        }
        
        // West side (negative X)
        for (float z = minZ; z <= maxZ; z += spacing)
        {
            Vector3 position = new Vector3(minX - bufferDistance, y, z);
            perimeterPositions.Add(position);
            if (position != Vector3.zero)
            {
                Debug.DrawRay(position, Vector3.up * 0.5f, Color.red, 3f);
            }
        }
        
        return perimeterPositions;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        // Check if the position is already occupied by another NPC
        foreach (var npc in MGameManager.instance.allNPCs)
        {
            // Skip checking against self
            if (npc.transform == transform) continue;
            
            // Check both current position and target position
            if (Vector3.Distance(npc.transform.position, position) < spacing * 0.9f || 
                Vector3.Distance(npc.TargetPosition, position) < spacing * 0.9f)
            {
                return true;
            }
        }
        
        // Check if the position is blocked by an obstacle
        if (Physics.CheckSphere(position, spacing * 0.4f, layerMask))
        {
            return true;
        }
        
        return false;
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
        
        // When we've reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // Snap to exact grid position
            transform.position = targetPosition;
            isMovingToRandomLocation = false;
        }
    }

    private void ResumePatrol()
    {
        if (!isPatrolling && !isMovingToRandomLocation)
        {
            isPatrolling = true;
            SetNewTarget();
        }
    }

    private void SetNewTarget()
    {
        Vector3 areaSize = walkableArea.GetComponent<Collider>()?.bounds.size ?? Vector3.one;
        float randomX = Random.Range(-areaSize.x / 2, areaSize.x / 2);
        float randomZ = Random.Range(-areaSize.z / 2, areaSize.z / 2);
        
        // Snap to grid
        randomX = Mathf.Round(randomX / spacing) * spacing;
        randomZ = Mathf.Round(randomZ / spacing) * spacing;
        
        targetPosition = walkableArea.transform.position + new Vector3(randomX, 0.5f, randomZ);
    }
}