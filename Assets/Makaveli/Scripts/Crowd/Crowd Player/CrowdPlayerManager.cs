using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdPlayerManager : MonoBehaviour 
{
    public enum PlayerState { ROAM_AROUND, CHOOSE_LOCATION, TRAVELING, CHOOSE_SHAPE, REARRANGE_SHAPE, SIGNAL, END }
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
    [SerializeField] private List<GameObject> NPCs = new();
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount;
    [SerializeField] private LayerMask npcLayer;

    private float elapsedTime = 0;    
    public Camera cam;
    // public Vector3 og_camPos = new();
    public Quaternion og_camRot = new();
    public float distanceOffset;
    

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
            NPCs,
            npcLayer,
            cam
        );

        MGameManager.instance.allCrowdPlayers.Add(this);
    }

    private void Start()
    {
        playerController.Start(this);

        // Original cam rotation and Y and Z position
        // og_camPos = cam.transform.position;
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
        inUIMode = UILocationCard();  
        UIMode(inUIMode);

        switch (playerState)
        {
            case PlayerState.ROAM_AROUND:
                playerController.chosenLocation = null;
                StopCoroutine(TiltCamera());
                StopCoroutine(StopNPCMovement());

                StartCoroutine(RepositionCamera());
                StartCoroutine(ResumeNPCMovement());

                // Player is able to walk around
                playerController.MovementInput();

            break;

            case PlayerState.CHOOSE_LOCATION:
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
                // Debug.Log("🚀 TRAVELING state running...");
                playerController.MovementInput();

                // Travel mechanic
                // playerController.MoveTowardsChosenLocation(transform);
                // InputActionHandler.DisableInputActions();

                // Check if player at position
                playerController.CheckPlayerPosition(controller.transform);

                if(playerController.isAtLocation == true) 
                {
                    Debug.Log("✅ Switching to CHOOSE_SHAPE state");
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
                if (playerController.UImanagement.shapeManagerUI.shapeConfirmed) 
                {
                    // playerController.UImanagement.shapeManagerUI.CloseShapePanel(this);

                    // Stop the movement of each npc
                    StartCoroutine(StopNPCMovement());
                    StartCoroutine(TiltCamera());
                }

            break;

            case PlayerState.SIGNAL:
                openShapePanelFirstTime = false;
                StartCoroutine(WaitBeforeTurnState());
                // playerController.MovementInput();

                // When in signal mode, the player can move around
                // The player can choose if the npcs stay at position or follow the player back
                // The player can choose to switch from shape 

            break;

            case PlayerState.END:
                StopCoroutine(TiltCamera());
                StopCoroutine(StopNPCMovement());

                StartCoroutine(RepositionCamera());
                StartCoroutine(ResumeNPCMovement());

            break;
        }
    } 

    private IEnumerator WaitBeforeTurnState() 
    {
        yield return new WaitForSeconds(3f);
        playerState = PlayerState.END;

        yield return new WaitForSeconds(2f);

        playerState = PlayerState.ROAM_AROUND;
        yield break;
    }

    private IEnumerator TiltCamera() 
    {
        yield return new WaitForSeconds(2f);
        playerController.shapeSetter.Start(playerController.chosenLocation);
        playerController.shapeSetter.Update();

        // Tilt camera
        TopDownCameraController camScript = cam.GetComponent<TopDownCameraController>();
        camScript.enabled = false;

        cam.transform.rotation = Quaternion.Euler(90f, 0, 0);

        float maxSize = Mathf.Max(playerController.chosenLocation.localScale.x, playerController.chosenLocation.localScale.z);

        // Adjust the camera position
        float distance = maxSize * distanceOffset;
        Vector3 cameraPosition = playerController.chosenLocation.position + Vector3.up * distance;

        // Set the camera position without altering the FOV calculation
        cam.transform.position = cameraPosition;

        // Camera camComponent = cam.GetComponent<Camera>();

        // // Adjust the field of view based on the distance
        // // We use a fixed distance here, no matter how far the camera is, to ensure it fits the object
        // float fov = Mathf.Atan(maxSize / (2 * distance)) * Mathf.Rad2Deg * 2;
        
        // camComponent.fieldOfView = Mathf.Clamp(fov, 30f, 60f); // Optionally, clamp the FOV to a reasonable range

        yield break;
    }

    private IEnumerator RepositionCamera() 
    {
        TopDownCameraController camScript = cam.GetComponent<TopDownCameraController>();
        camScript.enabled = true;

        yield break;
    }

    private IEnumerator StopNPCMovement() 
    {
        yield return new WaitForSeconds(5f);

        for (int i = 0; i < NPCs.Count; i++)
        {
            var npc = NPCs[i].GetComponent<NPCManager>();
            npc.moveable = false;

        }

        yield break;
    }

    private IEnumerator ResumeNPCMovement() 
    {
        for (int i = 0; i < NPCs.Count; i++)
        {
            var npc = NPCs[i].GetComponent<NPCManager>();
            npc.moveable = true;
        }

        NPCFormationManager formationManager = GetComponent<NPCFormationManager>();
        formationManager.currentFormation = FormationType.Follow;

        yield break;
    }

    private bool UILocationCard() 
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