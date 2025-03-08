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

    public void ChooseLocation(List<UILocationCard> cards, bool _bool)
    {
        // If in UI mode and not already processing a click
        if (_bool)
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
                                // Debug.Log($"Location: {chosenLocation}");
                                
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
        if (!_bool)
        {
            isProcessingClick = false;
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
        if(Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(chosenLocation.position.x, 0, chosenLocation.position.z)) <= 2f) 
        {
            isAtLocation = true;
            Debug.Log("Player is at the chosen location");
        }
    }

    public void OpenShapePanel() 
    {
        UImanagement.OpenShapePanelUI();
    }

    public void CloseShapePanel() 
    {
        UImanagement.CloseShapePanelUI();
    }


}
