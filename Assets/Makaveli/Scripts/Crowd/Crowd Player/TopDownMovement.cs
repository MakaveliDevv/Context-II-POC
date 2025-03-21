using UnityEngine;

public class TopDownMovement
{
    private readonly CharacterController controller;
    private readonly CustomNetworkBehaviour customNetworkBehaviour;
    private readonly Transform playerBody;

    public float movementSpeed;
    public float travelMovementSpeed = 60f;
    private readonly float appliedMovementSpeedPercentage;
    private float currentMovementSpeed;
    private readonly float rotationSpeed = 10f;
    private readonly float jumpForce;
    private readonly float gravity = 9.81f;
    private Vector3 velocity;

    public TopDownMovement
    (
        CharacterController controller,
        Transform playerBody,
        float movementSpeed,
        float appliedMovementSpeedPercentage,
        CustomNetworkBehaviour customNetworkBehaviour
    )
    {
        this.controller = controller;
        this.playerBody = playerBody;
        this.movementSpeed = movementSpeed;
        this.appliedMovementSpeedPercentage = appliedMovementSpeedPercentage;
        this.customNetworkBehaviour = customNetworkBehaviour;

        currentMovementSpeed = movementSpeed;
    }

    public void OverallMovement(Vector2 movementInput, bool isSprinting, bool isJumping)
    {
        if(customNetworkBehaviour.CustomIsOwner())
        {
            // Calculate movement direction in world space
            Vector3 moveDirection = new(movementInput.x, 0, movementInput.y);
            moveDirection.Normalize();

            // Apply movement
            controller.Move(currentMovementSpeed * Time.deltaTime * moveDirection);
            ApplyGravity(isJumping);
            
            // Rotate the player to face movement direction only when moving
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Sprinting logic
            if (isSprinting)
            {
                ApplyAdditionalMovementSpeed();
            }
            else
            {
                currentMovementSpeed = movementSpeed;
            }
        }
    }

    private void ApplyAdditionalMovementSpeed()
    {
        float newMovementSpeed = movementSpeed + (movementSpeed * appliedMovementSpeedPercentage);
        currentMovementSpeed = newMovementSpeed;

        Debug.Log($"New movement speed: {newMovementSpeed}");
    }

    private void ApplyGravity(bool isJumping) 
    {
        // If controller not grounded
        if(!controller.isGrounded) velocity.y -= gravity * Time.deltaTime;
        else if(isJumping) velocity.y = jumpForce;
        	
        // Move controller
        controller.Move(velocity * Time.deltaTime);
    }
}


