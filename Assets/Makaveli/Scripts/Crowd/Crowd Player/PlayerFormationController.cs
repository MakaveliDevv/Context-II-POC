using UnityEngine;
using TMPro;
using System;

public class PlayerFormationController : MonoBehaviour
{
    [Header("Formation Control")]
    public NPCFormationManager formationManager;
    public KeyCode[] formationHotkeys = new KeyCode[6]; // Array of hotkeys for each formation type
    public KeyCode toggleStaticFormationKey = KeyCode.T;
    
    [Header("UI References (Optional)")]
    public TextMeshProUGUI currentFormationText;

    private CrowdPlayerManager playerManager;

    private void Awake()
    {
        playerManager = GetComponent<CrowdPlayerManager>();    
    }

    private void Start()
    {
        // Find formation manager if not already assigned
        if (formationManager == null)
        {
            formationManager = GetComponent<NPCFormationManager>();
            if (formationManager == null)
            {
                formationManager = gameObject.AddComponent<NPCFormationManager>();
                formationManager.playerTransform = transform;
            }
        }
        
        // Setup default hotkeys if none were assigned in inspector
        if (formationHotkeys.Length < 6)
        {
            formationHotkeys = new KeyCode[6];
        }
        
        if (formationHotkeys[0] == KeyCode.None) formationHotkeys[0] = KeyCode.Alpha0; // Follow
        if (formationHotkeys[1] == KeyCode.None) formationHotkeys[1] = KeyCode.Alpha1; // Triangle
        if (formationHotkeys[2] == KeyCode.None) formationHotkeys[2] = KeyCode.Alpha2; // Rectangle
        if (formationHotkeys[3] == KeyCode.None) formationHotkeys[3] = KeyCode.Alpha3; // Circle
        if (formationHotkeys[4] == KeyCode.None) formationHotkeys[4] = KeyCode.Alpha4; // Line
        if (formationHotkeys[5] == KeyCode.None) formationHotkeys[5] = KeyCode.Alpha5; // Arrow
        
        UpdateFormationText();
    }
    
    private void Update()
    {
        
        // if (Input.GetKeyDown(formationHotkeys[0]))
        // {
        //     ChangeFormation(FormationType.Follow);
        // }
        // else if (Input.GetKeyDown(formationHotkeys[1]))
        // {
        //     ChangeFormation(FormationType.Triangle);
        // }
        // else if (Input.GetKeyDown(formationHotkeys[2]))
        // {
        //     ChangeFormation(FormationType.Rectangle);
        // }
        // else if (Input.GetKeyDown(formationHotkeys[3]))
        // {
        //     ChangeFormation(FormationType.Circle);
        // }
        // else if (Input.GetKeyDown(formationHotkeys[4]))
        // {
        //     ChangeFormation(FormationType.Line);
        // }
        // else if (Input.GetKeyDown(formationHotkeys[5]))
        // {
        //     ChangeFormation(FormationType.Arrow);
        // }

        // Toggle static formation mode
        if (Input.GetKeyDown(toggleStaticFormationKey))
        {
            // If turning static mode ON, update the formation origin to current position
            if (!formationManager.staticFormations)
            {
                formationManager.UpdateFormationOrigin();
            }

            formationManager.staticFormations = !formationManager.staticFormations;
            UpdateFormationText();
        }
        
        HandleFormation();
     
    }

    private void HandleFormation() 
    {
        if(playerManager.playerController.UImanagement.shapeManagerUI.shapeConfirmed) 
        {
            // Fetch the shape name
            string shapeName = playerManager.playerController.UImanagement.shapeManagerUI.shapeName;

            // Use the chosenLocation from the player controller if available
            if (playerManager.playerController.chosenLocation != null)
            {
                formationManager.SetTargetLocation(playerManager.playerController.chosenLocation);
            }

            // Convert string to enum and change formation
            if (Enum.TryParse(shapeName, out FormationType formationType))
            {
                ChangeFormation(formationType);
            }
            else
            {
                Debug.LogError($"Invalid shape name: {shapeName}");
            }
        }
    }
    
    public void ChangeFormation(FormationType newFormation)
    {
        formationManager.ChangeFormation(newFormation);
        UpdateFormationText();
    }
    
    // New method to set formation location
    public void SetFormationLocation(Transform location)
    {
        if (formationManager != null)
        {
            formationManager.SetTargetLocation(location);
        }
    }
    
    private void UpdateFormationText()
    {
        if (currentFormationText != null)
        {
            string staticText = formationManager.staticFormations ? " (Static)" : " (Following)";
            string locationText = formationManager.targetLocation != null ? 
                $" at {formationManager.targetLocation.name}" : "";
            
            currentFormationText.text = "Formation: " + 
                                        formationManager.currentFormation.ToString() + 
                                        staticText + 
                                        locationText;
        }
    }
}









