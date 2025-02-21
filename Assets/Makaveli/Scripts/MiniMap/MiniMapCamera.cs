using UnityEngine;

public class MiniMapCameraFollow : MonoBehaviour
{
    public Transform player; 
    public float height = 20f; 
    public float smoothSpeed = 0.125f; 


    void LateUpdate()
    {
        if (player == null) return;
        
        transform.SetPositionAndRotation(new Vector3(player.position.x, player.position.y + height, player.position.z), Quaternion.Euler(90f, 0f, 0f));

        Vector3 desiredPos = new(player.position.x, transform.position.y + height, transform.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        
        transform.position = smoothedPosition;
    }
}
