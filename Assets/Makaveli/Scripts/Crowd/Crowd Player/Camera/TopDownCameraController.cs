using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new(0, 10, -5);
    public float smoothSpeed = 5f;
    public float rotationSpeed = 100f;
    
    private float currentRotation = 0f;

    private void LateUpdate()
    {
        if (player == null) return;

        // Rotate camera with arrow keys
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) rotationInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) rotationInput = 1f;

        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;
        
        // Calculate new camera position by rotating around the player
        Quaternion rotation = Quaternion.Euler(0f, currentRotation, 0f);
        Vector3 newOffset = rotation * offset;
        Vector3 desiredPosition = player.position + newOffset;

        // Smoothly move the camera to the new position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Look at the player from above at a fixed angle
        transform.LookAt(player.position + Vector3.up * 2f);
    }
}

