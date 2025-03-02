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
    private bool lastDisplayUIState = false; 

    [Header("NPC Stuff")]
    [SerializeField] private List<GameObject> NPCs = new();
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount;

    private void Awake()
    {
        controller = transform.GetChild(0).gameObject.GetComponent<CharacterController>();
        playerTransform = transform.GetChild(0).gameObject.GetComponent<Transform>();
       
        // playerController = new 
        // (
        //     this,
        //     controller,                         // Character controller component
        //     playerTransform,                    // Player transform
        //     cameraTransform,                    // Camera transform object
        //     angleLimit,                         // Look angle limits
        //     movementSpeed,                      // Movement speed
        //     aplliedMovementSpeedPercentage,     // Applied movement speed
        //     jumpForce,                          // Jump force
        //     sensivity,                          // Look sensivity
        //     invert,                             // Invert look
        //     npcPrefab,                          // NPC prefab
        //     npcCount,                           // NPC count
        //     cardsUI,
        //     cardPanels,
        //     cards,
        //     NPCs
        // );

        playerController = new
        (
            this,
            controller,
            playerTransform,
            movementSpeed,
            aplliedMovementSpeedPercentage,
            npcPrefab,
            npcCount,                           
            cardsUI,
            cardPanels,
            cards,
            NPCs
        );

        MGameManager.instance.allCrowdPlayers.Add(this);
    }

    private void OnEnable()
    {
        InputActionHandler.EnableInputActions();
    }

    private void OnDisable()
    {
        InputActionHandler.DisableInputActions();
    }

    private void Update()
    {
        if (MGameManager.instance.showLocationCards != lastDisplayUIState) 
        {
            lastDisplayUIState = MGameManager.instance.showLocationCards; 

            if (MGameManager.instance.showLocationCards)
            {
                inUIMode = true;
                InputActionHandler.DisableInputActions();
                playerController.OpenCardUI();
                ableToLook = false;
            }
            else
            {
                InputActionHandler.EnableInputActions();
                playerController.HideCards();
                ableToLook = true;
                inUIMode = false;
            }

            // Lock/unlock cursor based on UI state
            Cursor.lockState = inUIMode ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // These should still be in Update if they need to run continuously
        playerController.MovementInput();
        playerController.CardPanelNavigation();
        playerController.ChooseLocation(cards, inUIMode);
    } 

    private void OnDestroy()
    {
        InputActionHandler.DisableInputActions();
    }
}