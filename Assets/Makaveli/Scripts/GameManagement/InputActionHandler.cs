using UnityEngine;
using UnityEngine.InputSystem;

public static class InputActionHandler 
{
    private static readonly InputActions inputActionAsset; // The Input Actions 
    private static readonly InputAction moveAction, sprintAction, jumpAction; // The action
    private static bool isEnabled = false; 
    private static bool isSprinting = false;
    private static bool isJumping = false;
    
    static InputActionHandler() 
    {
        inputActionAsset ??= new InputActions();

        var actionMap = inputActionAsset.CrowdPlayer;
        moveAction = actionMap.Movement; 
        sprintAction = actionMap.Sprint;
        jumpAction = actionMap.Jump;
    }

    public static void EnableInputActions()
    {
        if (!isEnabled) 
        {
            Debug.Log("Enabling Input Actions");
            moveAction.Enable();
            sprintAction.Enable();
            jumpAction.Enable();
            isEnabled = true;
        }
    }

    public static void DisableInputActions()
    {
        if (isEnabled) 
        {
            Debug.Log("Disabling Input Actions");
            moveAction.Disable();
            sprintAction.Disable();
            jumpAction.Disable();
            isEnabled = false;
        }
    }

    public static Vector2 GetMovementInput()
    {
        return moveAction.ReadValue<Vector2>();
    }

    public static bool IsSprinting() 
    {
        if(!isSprinting) 
        {
            isSprinting = true;
            return sprintAction.IsPressed();
        }
    	
        isSprinting = false;
        return false;
    }

    public static bool IsJumping() 
    {
        if(!isJumping) 
        {
            isJumping = true;
            return jumpAction.IsPressed();
        }

        isJumping = false;
        return false;
    }
}