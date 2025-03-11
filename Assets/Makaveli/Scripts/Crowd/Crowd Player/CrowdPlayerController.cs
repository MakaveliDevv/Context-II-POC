using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdPlayerController 
{
    // References
    private readonly MonoBehaviour mono;
    private readonly TopDownMovement topDownMovement; // Top down movement class
    // private readonly CrowdPlayerMovement movement; // FP movement class
    public readonly CrowdPlayerUIManager UImanagement; // UI management class
    
    public Transform chosenLocation;
    private readonly Transform npcContainer;
    private Vector2 movementInput;
    
    private bool isProcessingClick;
    public bool isAtLocation;
    public bool isLocationChosen;

    public CrowdPlayerController
    (
        MonoBehaviour mono,
        CharacterController controller,
        // Transform player,
        // Transform camera,
        // Vector2 angleLimit,
        float movementSpeed,
        float appliedMovementSpeedPercentage,
        // float jumpForce,
        // float sensivity,
        // bool invert,
        GameObject npc,
        int npcCount,
        GameObject cardsUI,
        List<GameObject> cardPanels,
        List<UILocationCard> cards,
        List<GameObject> NPCs
    ) 
    {
        this.mono = mono;

        // movement = new
        // (
        //     controller,
        //     player,
        //     camera,
        //     angleLimit,
        //     movementSpeed,
        //     appliedMovementSpeedPercentage,
        //     jumpForce,
        //     sensivity,
        //     invert
        // );

        topDownMovement = new
        (
            controller,
            controller.transform,
            movementSpeed,
            appliedMovementSpeedPercentage
        );

        UImanagement = new
        (
            controller.transform.parent, 
            cardsUI, 
            cardPanels, 
            cards
        );
        
        npcContainer = controller.transform.parent.transform.GetChild(3);
        mono.StartCoroutine(NPCsManagement.SpawnNPC(NPCs, npc, npcCount, npcContainer));    
    }

    public void Start(CrowdPlayerManager playerManager) 
    {
        UImanagement.Start(mono, playerManager);
    }

    public void MovementInput() 
    {
        movementInput = InputActionHandler.GetMovementInput();
        // movement.OverallMovement(movementInput, InputActionHandler.IsSprinting(), InputActionHandler.IsJumping(), ableToLook);
        topDownMovement.OverallMovement(movementInput, InputActionHandler.IsSprinting());
    }

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

    public void ChooseLocation(List<UILocationCard> cards, bool inUIMode)
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
                                chosenLocation = card.location;
                                Debug.Log($"Location: {chosenLocation.name}");
                                isLocationChosen = true;
                                
                                // Set the formation location in the formation manager
                                if (mono.transform.TryGetComponent<PlayerFormationController>(out var formationController))
                                {
                                    formationController.SetFormationLocation(chosenLocation);
                                    
                                    // If already in a formation, update it to use the new location
                                    NPCFormationManager formManager = formationController.formationManager;
                                    if (formManager != null && formManager.currentFormation != FormationType.Follow)
                                    {
                                        // Re-apply current formation to update positions
                                        formationController.ChangeFormation(formManager.currentFormation);
                                    }
                                }
                                
                                mono.StartCoroutine(ResetClickState(1.0f)); // 1 second delay

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

        if (!isLocationChosen) 
        {
            RandomizeLocation(cards);
        }
    }

    private void RandomizeLocation(List<UILocationCard> cards)
    {
        // Check if we have any cards to randomize from
        if (cards.Count > 0)
        {
            int randomIndex = Random.Range(0, cards.Count);
            chosenLocation = cards[randomIndex].location;

            Debug.Log($"Randomized Location: {chosenLocation.name}");
            isLocationChosen = true;

            // Set the formation location in the formation manager
            if (mono.transform.TryGetComponent<PlayerFormationController>(out var formationController))
            {
                formationController.SetFormationLocation(chosenLocation);

                // If already in a formation, update it to use the new location
                NPCFormationManager formManager = formationController.formationManager;
                if (formManager != null && formManager.currentFormation != FormationType.Follow)
                {
                    // Re-apply current formation to update positions
                    formationController.ChangeFormation(formManager.currentFormation);
                }
            }
        }
    }

    
    // public void ConfirmShape() 
    // {
    //     UImanagement.shapeManagerUI.ConfirmShape(mono);
    // }
    
    #region Oldcode
    // public void ChooseLocation(List<UILocationCard> cards, bool _bool)
    // {
    //     // If in UI mode and not already processing a click
    //     if (_bool)
    //     {
    //         if (cards.Count > 0)
    //         {
    //             for (int i = 0; i < cards.Count; i++)
    //             {
    //                 var card = cards[i];
                    
    //                 // Remove any existing listeners to prevent duplicates
    //                 if(card.btn != null) 
    //                 {
    //                     card.btn.onClick.RemoveAllListeners();
    //                     card.btn.onClick.AddListener(() =>
    //                     {
    //                         if (!isProcessingClick)
    //                         {
    //                             isProcessingClick = true;
    //                             chosenLocation = card.location;
    //                             // Debug.Log($"Location: {chosenLocation}");
    //                             // NPCsManagement.TriggerAllMovements(card.location);
                                
    //                             mono.StartCoroutine(ResetClickState(1.0f)); // 1 second delay
    //                         }
    //                     });
    //                 }
    //             }
    //         }
    //     }
        
    //     // Only reset if we're not in UI mode
    //     if (!_bool)
    //     {
    //         isProcessingClick = false;
    //     }
    // }

    #endregion

    private IEnumerator ResetClickState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isProcessingClick = false;
    }

    public void CheckPlayerPosition(Transform player) 
    {
        if(Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(chosenLocation.position.x, 0, chosenLocation.position.z)) <= 1f) 
        {
            isAtLocation = true;
            Debug.Log("Player is at the chosen location");
        }
    }

    // public void OpenShapePanel(ref bool inUIMode) 
    // {
    //     if(inUIMode) 
    //     {
    //         UImanagement.OpenShapePanelUI();
    //     }
    // }

    // public void CloseShapePanel(ref bool inUIMode) 
    // {
    //     if(inUIMode) 
    //     {
    //         UImanagement.CloseShapePanelUI();
    //         inUIMode = false;
    //     }
    // }

    public void MoveTowardsChosenLocation(Transform transform, List<GameObject> npcs) 
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
