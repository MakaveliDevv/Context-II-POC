using UnityEngine;

public class ShapeSetterTest
{
    private LayerMask npcLayer;
    private readonly Camera camera;
    // private GameObject locationObject; // The plane object representing the location
    
    // private List<GameObject> allNpcs = new List<GameObject>();
    private GameObject currentlySelectedNpc;
    private bool isDragging = false;

    private Vector2 minBoundary;
    private Vector2 maxBoundary;

    public ShapeSetterTest(LayerMask npcLayer, Camera camera) 
    {
        this.npcLayer = npcLayer;
        this.camera = camera;
    }

    public void Start(Transform locationObject)
    {
        // if (mainCamera == null)
        //     mainCamera = Camera.main;

        // // If no location object is set, attempt to find it by tag (assuming it's a plane).
        // if (locationObject == null)
        // {
        //     locationObject = GameObject.FindWithTag("Location");
        // }

        // // Find all NPCs in the scene
        // GameObject[] npcsInScene = GameObject.FindGameObjectsWithTag("NPC");
        // allNpcs.AddRange(npcsInScene);

        // Get the boundaries of the location (plane object)
        if (locationObject != null)
        {
            UpdateLocationBoundaries(locationObject);
        }
        
        // Debug the boundary values at start
        Debug.Log($"Boundaries: Min({minBoundary.x}, {minBoundary.y}), Max({maxBoundary.x}, {maxBoundary.y})");
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
                Debug.Log($"Selected NPC at position: {currentlySelectedNpc.transform.position}");
            }
        }

        // Handle dragging
        if (isDragging && currentlySelectedNpc != null)
        {
            // Get the mouse position in world space
            Vector3 mouseWorldPos = GetMousePositionOnXZPlane();

            // Create a new position, keeping the Y value the same
            Vector3 newPosition = new(
                mouseWorldPos.x,
                currentlySelectedNpc.transform.position.y,
                mouseWorldPos.z
            );

            // Debug pre-clamped position
            Debug.Log($"Pre-clamp position: {newPosition}");

            // Clamp the position within proper boundaries
            newPosition.x = Mathf.Clamp(newPosition.x, minBoundary.x, maxBoundary.x);
            newPosition.z = Mathf.Clamp(newPosition.z, minBoundary.y, maxBoundary.y);

            // Debug post-clamped position
            Debug.Log($"Post-clamp position: {newPosition}");

            // Update the NPC position
            currentlySelectedNpc.transform.position = newPosition;
        }

        // Handle release
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            currentlySelectedNpc = null;
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

                Debug.Log($"Updated Boundaries: Min({minBoundary.x}, {minBoundary.y}), Max({maxBoundary.x}, {maxBoundary.y})");
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
