using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrowdPlayerManager : MonoBehaviour 
{
    public enum PlayerState { ROAM_AROUND, CHOOSE_LOCATION, TRAVELING, CHOOSE_SHAPE, SIGNAL }
    public PlayerState playerState; 

    public CrowdPlayerController playerController;

    [SerializeField] private CharacterController controller;
    // [SerializeField] private Transform cameraTransform;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float aplliedMovementSpeedPercentage = .5f;
    // [SerializeField] private float jumpForce = 5f;
    // [SerializeField] private float sensivity = 5f;
    // [SerializeField] private bool invert;
    // [SerializeField] private Vector2 angleLimit = new(-70f, 70f);

    // UI Stuff
    [Header("UI Stuff")]
    [SerializeField] private GameObject cardsUI;
    [SerializeField] private List<GameObject> cardPanels = new();
    [SerializeField] private List<UILocationCard> cards = new();
    public bool cardButtonClicked = false;
    public bool isProcessingClick = false;
    private bool openShapePanelFirstTime;

    public bool inUIMode = false;
    // private bool ableToLook = true;
    private bool lastDisplayUIState = false; 

    [Header("NPC Stuff")]
    public List<GameObject> NPCs = new();
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount;

    float elapsedTime = 0;    

    private void Awake()
    {
        controller = transform.GetChild(0).gameObject.GetComponent<CharacterController>();
        // playerRenderer = transform.GetChild(0).gameObject.GetComponent<Transform>();
       
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

    private void Start()
    {
        playerController.Start(this);
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
        // inUIMode = UILocationCard();  
        // Debug.Log("Update: inUIMode = " + inUIMode); // Debugging

        UILocationCard();  

        switch (playerState)
        {
            case PlayerState.ROAM_AROUND:
                // Player is able to walk around
                playerController.MovementInput();

            break;

            case PlayerState.CHOOSE_LOCATION:
                UIMode(true);
                playerController.ChooseLocation(cards, inUIMode);
                playerController.CardPanelNavigation();

                // An extra method to keep track of the chosen locations
                StartCoroutine(MGameManager.instance.InitializeLocation());

                elapsedTime += Time.deltaTime; 

                if(playerController.chosenLocation != null && elapsedTime >= MGameManager.instance.chooseLocationTimer) 
                {
                    elapsedTime = 0f;
                    playerState = PlayerState.TRAVELING;
                }

            break;

            case PlayerState.TRAVELING:
                // Should move the player automatically toward the chosen location
                // Debug.Log("Player is traveling");

                // Travel mechanic
                playerController.MoveTowardsChosenLocation(transform, NPCs);
                InputActionHandler.DisableInputActions();

                // Check if player at position
                playerController.CheckPlayerPosition(transform);

                if(playerController.isAtLocation == true) 
                {
                    // InputActionHandler.EnableInputActions();
                    UIMode(false);
                    playerState = PlayerState.CHOOSE_SHAPE;
                }

            break;

            case PlayerState.CHOOSE_SHAPE:
                // Stop npc movement
                // for (int i = 0; i < NPCs.Count; i++)
                // {
                //     if(NPCs[i].TryGetComponent<NPCManager>(out var npc)) 
                //     {
                //         npc.nPCFollower.currentVelocity = Vector3.zero;
                //         npc.nPCFollower.smoothSpeed = 0f;
                //     }
                //     else { Debug.LogError("Couldn't fetch the NPCManager script, something went wrong!"); return; }
                // }

                if(!openShapePanelFirstTime) 
                {
                    playerController.UImanagement.shapeManagerUI.closePanelButton.gameObject.SetActive(false);
                    playerController.UImanagement.shapeManagerUI.openPanelButton.gameObject.SetActive(true);
                    // playerController.UImanagement.shapeManagerUI.UpdatePanelButtons(true);
                    openShapePanelFirstTime = true;
                }

            break;

            case PlayerState.SIGNAL:
                openShapePanelFirstTime = false;
                // UIMode(false);

                // When in signal mode, the player can move around
                // The player can choose if the npcs stay at position or follow the player back
                // The player can choose to switch from shape 

            break;

            default:

            break;
        }
    } 

    private void UILocationCard() 
    {
        inUIMode = MGameManager.instance.showLocationCards; 
        
        if (MGameManager.instance.showLocationCards != lastDisplayUIState) 
        {
            lastDisplayUIState = MGameManager.instance.showLocationCards;

            if (MGameManager.instance.showLocationCards)
            {
                playerController.OpenCardUI();
            }
            else
            {
                playerController.HideCards();
            }
        }

        // Debug.Log($"UILocationCard: showLocationCards = {MGameManager.instance.showLocationCards}, lastDisplayUIState = {lastDisplayUIState}, inUIMode = {inUIMode}");
    }

    public bool UIMode(bool inUIMode) 
    {
        // Debug.Log("UIMode: inUIMode = " + inUIMode); // Debugging

        if(inUIMode) 
        {
            // Debug.Log("Unlocking Cursor");
            // Cursor.lockState = CursorLockMode.None;
            InputActionHandler.DisableInputActions();
        }
        else 
        {
            // Debug.Log("Locking Cursor");
            // Cursor.lockState = CursorLockMode.Locked;
            InputActionHandler.EnableInputActions();
        }

        return inUIMode;
    }

    private void OnDestroy()
    {
        InputActionHandler.DisableInputActions();
    }
}