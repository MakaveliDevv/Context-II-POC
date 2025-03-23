using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CrowdPlayerManager : NetworkBehaviour 
{
    public enum PlayerState { ROAM_AROUND, CHOOSE_LOCATION, TRAVELING, CHOOSE_SHAPE, REARRANGE_SHAPE, SIGNAL, END }
    public PlayerState playerState; 

    [HideInInspector] public CrowdPlayerController playerController; // Class reference
    public PlayerFormationController playerFormationController;

    [Header("Movement Management")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float aplliedMovementSpeedPercentage = .5f;

    // UI Management
    [Header("UI Management")]
    [SerializeField] private GameObject cardsUI;
    public bool inUIMode = false;
    private bool lastDisplayUIState = false; 
    private bool openShapePanelFirstTime;
    public bool signal;

    // NPC Management
    [Header("NPC Management")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private LayerMask npcLayer;
    [SerializeField] private Vector3 npcSpawnOffset = new();    
    [SerializeField] private int npcCount;
    private bool rearrangeFormation;
    public Transform spawnPoint;
    public Transform npcContainer;

    // Camera Management
    [Header("Camera Management")]
    public Camera cam;
    [SerializeField] private Vector3 camOffset = new(0, 10, -5);    
    [SerializeField] private float camSmoothSpeed = 5f;
    [SerializeField] private float camRotationSpeed = 100f;  
    public List<LocationCardUI> chosenCards = new();
    public Quaternion og_camRot = new();
    public float distanceOffset;
    public float interpolationDuration;

    [Header("Network Management")]
    private CustomNetworkBehaviour customNetworkBehaviour;
    [SerializeField] List<GameObject> crowdOnlyObjects;

    [Header("Tasks")]
    public List<Task> tasks = new();


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
            cardsUI,                             // Reference to the main panel for the UI location cards
            spawnPoint,
            npcContainer,
            tasks,
            this
        );        

        MGameManager.instance.allCrowdPlayers.Add(this);

        if(playerFormationController == null) 
        {
            playerFormationController = GetComponent<PlayerFormationController>();
        }
    }

    private void Start()
    {
        StartCoroutine(playerController.Start(this));
        og_camRot = cam.transform.rotation;
    }

    public override void OnNetworkSpawn()
    {
        if(customNetworkBehaviour == null) customNetworkBehaviour = GetComponent<CustomNetworkBehaviour>();
        StartCoroutine(InstantiateCorrectly());
        Debug.Log("Crowd network spawn");
    }

    IEnumerator InstantiateCorrectly()
    {
        bool timeout = false;
        float elapsedTime = 0;

        while(!customNetworkBehaviour.CustomIsOwner())
        {
            yield return new WaitForFixedUpdate();
            elapsedTime += 0.02f;
            if(elapsedTime > 1) 
            {
                timeout = true;
                break;
            }
        }

        Debug.Log("Timeout: " + timeout + ", IsOwner " + customNetworkBehaviour.CustomIsOwner());

        if(!timeout)
        {
            if(!customNetworkBehaviour.CustomIsOwner()) 
            {
                foreach(GameObject _go in crowdOnlyObjects)
                {
                    _go.SetActive(false);
                }
            }
        }
        else
        {
            foreach(GameObject _go in crowdOnlyObjects)
            {
                _go.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        InputActionHandler.EnableInputActions();
    }

    private void OnDisable()
    {
        InputActionHandler.DisableInputActions();
    }

    [ServerRpc(RequireOwnership =  false)]
    public void ConfirmShapeServerRpc()
    {
        ConfirmShapeClientRpc();
    }

    [ClientRpc]
    void ConfirmShapeClientRpc()
    {
        MGameManager.instance.gamePlayManagement = MGameManager.GamePlayManagement.SOLVING_TASK;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChooseLocationServerRpc(int i)
    {
        ChooseLocationClientRpc(i);
    }
    
    [ClientRpc]
    void ChooseLocationClientRpc(int i)
    {
        // Debug.Log("123 i is: " + i);
        playerController.chosenLocation = chosenCards[i].location;
        playerController.SecondHalfOfChooseLocation(chosenCards[i]);
    }

    private void Update()
    {
        inUIMode = LocationCardsUIVisibility();  
        UIMode(inUIMode);
        playerController.Update(this);

        switch (playerState)
        {
            case PlayerState.ROAM_AROUND:
                playerController.MovementInput();

            break;

            case PlayerState.CHOOSE_LOCATION:
                chosenCards = playerController.cards;
                playerController.ChooseLocation(chosenCards, inUIMode);
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
                        playerController.TitlCamera(distanceOffset, interpolationDuration, playerFormationController);
                        rearrangeFormation = true;
                    }
                }

            break;

            case PlayerState.SIGNAL:
                rearrangeFormation = false;
                if(!signal) 
                {
                    playerController.RepositionCamera(distanceOffset, interpolationDuration, playerFormationController);

                    signal = true;
                }

                if(signal) 
                {
                    transform.GetChild(4).GetChild(8).gameObject.SetActive(true); // Display the signal button
                    playerController.MovementInput();
                }

            break;

            case PlayerState.END:
                playerController.chosenLocation = null;
                signal = false;
                playerController.UImanagement.shapeManagerUI.shapeConfirmed = false;

                StartCoroutine(NPCsManagement.ResumeNPCMovement(playerController.npcs, transform));

                playerController.MovementInput();

                // Display something like new task spawning in or something like that 
                // And then turn to roaming around state
                StartCoroutine(TempMethod());
              

            break;

            default:

            break;
        }
    }

    private void LateUpdate()
    {
        playerController.CameraMovement();
        // Debug.Log($"[Parent Transform] World Pos: {transform.position}, Local Pos: {transform.localPosition}");
    }

    private IEnumerator TempMethod() 
    {
        playerController.isAtLocation = false;
        playerController.locationChosen = false;
        yield return new WaitForSeconds(3f);
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

    public override void OnDestroy()
    {
        base.OnDestroy();
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

    
    // float CalculateGroupSideOffset()
    // {
    //     // Distribute NPCs across a line perpendicular to target's movement
    //     float spacing = minNPCDistance;
    //     int npcCount = MGameManager.instance.allNPCss.Count;
        
    //     // Center the group around the target's path
    //     float centerOffset = (npcCount - 1) * spacing * 0.5f;
        
    //     // Calculate individual NPC's offset
    //     float individualOffset = npcIndex * spacing - centerOffset;
        
    //     return individualOffset;
    // }
}