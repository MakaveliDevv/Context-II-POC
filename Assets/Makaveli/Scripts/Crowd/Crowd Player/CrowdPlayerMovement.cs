using UnityEngine;

public class CrowdPlayerMovement
{
    private readonly CharacterController controller;
    private readonly Transform cameraTransform;
    private readonly Transform playerBody;
    private Vector2 angleLimit;
    private Vector3 velocity;
    private Vector2 lookAngles;

    private readonly float movementSpeed;
    private readonly float aplliedMovementSpeedPercentage;
    private float currentMovementSpeed;
    private readonly float jumpForce;
    private readonly float gravity = 9.81f;
    private readonly float sensivity;
    private readonly bool invert;

    public CrowdPlayerMovement
    (
        CharacterController controller,
        Transform playerBody,
        Transform cameraTransform,
        Vector2 angleLimit,
        float movementSpeed,
        float aplliedMovementSpeedPercentage,
        float jumpForce,
        float sensivity,
        bool invert
    ) 
    {
        this.controller = controller;
        this.playerBody = playerBody;
        this.cameraTransform = cameraTransform;
        this.movementSpeed = movementSpeed;
        this.aplliedMovementSpeedPercentage = aplliedMovementSpeedPercentage;
        this.jumpForce = jumpForce;
        this.sensivity = sensivity;
        this.invert = invert;
        this.angleLimit = angleLimit;

        currentMovementSpeed = movementSpeed;
    }

    public void OverallMovement(Vector2 movementInput, bool isSprinting, bool isJumping, bool ableToLook) 
    {
        CameraLookAround(ref ableToLook);

        // Calculate movement direction based on camera forward direction
        Vector3 moveDirection = (cameraTransform.forward * movementInput.y) + (cameraTransform.right * movementInput.x);
        moveDirection.y = 0;
        moveDirection.Normalize();

        // Apply Movement
        controller.Move(currentMovementSpeed * Time.deltaTime * moveDirection); 
        ApplyGravity(isJumping);
        
        if(moveDirection != Vector3.zero) 
        {
            if(isSprinting) ApplyAdditionalMovementSpeed();
            else currentMovementSpeed = movementSpeed;
        } 
    }

    private void ApplyGravity(bool isJumping) 
    {
        // If controller not grounded
        if(!controller.isGrounded) velocity.y -= gravity * Time.deltaTime;
        else if(isJumping) velocity.y = jumpForce;
        	
        // Move controller
        controller.Move(velocity * Time.deltaTime);
    }

    private void CameraLookAround(ref bool ableToLook) 
    {
        if(!ableToLook) return;

        if(Cursor.lockState == CursorLockMode.None) Cursor.lockState = CursorLockMode.Locked;
        
        // Mouse Inputs
        float mouseX = Input.GetAxis("Mouse X") * (sensivity * 10f) * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * (sensivity * 10f) * Time.deltaTime;

        // Mouse direction
        Vector2 mouseDirection = new(mouseX, mouseY);

        // Look Angles
        lookAngles.x -= mouseDirection.x * sensivity * (invert ? 1f : -1f);
        lookAngles.y += mouseDirection.y * sensivity * (invert ? 1f : -1f);

        // Clamp the angles
        lookAngles.y = Mathf.Clamp(lookAngles.y, angleLimit.x, angleLimit.y);

        playerBody.localRotation = Quaternion.Euler(0, lookAngles.x, 0);
        cameraTransform.localRotation = Quaternion.Euler(lookAngles.y, 0, 0);
    }

    private void ApplyAdditionalMovementSpeed() 
    {
        float newMovementSpeed = currentMovementSpeed + (currentMovementSpeed * aplliedMovementSpeedPercentage);
        currentMovementSpeed = newMovementSpeed; 

        Debug.Log($"New movement speed : {newMovementSpeed}");
    }
}
