using UnityEngine;
using System.Collections.Generic;

public class NPCPatrol : MonoBehaviour
{
    public NPCSpawner thingySpawner;
    public Transform patrolArea;
    public float patrolSpeed = 3f;
    public float obstacleAvoidanceDistance = 2.5f;
    public float raycastLength = 2f;
    public float spacing = 1f;
    private Vector3 targetPosition;
    public LayerMask obstacleLayerMask;

    private bool isMovingToRandomLocation = false;
    private bool isPatrolling = true;
    private static readonly List<NPCPatrol> allThingys = new();

    private void Start()
    {
        thingySpawner = FindAnyObjectByType<NPCSpawner>();
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
        patrolArea = thingySpawner.patrolArea;
        SetNewTarget();
        allThingys.Add(this);
    }

    private void Update()
    {
        if (isMovingToRandomLocation)
        {
            MoveToTarget();
        }
        else if (isPatrolling)
        {
            Patrol();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerMovement();
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
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, raycastLength, obstacleLayerMask))
        {
            if (hit.distance < obstacleAvoidanceDistance)
            {
                return true;
            }
        }
        return false;
    }

    private void TriggerMovement()
    {
        if (thingySpawner.randomLocations.Count == 0) return;

        Transform chosenLocation = thingySpawner.randomLocations[Random.Range(0, thingySpawner.randomLocations.Count)];
        if (chosenLocation.TryGetComponent<Collider>(out var locationCollider))
        {
            targetPosition = GetAvailablePosition(locationCollider.bounds);
            isMovingToRandomLocation = true;
            isPatrolling = false;
        }
    }

    private Vector3 GetAvailablePosition(Bounds bounds)
    {
        List<Vector3> gridPositions = GenerateGrid(bounds);
        foreach (var position in gridPositions)
        {
            if (!IsPositionOccupied(position)) return position;
        }
        return Vector3.zero;
    }

    private List<Vector3> GenerateGrid(Bounds bounds)
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
        return positions;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        foreach (var thingy in allThingys)
        {
            if (Vector3.Distance(thingy.targetPosition, position) < spacing)
            {
                return true;
            }
        }
        return false;
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
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
        Vector3 areaSize = patrolArea.GetComponent<Collider>()?.bounds.size ?? Vector3.one;
        float randomX = Random.Range(-areaSize.x / 2, areaSize.x / 2);
        float randomZ = Random.Range(-areaSize.z / 2, areaSize.z / 2);
        targetPosition = patrolArea.position + new Vector3(randomX, 0.5f, randomZ);
    }
}
