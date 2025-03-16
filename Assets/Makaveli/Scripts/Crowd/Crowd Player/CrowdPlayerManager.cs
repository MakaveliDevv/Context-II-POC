using System.Collections;
using UnityEngine;

public class CrowdPlayerManager : MonoBehaviour 
{
    public enum PlayerState { ROAM_AROUND, CHOOSE_LOCATION, TRAVELING, CHOOSE_SHAPE, REARRANGE_SHAPE, SIGNAL, END }
    public PlayerState playerState; 

    [HideInInspector] public CrowdPlayerController playerController; // Class reference

    [Header("Movement Management")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float aplliedMovementSpeedPercentage = .5f;


    // UI Management
    [Header("UI Management")]
    [SerializeField] private GameObject cardsUI;
    private bool inUIMode = false;
    private bool lastDisplayUIState = false; 
    private bool openShapePanelFirstTime;

    // NPC Management
    [Header("NPC Management")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private LayerMask npcLayer;
    [SerializeField] private Vector3 npcSpawnOffset = new();    
    [SerializeField] private int npcCount;

    private float elapsedTime = 0;    

    // Camera Management
    [Header("Camera Management")]
    public Camera cam;
    [SerializeField] private Vector3 camOffset = new(0, 10, -5);    
    [SerializeField] private float camSmoothSpeed = 5f;
    [SerializeField] private float camRotationSpeed = 100f;  
    public Quaternion og_camRot = new();
    public float distanceOffset;
    public float interpolationDuration;

    // State management
    public bool rearrangeFormation;
    public bool signal;

    private void Awake()
    {
        playerController = new
        (
            this,                               // Reference to the mono class
            cam,                                // Reference to the top down camera
            camOffset,                          // Reference to the camera offset position
            camSmoothSpeed,                     // Reference to the camera movement
            camRotationSpeed,                   // Reference to the camera rotation
            movementSpeed,                      // Reference to the player movement speed
            aplliedMovementSpeedPercentage,     // Reference to the applied player movement speed
            npcPrefab,                          // Reference to the npc prefab
            npcCount,                           // Reference to the amount of npcs to spawn in for the player
            npcLayer,                           // Reference to the npc layer
            npcSpawnOffset,                     // Reference to the spawn offset for the npc
            cardsUI                             // Reference to the main panel for the UI location cards
        );        
    }

    private void Start()
    {
        MGameManager.instance.allCrowdPlayers.Add(this);

        playerController.Start(this);
        og_camRot = cam.transform.rotation;
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
        inUIMode = LocationCardsUIVisibility();  
        UIMode(inUIMode);
        playerController.Update(this);

        switch (playerState)
        {
            case PlayerState.ROAM_AROUND:
                playerController.chosenLocation = null;
                StartCoroutine(NPCsManagement.ResumeNPCMovement(playerController.npcs, transform));

                // Player is able to walk around
                playerController.MovementInput();

            break;

            case PlayerState.CHOOSE_LOCATION:
                playerController.ChooseLocation(playerController.cards, inUIMode);
                playerController.CardPanelNavigation();

                // An extra method to keep track of the chosen locations
                StartCoroutine(MGameManager.instance.InitializeLocation());

                // If location selected, change state
                if(playerController.locationChosen) 
                {
                    MGameManager.instance.showLocationCards = false;
                    playerState = PlayerState.TRAVELING;
                }

                break;

            case PlayerState.TRAVELING:
                // Debug.Log("🚀 TRAVELING state running...");
                playerController.MovementInput();

                // Travel mechanic
                // playerController.MoveTowardsChosenLocation(transform);
                // InputActionHandler.DisableInputActions();

                // Check if player at position
                playerController.CheckPlayerPosition(playerController.controller.transform);

                if(playerController.isAtLocation == true) 
                {
                    // Debug.Log("✅ Switching to CHOOSE_SHAPE state");
                    playerState = PlayerState.CHOOSE_SHAPE;
                }

            break;

            case PlayerState.CHOOSE_SHAPE:
                if(!openShapePanelFirstTime) 
                {
                    playerController.UImanagement.shapeManagerUI.OpenShapePanel(this);
                    openShapePanelFirstTime = true;
                }

            break;

            case PlayerState.REARRANGE_SHAPE:
                openShapePanelFirstTime = false;
                signal = false; 

                playerController.DragNPC();

                if(!rearrangeFormation) 
                {
                    if (playerController.UImanagement.shapeManagerUI.shapeConfirmed) 
                    {
                        // Stop the movement of each npc
                        StartCoroutine(NPCsManagement.StopNPCMovement(playerController.npcs));
                        playerController.TitlCamera(distanceOffset, interpolationDuration);
                        rearrangeFormation = true;
                    }
                }

            break;

            case PlayerState.SIGNAL:
                rearrangeFormation = false;
                if(!signal) 
                {
                    playerController.RepositionCamera(distanceOffset, interpolationDuration);

                    signal = true;
                }

                if(signal) 
                {
                    transform.GetChild(4).GetChild(8).gameObject.SetActive(true); // Display the signal button
                    playerController.MovementInput();
                }

                // If lion placed an object switch state to roam around

            break;

            default:

            break;
        }
    }

    private void LateUpdate()
    {
        playerController.CameraMovement();
    }

    private IEnumerator Signal() 
    {
        signal = true;
        
        // Signal stuff
        yield return new WaitForSeconds(5f);


        playerState = PlayerState.ROAM_AROUND;
        yield break;
    }

    // UI stuff
    private bool LocationCardsUIVisibility() 
    {
        bool inUIMode = MGameManager.instance.showLocationCards; 
        
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
        
        return inUIMode;
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

    // Helper method to visualize the boundary in the editor
    // private void OnDrawGizmos()
    // {
    //     if (minBoundary == null || maxBoundary == null) return;

    //     Gizmos.color = Color.yellow;

    //     // Draw the boundary as a wireframe box
    //     Vector3 size = new Vector3(maxBoundary.x - minBoundary.x, 0.1f, maxBoundary.y - minBoundary.y);
    //     Vector3 center = new Vector3((minBoundary.x + maxBoundary.x) * 0.5f, 0, (minBoundary.y + maxBoundary.y) * 0.5f);
    //     Gizmos.DrawWireCube(center, size);
    // }
}