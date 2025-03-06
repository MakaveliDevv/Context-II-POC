using UnityEngine;

public class PlayerMovement2 : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float depthSpeed = 3f;
    [SerializeField] private float zMin = -7f;
    [SerializeField] private float zMax = 1f;

    private Rigidbody rb;
    [SerializeField] private Animator animator; // Ensure it's visible in the Inspector

    private Vector2 movement;
    private bool facingRight = true;

    void Awake()
    {
        // Automatically assign Rigidbody and Animator if they are not set in Inspector
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component is missing on " + gameObject.name);
            }
        }
    }

    void Update()
    {
        // Get input for X (side to side) and Z (depth) movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Update the Speed parameter in the Animator
        float currentSpeed = Mathf.Abs(moveX);
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
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

}
