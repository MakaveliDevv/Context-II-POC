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

            // Calculate movement speed based on magnitude of movement vector
            float currentSpeed = moveDirection.magnitude * maxSpeed; 
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
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, moveDirection * maxSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
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
