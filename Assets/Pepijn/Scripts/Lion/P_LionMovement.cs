using Unity.Netcode;
using UnityEngine;

public class P_LionMovement : NetworkBehaviour
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
    CustomNetworkBehaviour customNetworkBehaviour;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        customNetworkBehaviour = GetComponent<CustomNetworkBehaviour>();

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
        if(customNetworkBehaviour.CustomIsOwner())
        {
            // Get input
            inputX = Input.GetAxisRaw("Horizontal");
            inputZ = Input.GetAxisRaw("Vertical");

            float animInputX = Input.GetAxis("Horizontal");
            float animInputZ = Input.GetAxis("Vertical");

            // Calculate movement direction
            moveDirection = new Vector3(inputX, 0, inputZ).normalized;

            // Handle rotation
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Update animator speed parameter
            float currentSpeed = Mathf.Abs((animInputX + animInputZ) / 2f);
            if (animator != null)
            {
                SetAnimatorFloatServerRpc(currentSpeed);
            }
        }
    }

    void FixedUpdate()
    {
        if(customNetworkBehaviour.CustomIsOwner())
        {
            // Handle movement
            if (moveDirection.magnitude > 0)
            {
                // Get current velocity
                Vector3 currentVelocity = rb.linearVelocity;

                // Keep the vertical velocity (y) unchanged, while updating only the horizontal (x, z) velocity
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, moveDirection.x * maxSpeed, acceleration * Time.fixedDeltaTime);
                currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, moveDirection.z * maxSpeed, acceleration * Time.fixedDeltaTime);

                // Set the new velocity
                rb.linearVelocity = currentVelocity;
            }
            else
            {
                // Get current velocity
                Vector3 currentVelocity = rb.linearVelocity;

                // Keep the vertical velocity (y) unchanged, while reducing the horizontal (x, z) velocity
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, 0, deceleration * Time.fixedDeltaTime);
                currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, 0, deceleration * Time.fixedDeltaTime);

                // Set the new velocity
                rb.linearVelocity = currentVelocity;
            }
            customNetworkBehaviour.RequestMoveServerRpc(transform.position, transform.rotation, transform.localScale);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetAnimatorFloatServerRpc(float _float)
    {
        animator.SetFloat("Speed", _float);
        SetAnimatorFloatClientRpc(_float);
    }

    [ClientRpc]
    void SetAnimatorFloatClientRpc(float _float)
    {
        animator.SetFloat("Speed", _float);
    }
}
