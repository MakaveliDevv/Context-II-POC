using System.Collections;
using UnityEngine;

public class CameraManagement
{
    private TopDownCameraController topDownController;
    private ShapeRearrangement shapeRearrangement; 

    private readonly Transform player;
    private LayerMask npcLayer;

    // Camera movement 
    private readonly Camera camera;
    private Vector3 offset;
    private readonly float smoothSpeed;
    private readonly float rotationSpeed;  
    public bool camControlEnabled;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    // Camera shape rearrangement
    

    public CameraManagement
    (
        Camera camera, 
        LayerMask npcLayer,
        Transform player, 
        Vector3 offset, 
        float smoothSpeed, 
        float rotationSpeed
    )
    {
        this.camera = camera;
        this.npcLayer = npcLayer;
        this.player = player;
        this.offset = offset; 
        this.smoothSpeed = smoothSpeed;
        this.rotationSpeed = rotationSpeed;
    }

    public void Start() 
    {
        topDownController = new
        (
            player,
            offset,
            smoothSpeed,
            rotationSpeed
        );

        shapeRearrangement = new
        (
            camera,
            npcLayer
        );

        camControlEnabled = true;
    }

    public void CameraMovement() 
    {
        if(camControlEnabled) 
        {
            topDownController.Movement(camera.transform);
        }
    }
    
    public void DragMovement() 
    {
        shapeRearrangement.Update();
    }

    public IEnumerator TiltCamera(Transform location, float distanceOffset, float interpolationDuration)
    {
        // yield return new WaitForSeconds(2f);
        shapeRearrangement.Start(location);

        Debug.Log($"location position -> {location.position}");
        
        // Store original camera position and rotation
        originalCameraPosition = camera.transform.position;
        originalCameraRotation = camera.transform.rotation;
        // Debug.Log($"Orignal cam pos -> {originalCameraPosition}, Original cam rot -> {originalCameraRotation}");
        
        // Set movement false
        camControlEnabled = false;
        Debug.Log($"TiltCamera -> camControlEnabled = {camControlEnabled}");
    
        // Calculate target position and rotation
        Quaternion targetRotation = Quaternion.Euler(90f, 0, 0);
        float maxSize = Mathf.Max(location.localScale.x, location.localScale.z);
        float distance = maxSize * distanceOffset;
        Vector3 targetPosition = location.position + Vector3.up * distance;
        
        // Smooth transition to new position and rotation
        float elapsedTime = 0;
    
        camera.transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        while (elapsedTime < interpolationDuration)
        {
            float t = elapsedTime / interpolationDuration;
            t = Mathf.Clamp01(t);
            camera.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, targetPosition, t), Quaternion.Slerp(startRotation, targetRotation, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position and rotation are exactly as calculated
        camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
        yield break;
    }

    public IEnumerator RepositionCamera(float interpolationDuration)
    {
        // Resume movement
        camControlEnabled = true;
        Debug.Log($"RepositionCamera -> camControlEnabled = {camControlEnabled}");

        float elapsedTime = 0;
        
        camera.transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        while (elapsedTime < interpolationDuration)
        {
            float t = elapsedTime / interpolationDuration;
            t = Mathf.Clamp01(t);
            camera.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, originalCameraPosition, t), Quaternion.Slerp(startRotation, originalCameraRotation, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position and rotation are exactly as stored
        camera.transform.SetPositionAndRotation(originalCameraPosition, originalCameraRotation);
        yield break;
    }
}
