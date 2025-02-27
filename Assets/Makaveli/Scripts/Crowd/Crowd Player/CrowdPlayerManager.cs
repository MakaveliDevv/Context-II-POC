using System.Collections.Generic;
using UnityEngine;

public class CrowdPlayerManager : MonoBehaviour 
{
    public CrowdPlayerController playerController;

    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float aplliedMovementSpeedPercentage = .5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float sensivity = 5f;
    [SerializeField] private bool invert;
    [SerializeField] private Vector2 angleLimit = new(-70f, 70f);


    // UI Stuff
    [Header("UI Stuff")]
    [SerializeField] private GameObject cardsUI;
    [SerializeField] private List<GameObject> cardPanels = new();
    [SerializeField] private List<UICard> cards = new();
    public bool cardButtonClicked = false;
    public bool isProcessingClick = false;

    public bool inUIMode = false;
    private bool ableToLook = true;

    [Header("NPC Stuff")]
    [SerializeField] private List<GameObject> NPCs = new();
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount;

    private void Awake()
    {
        controller = transform.GetChild(0).gameObject.GetComponent<CharacterController>();
        playerTransform = transform.GetChild(0).gameObject.GetComponent<Transform>();
       
        playerController = new 
        (
            this,
            controller,                         // Character controller component
            playerTransform,                    // Player transform
            cameraTransform,                    // Camera transform object
            angleLimit,                         // Look angle limits
            movementSpeed,                      // Movement speed
            aplliedMovementSpeedPercentage,     // Applied movement speed
            jumpForce,                          // Jump force
            sensivity,                          // Look sensivity
            invert,                             // Invert look
            npcPrefab,                          // NPC prefab
            npcCount,                           // NPC count
            cardsUI,
            cardPanels,
            cards,
            NPCs
        );
    }

    private void OnEnable()
    {
        InputActionHandler.EnableInputActions();
    }

    private void OnDisable()
    {
        InputActionHandler.DisableInputActions();
    }

    private MGameManager.GamePlayManagement previousGameState;

    private void Update()
    {
        // // Check for state transition INTO DISCUSS_LOCATION
        // if (MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.DISCUSS_LOCATION && 
        //     previousGameState != MGameManager.GamePlayManagement.DISCUSS_LOCATION)
        // {
        //     // Set UI mode ON when entering discussion state
        //     inUIMode = true;
        //     InputActionHandler.DisableInputActions();
        //     playerController.OpenCardUI();
        //     ableToLook = false;
        // }
        // // Check for state transition OUT OF DISCUSS_LOCATION
        // else if (MGameManager.instance.gamePlayManagement != MGameManager.GamePlayManagement.DISCUSS_LOCATION && 
        //         previousGameState == MGameManager.GamePlayManagement.DISCUSS_LOCATION)
        // {
        //     // Set UI mode OFF when exiting discussion state
        //     inUIMode = false;
        //     InputActionHandler.EnableInputActions();
        //     playerController.HideCards();
        //     ableToLook = true;
        // }
        
        // // Store the current state for the next frame's comparison
        // previousGameState = MGameManager.instance.gamePlayManagement;

        if (Input.GetKeyDown(KeyCode.M)) 
        {
            inUIMode = !inUIMode; 

            if (inUIMode)
            {
                InputActionHandler.DisableInputActions();
                playerController.OpenCardUI();
                ableToLook = false;
            }
            else
            {
                InputActionHandler.EnableInputActions();
                playerController.HideCards();
                ableToLook = true;
            }
        }
        
        // Rest of your update method
        Cursor.lockState = inUIMode ? CursorLockMode.None : CursorLockMode.Locked;
        
        playerController.MovementInput(ref ableToLook);
        playerController.CardPanelNavigation();
        playerController.TriggerNPCMovement(cards, inUIMode);
    } 


    // private void Update()
    // {
    //     if (/*Input.GetKeyDown(KeyCode.M)*/ MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.DISCUSS_LOCATION) 
    //     {
    //         inUIMode = !inUIMode; 

    //         if (inUIMode)
    //         {
    //             InputActionHandler.DisableInputActions();
    //             playerController.OpenCardUI();
    //             ableToLook = false;
    //         }
    //         else
    //         {
    //             InputActionHandler.EnableInputActions();
    //             playerController.HideCards();
    //             ableToLook = true;
    //         }
    //     }

    //     Cursor.lockState = inUIMode ? CursorLockMode.None : CursorLockMode.Locked;
        
    //     playerController.MovementInput(ref ableToLook);
    //     playerController.CardPanelNavigation();
    //     playerController.TriggerNPCMovement(cards, inUIMode);
    // }

    private void OnDestroy()
    {
        InputActionHandler.DisableInputActions();
    }
}