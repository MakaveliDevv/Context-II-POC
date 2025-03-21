using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    private TopDownMovement topDownMovement;
    private CharacterController controller;
    public float movementSpeed;
    public float appliedMovementSpeedPercentage;
    private readonly CustomNetworkBehaviour customNetworkBehaviour;
    private Vector2 movementInput = new();

    private TopDownCameraController topDownCameraController;
    [SerializeField] private Vector3 camOffset = new(0, 10, -5);    
    [SerializeField] private float camSmoothSpeed = 5f;
    [SerializeField] private float camRotationSpeed = 100f;  


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = transform.GetChild(0).GetComponent<CharacterController>();
        
        topDownMovement = new
        (
            controller,
            controller.transform,
            movementSpeed,
            appliedMovementSpeedPercentage,
            customNetworkBehaviour
        );

        topDownCameraController = new
        (
            transform,
            camOffset,
            camSmoothSpeed,
            camRotationSpeed
        );
    }

    // Update is called once per frame
    void Update()
    {
        movementInput = InputActionHandler.GetMovementInput();
        // topDownMovement.OverallMovement(movementInput, InputActionHandler.IsSprinting());
        topDownCameraController.Movement(transform);
    }
}
