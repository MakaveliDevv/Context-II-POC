using System.Collections.Generic;
using UnityEngine;

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

        if(Input.GetKey(KeyCode.M)) 
        {
            DisplayLocationCards();
        }
    }

    private void DisplayLocationCards() 
    {
        Debug.Log("Method 'DisplayLocationCards' Invoked");
        // for (int i = 0; i < UI_Locations.Count; i++)
        // {
        //     UI_Locations[i].gameObject.SetActive(true);
            
        //     for (int j = 0; j < Manager.instance.objectsToTrack.Count; j++)
        //     {
        //         UI_Locations[i].objectPosition = Manager.instance.objectsToTrack[j].position;
        //     }
        // }

        for (int i = 0; i < UI_Locations.Count && i < Manager.instance.objectsToTrack.Count; i++)
        {
            UI_Locations[i].gameObject.SetActive(true);
            UI_Locations[i].objectPosition = Manager.instance.objectsToTrack[i].transform.position; // Correct 1:1 mapping
        }
    }
}