using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CrowdPlayerController
{
    private readonly MonoBehaviour mono;
    
    // Class References
    private readonly TopDownMovement topDownMovement; 
    public UIManagement UImanagement;
    public CameraManagement cameraManagement;
    private readonly CustomNetworkBehaviour customNetworkBehaviour;
    
    // Player Management
    public CharacterController controller;
    public Transform chosenLocation;
    private Vector2 movementInput;
    public Transform playerPositionInsideTaskLocation;
    
    private bool isProcessingClick;
    public bool isAtLocation;
    public bool locationChosen;

    // NPC Management
    private readonly GameObject npc;
    private readonly int npcCount;
    public List<GameObject> npcs = new();
    private Vector3 npcSpawnOffset = new();

    // UI Management
    private readonly List<GameObject> cardPanels = new();
    public List<LocationCardUI> cards = new();

    // Camera Management
    private Coroutine tiltCam;
    private Coroutine repositionCam;

    // Tasks
    public List<Task> tasks = new();

    // Temp stuff
    private Transform spawnPoint;
    private Transform npcContainer;
    private CrowdPlayerManager playerManager;
    public Transform npcsFormationLocation;

    public CrowdPlayerController
    (
        MonoBehaviour mono,                     // Reference to the mono class
        Camera cam,                             // Reference to the top down camera
        Vector3 camOffset,                      // Reference to the camera offset position
        float camSmoothSpeed,                   // Reference to the camera movement
        float camRotationSpeed,                 // Reference to the camera rotation
        float movementSpeed,                    // Reference to the player movement speed
        float appliedMovementSpeedPercentage,   // Reference to the applied player movement speed
        float jumpForce,
        GameObject npc,                         // Reference to the npc prefab
        int npcCount,                           // Reference to the amount of npcs to spawn in for the player
        LayerMask npcLayer,                     // Reference to the npc layer
        Vector3 npcSpawnOffset,                 // Reference to the spawn offset for the npc
        GameObject cardsUI,                      // Reference to the main panel for the UI location cards
        Transform spawnPoint,
        Transform npcContainer,
        CrowdPlayerManager playerManager
    ) 
    {
        this.mono = mono;
        this.npc = npc;
        this.npcCount = npcCount;
        this.npcSpawnOffset = npcSpawnOffset;
        this.spawnPoint = spawnPoint;
        this.npcContainer = npcContainer;
        this.playerManager = playerManager;

        controller = mono.transform.GetChild(0).GetComponent<CharacterController>();
        customNetworkBehaviour = mono.GetComponent<CustomNetworkBehaviour>();

        if(controller == null) 
        {
            Debug.LogError("No charactercontroller found");
            return;
        }

        if(customNetworkBehaviour == null) 
        {
            Debug.LogError("No custom network behaviour found!");
            return;
        }

        topDownMovement = new
        (
            controller,
            controller.transform,
            movementSpeed,
            appliedMovementSpeedPercentage,
            jumpForce,
            customNetworkBehaviour
        );

        UImanagement = new
        (
            mono.transform, 
            cardsUI, 
            cardPanels, 
            cards
        );

        cameraManagement = new
        (
            playerManager,
            cam,
            npcLayer,
            controller.transform,
            camOffset,
            camSmoothSpeed,
            camRotationSpeed
        );
    }

    public void Start() 
    {
        controller.transform.LookAt(playerManager.playerSpawnPoint);
        
        mono.StartCoroutine(SpawnCrowdWithDelay());
        UImanagement.Start(mono, playerManager);
        cameraManagement.Start();

        if(npcsFormationLocation == null) 
        {
            npcsFormationLocation = playerManager.transform.Find("CrowdPlayer").Find("npc formationPoint");
        }

        Debug.Log($"npcs formation location: {npcsFormationLocation.gameObject.name}");
        Debug.Log($"npcs formation location local position: {npcsFormationLocation.localPosition}");
        Debug.Log($"npcs formation location world position: {npcsFormationLocation.position}");
        
        playerManager.playerFormationController.SetFormationLocationTransform(npcsFormationLocation);
        // npcsFormationLocation = playerManager.transform.Find("CrowdPlayer").Find("npc formationPoint");
    }

    private IEnumerator SpawnCrowdWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        CrowdRpcBehaviour crowdRpcBehaviour = mono.GetComponent<CrowdRpcBehaviour>();
        crowdRpcBehaviour.SetCorrectReferences(controller, npcCount, npc, npcSpawnOffset, this);

        if(customNetworkBehaviour.CustomIsOwner())
        {
            crowdRpcBehaviour.SpawnCrowdServerRpc(customNetworkBehaviour.ownerClientID);
        }

    }

    public void Update() 
    {
        UImanagement.Update(playerManager);
    }

    // Player movement stuff
    public void MovementInput() 
    {
        movementInput = InputActionHandler.GetMovementInput();
        // movement.OverallMovement(movementInput, InputActionHandler.IsSprinting(), InputActionHandler.IsJumping(), ableToLook);
        topDownMovement.OverallMovement(movementInput, InputActionHandler.IsSprinting(), InputActionHandler.IsJumping());
    }

    public void CheckPlayerPosition(Transform player) 
    {
        if (chosenLocation == null) 
        {
            Debug.LogWarning("❌ chosenLocation is NULL.");
            return;
        }

        // Debug.Log($"Player Position: {player.position}");

        float distance = Vector3.Distance(
            new Vector3(player.position.x, 0, player.position.z), 
            new Vector3(chosenLocation.position.x, 0, chosenLocation.position.z)
        );

        // Debug.Log($"Target Position: {chosenLocation.position}");
        // Debug.Log($"📏 Distance to target: {distance}");

        if (distance <= 10f) 
        {
            TaskLocation taskLocation = chosenLocation.gameObject.GetComponent<TaskLocation>();
            taskLocation.fixable = true;
            Debug.Log("✅ Player is at the chosen location!");
            

            // float elapsedTime = 0;
            // float duration = 4f;

            // // Move the player towards one of the player positions
            // for (int i = 0; i < chosenLocation.childCount; i++)
            // {
            //     playerPositionInsideTaskLocation = chosenLocation.GetChild(0).GetChild(i).GetChild(0);    
            //     Debug.Log($"playerPositionInsideTaskLocation -> {playerPositionInsideTaskLocation.gameObject.name}");
            //     // player.position = targetPosition.position;
            // }

            // while(elapsedTime < duration) 
            // {
            //     elapsedTime += Time.deltaTime;
            //     float t = elapsedTime / duration;
            //     player.position = Vector3.Lerp(player.position, playerPositionInsideTaskLocation.position, t);
            // } 

            isAtLocation = true;
        }
        else 
        {
            isAtLocation = false;
        }
    }

    // Camera stuff
    public void CameraMovement() 
    {
        cameraManagement.CameraMovement();
    }

    public void TitlCamera(float distanceOffset, float interpolationDuration, PlayerFormationController playerFormationController) 
    {
        if(repositionCam != null) 
        {
            // Debug.Log("RepositionCamera coroutine is running and now set to stop");
            mono.StopCoroutine(cameraManagement.RepositionCamera(interpolationDuration));
            repositionCam = null;
        }  

        Debug.Log($"camera tilt position: {npcsFormationLocation}");
    
        tiltCam = mono.StartCoroutine(cameraManagement.TiltCamera(npcsFormationLocation, distanceOffset, interpolationDuration));
    }

    public void RepositionCamera(float distanceOffset, float interpolationDuration, PlayerFormationController playerFormationController) 
    {
        if(tiltCam != null) 
        {
            // Debug.Log("Tilt coroutine is running and now set to stop");

            mono.StopCoroutine(cameraManagement.TiltCamera(npcsFormationLocation, distanceOffset, interpolationDuration));
            tiltCam = null;
        }

        repositionCam = mono.StartCoroutine(cameraManagement.RepositionCamera(interpolationDuration));
    }
    
    public void DragNPC() 
    {
        cameraManagement.DragMovement();
    }

    // Location cards UI
    public void HideCards() 
    {
        UImanagement.HideCards();
    }

    public void OpenCardUI() 
    {
        UImanagement.DisplayCards(mono);
    }
    
    public void CardPanelNavigation() 
    {
        UImanagement.CardPanelNavigation();
    }

    public void SecondHalfOfChooseLocation(LocationCardUI card)
    {
        // Assign the tasks form the card
        tasks = card.tasks;

        playerManager.tasks = tasks;

        Debug.Log($"Location: {chosenLocation.gameObject.name}");
        locationChosen = true;
        
        // // Set the formation location in the formation manager
        // if (mono.transform.TryGetComponent<PlayerFormationController>(out var formationController))
        // {
        //     Transform npcsLocation = null;

        //     for (int i = 0; i < chosenLocation.childCount; i++)
        //     {
        //         npcsLocation = chosenLocation.GetChild(0).GetChild(i);    
        //     } 

        //     Debug.Log($"NPC location: {npcsLocation.gameObject.name}");
        //     formationController.SetFormationLocation(npcsLocation);
            
        //     // If already in a formation, update it to use the new location
        //     NPCFormationManager formManager = formationController.formationManager;
        //     if (formManager != null && formManager.currentFormation != FormationType.Follow)
        //     {
        //         // Re-apply current formation to update positions
        //         formationController.ChangeFormation(formManager.currentFormation);
        //     }
        // }
        
        mono.StartCoroutine(ResetClickState(1.0f)); // 1 second delay
    }

    public void ChooseLocation(List<LocationCardUI> cards, bool inUIMode)
    {
        // If in UI mode and not already processing a click
        if (inUIMode)
        {
            if (cards.Count > 0)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    
                    // Remove any existing listeners to prevent duplicates
                    if(card.btn != null) 
                    {
                        card.btn.onClick.RemoveAllListeners();
                        card.btn.onClick.AddListener(() =>
                        {
                            if (!isProcessingClick)
                            {
                                isProcessingClick = true;
                                playerManager.ChooseLocationServerRpc(cards.IndexOf(card), ClientServerRefs.instance.localClient.OwnerClientId);
                                MGameManager.instance.showLocationCards = false;
                                playerManager.arrow.SetActive(true);
                                playerManager.playerState = CrowdPlayerManager.PlayerState.DEFAULT;
                            }
                        });
                    }
                }
            }
        }
        
        // Only reset if we're not in UI mode
        if (!inUIMode)
        {
            isProcessingClick = false;
        }

        // if (!isLocationChosen) 
        // {
        //     RandomizeLocation(cards);
        // }
    }

    // private void RandomizeLocation(List<LocationCardUI> cards)
    // {
    //     // Check if we have any cards to randomize from
    //     if (cards.Count > 0)
    //     {
    //         int randomIndex = Random.Range(0, cards.Count);
    //         chosenLocation = cards[randomIndex].location;

    //         Debug.Log($"Randomized Location: {chosenLocation.name}");
    //         locationChosen = true;

    //         // Set the formation location in the formation manager
    //         if (mono.transform.TryGetComponent<PlayerFormationController>(out var formationController))
    //         {
    //             formationController.SetFormationLocation(chosenLocation);

    //             // If already in a formation, update it to use the new location
    //             NPCFormationManager formManager = formationController.formationManager;
    //             if (formManager != null && formManager.currentFormation != FormationType.Follow)
    //             {
    //                 // Re-apply current formation to update positions
    //                 formationController.ChangeFormation(formManager.currentFormation);
    //             }
    //         }
    //     }
    // }

    private IEnumerator ResetClickState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isProcessingClick = false;
    }


    public void MoveTowardsChosenLocation(Transform transform) 
    {
        if (chosenLocation == null) return;

        float moveSpeed = topDownMovement.travelMovementSpeed; 
        float avoidanceStrength = 5f; 
        float raycastDistance = 1f; 
        float cornerCheckDistance = 0.5f; 
        Vector3 direction = (chosenLocation.position - transform.position).normalized;
        
        // Perform forward raycast for obstacle avoidance
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Obstacle")) 
            {
                Debug.Log("Obstacle detected, adjusting path");

                // Try to steer left or right based on which side has more clearance
                Vector3 left = Quaternion.Euler(0, -45, 0) * direction;
                Vector3 right = Quaternion.Euler(0, 45, 0) * direction;

                bool leftClear = !Physics.Raycast(transform.position, left, cornerCheckDistance);
                bool rightClear = !Physics.Raycast(transform.position, right, cornerCheckDistance);

                if (leftClear && !rightClear)
                    direction = left;
                else if (rightClear && !leftClear)
                    direction = right;
                else if (leftClear && rightClear)
                    direction = Random.value > 0.5f ? left : right; // Choose randomly if both are clear
                else
                    return; // No clear path, stop moving
            }
        }

        // Update player's rotation to face the target direction
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Smooth movement towards the target position
        Vector3 targetPosition = transform.position + moveSpeed * Time.deltaTime * direction;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * avoidanceStrength);
    }
}
