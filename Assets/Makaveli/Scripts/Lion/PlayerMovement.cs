using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float depthSpeed = 3f;
    public float zMin = -7f;
    public float zMax = 1f;

    public Rigidbody rb;
    private Vector2 movement;
    private bool facingRight = true;

    void Update()
    {
        // Get input for X (side to side) and Z (depth) movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Flip the player if changing direction
        if (moveX > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveX < 0 && facingRight)
        {
            Flip();
        }

        // Calculate movement direction
        movement = new Vector2(moveX * speed, rb.linearVelocity.y);

        // Move in the Z direction (simulated depth)
        float newZ = Mathf.Clamp(transform.position.z + moveZ * depthSpeed * Time.deltaTime, zMin, zMax);
        transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
    }

    void FixedUpdate()
    {
        // Apply movement to Rigidbody
        rb.linearVelocity = new Vector2(movement.x, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        rb.transform.rotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);
    }
}
