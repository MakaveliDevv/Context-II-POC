using UnityEngine;

public class ShapeSetter 
{
    private LayerMask layer;
    private readonly Camera cam;

    private GameObject currentlySelectedNpc;
    private bool isDragging = false;
    private Vector3 offset;

    public ShapeSetter(LayerMask layer, Camera cam) 
    {
        this.layer = layer;
        this.cam = cam;
    }

    public void Update(Transform location)
    {
        Vector3 mousePosition = GetMouseWorldPosition();

        // Check for mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse input detected...");
            // Try to select an NPC
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
            {
                currentlySelectedNpc = hit.collider.gameObject;
                isDragging = true;
                offset = currentlySelectedNpc.transform.position - hit.point;
            }
            else
            {
                Debug.Log("No collider detected!");
            }
        }

        // Check for mouse movement while dragging
        if (isDragging && currentlySelectedNpc != null)
        {
            // Calculate the new position with the offset
            Vector3 newPosition = mousePosition + offset;
            
            // Clamp the position within boundaries
            newPosition.x = Mathf.Clamp(newPosition.x, location.position.x, location.position.x);
            newPosition.y = Mathf.Clamp(newPosition.y, location.position.y, location.position.y);
            newPosition.z = Mathf.Clamp(newPosition.z, location.position.z, location.position.z);
            
            // Update the NPC position
            currentlySelectedNpc.transform.position = newPosition;
        }

        // Check for mouse button up
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            // Release the NPC
            isDragging = false;
            currentlySelectedNpc = null;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}