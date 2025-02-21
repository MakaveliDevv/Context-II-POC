using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.125f; 
    // public Vector3 offset; 
    public float offsetZ;

    void LateUpdate()
    {
        if (player == null) return;

        // Vector3 desiredPosition = player.position + offset;
        Vector3 desiredPos = new(player.position.x, transform.position.y, transform.position.z + offsetZ);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        
        transform.position = smoothedPosition;
    }
}
