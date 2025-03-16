using UnityEngine;

public class ShapeRearrangement
{
    private readonly Camera camera;    
    private LayerMask npcLayer;
    private GameObject currentlySelectedNpc;
    private Vector3 targetPosition;
    private bool isDragging;
    private bool isMovingToTarget;
    private readonly float lerpSpeed = 3f;

    private Vector2 minBoundary;
    private Vector2 maxBoundary;

    public ShapeRearrangement(Camera camera, LayerMask npcLayer) 
    {
        this.camera = camera;
        this.npcLayer = npcLayer;
    }

    public void Start(Transform location)
    {
        // Get the boundaries of the location (plane object)
        if (location != null)
        {
            UpdateLocationBoundaries(location);
        }
        
        // Debug the boundary values at start
        // Debug.Log($"Boundaries: Min({minBoundary.x}, {minBoundary.y}), Max({maxBoundary.x}, {maxBoundary.y})");
    }

    public void Update()
    {
        // Handle selection
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, npcLayer))
            {
                currentlySelectedNpc = hit.collider.gameObject;
                isDragging = true;
                isMovingToTarget = true;
                // Debug.Log($"Selected NPC at position: {currentlySelectedNpc.transform.position}");
            }
        }

        // Handle dragging
        if (isDragging && currentlySelectedNpc != null)
        {
            // Get the mouse position in world space
            Vector3 mouseWorldPos = GetMousePositionOnXZPlane();

            // Create a new position, keeping the Y value the same
            targetPosition = new(
                mouseWorldPos.x,
                currentlySelectedNpc.transform.position.y,
                mouseWorldPos.z
            );

            // Debug pre-clamped position
            // Debug.Log($"Pre-clamp position: {targetPosition}");

            // Clamp the position within proper boundaries
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBoundary.x, maxBoundary.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minBoundary.y, maxBoundary.y);

            // Debug post-clamped position
            // Debug.Log($"Post-clamp position: {targetPosition}");

            Vector3 newPosition = Vector3.Lerp(currentlySelectedNpc.transform.position, targetPosition, lerpSpeed * Time.deltaTime);
         

            // Update the NPC position
            currentlySelectedNpc.transform.position = newPosition;
        }

        // Handle release
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if(isMovingToTarget && currentlySelectedNpc != null && !isDragging) 
        {
            Vector3 newPosition = Vector3.Lerp(currentlySelectedNpc.transform.position, targetPosition, Time.deltaTime * lerpSpeed);
            
            // Debug lerped position
            // Debug.Log($"Lerped position: {newPosition}");
            
            // Update the NPC position
            currentlySelectedNpc.transform.position = newPosition;
            
            // Check if we've essentially reached the target position
            if (!isDragging && Vector3.Distance(currentlySelectedNpc.transform.position, targetPosition) < 0.01f)
            {
                isMovingToTarget = false;
                currentlySelectedNpc = null;
                // Debug.Log("Movement complete");
            }
        }
    }

    private Vector3 GetMousePositionOnXZPlane()
    {
        // Create a ray from the camera through the mouse position
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        // Define the Y position of the XZ plane
        float planeY = 0f;
        if (currentlySelectedNpc != null)
        {
            planeY = currentlySelectedNpc.transform.position.y;
        }

        // Calculate where the ray intersects the XZ plane
        if (Mathf.Approximately(ray.direction.y, 0f))
        {
            // Handle case where ray is parallel to the plane
            return new Vector3(ray.origin.x, planeY, ray.origin.z);
        }

        float t = (planeY - ray.origin.y) / ray.direction.y;

        // Get the point on the plane
        Vector3 planePoint = ray.origin + ray.direction * t;

        return planePoint;
    }

    private void UpdateLocationBoundaries(Transform locationObject)
    {
        if (locationObject != null)
        {
            // Get the Collider of the locationObject
            if (locationObject.TryGetComponent<Collider>(out var locationCollider))
            {
                // Update the min and max boundaries using the collider's bounds
                minBoundary = new Vector2(
                    locationCollider.bounds.min.x,
                    locationCollider.bounds.min.z
                );
                maxBoundary = new Vector2(
                    locationCollider.bounds.max.x,
                    locationCollider.bounds.max.z
                );

                // Debug.Log($"Updated Boundaries: Min({minBoundary.x}, {minBoundary.y}), Max({maxBoundary.x}, {maxBoundary.y})");
            }
            else
            {
                Debug.LogError("Location object does not have a collider.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (minBoundary == null || maxBoundary == null) return;

        Gizmos.color = Color.yellow;

        // Draw the boundary as a wireframe box
        Vector3 size = new Vector3(maxBoundary.x - minBoundary.x, 0.1f, maxBoundary.y - minBoundary.y);
        Vector3 center = new Vector3((minBoundary.x + maxBoundary.x) * 0.5f, 0, (minBoundary.y + maxBoundary.y) * 0.5f);
        Gizmos.DrawWireCube(center, size);
    }
}
