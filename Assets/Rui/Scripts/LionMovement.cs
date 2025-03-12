using UnityEngine;

public class LionMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 200f;

    private Rigidbody rb;
    private float inputX, inputZ;
    private Vector3 moveDirection;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    void Start()
    {
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
        // Get input
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        // Calculate movement direction
        moveDirection = new Vector3(inputX, 0, inputZ).normalized;

        // Handle rotation
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Update animator speed parameter
        float currentSpeed = Mathf.Abs(inputX);
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    void FixedUpdate()
    {
        // Handle movement
        if (moveDirection.magnitude > 0)
        {
            rb.velocity = Vector3.MoveTowards(rb.velocity, moveDirection * maxSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }
    }
}
