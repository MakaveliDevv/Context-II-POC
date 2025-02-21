using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrowdPlayerManager : MonoBehaviour 
{
    private CrowdPlayerMovement playerController;

    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float aplliedMovementSpeedPercentage = .5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float sensivity = 5f;
    [SerializeField] private bool invert;
    [SerializeField] private Vector2 angleLimit = new(-70f, 70f);


    // UI Stuff
    [Header("UI Stuff")]
    [SerializeField] private List<UILocationCard> UI_Locations;
    public bool inUIMode = false;

    private void Awake()
    {
        controller = transform.GetChild(0).gameObject.GetComponent<CharacterController>();
        playerBody = transform.GetChild(0).gameObject.GetComponent<Transform>();
       
        playerController = new 
        (
            controller,                         // CharacterController Component
            playerBody,                         // Transform Player Model
            cameraTransform,
            movementSpeed,                      // Movement Speed
            aplliedMovementSpeedPercentage,     // Applied Movement Speed
            jumpForce,                          // Jump Force
            sensivity,                          // Look Sensivity
            invert,                             // Invert Look
            angleLimit                         // Look Angle Limits
        );
    }

    private void Update()
    {
        playerController.OverallMovement();

        if (Input.GetKeyDown(KeyCode.M)) 
        {
            inUIMode = !inUIMode; 

            if (inUIMode)
            {
                DisplayLocationCards();
            }
            else
            {
                HideLocationCards();
            }
        }

        Cursor.lockState = inUIMode ? CursorLockMode.None : CursorLockMode.Locked;
    }

   private void DisplayLocationCards() 
    {
        for (int i = 0; i < UI_Locations.Count && i < Manager.instance.objectsToTrack.Count; i++)
        {
            UI_Locations[i].gameObject.SetActive(true);
            UI_Locations[i].objectPosition = Manager.instance.objectsToTrack[i].transform.position;

            // Transform
            UI_Locations[i].objectTransform = Manager.instance.objectsToTrack[i].transform;
            Transform newTransform = UI_Locations[i].objectTransform;
            UI_Locations[i].objectPosition = newTransform.position;
        }
    }

    private void HideLocationCards()
    {
        for (int i = 0; i < UI_Locations.Count && i < Manager.instance.objectsToTrack.Count; i++)
        {
            UI_Locations[i].gameObject.SetActive(false);
        }
    }
}