using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdPlayerController 
{
    public Transform chosenLocation;
    public bool location = false;

    private readonly MonoBehaviour monoBehaviour;
    // private readonly CrowdPlayerMovement movement;
    private readonly TopDownMovement topDownMovement;
    private readonly CrowdPlayerUIManager UImanagement;
    private Vector2 movementInput;

    private bool isProcessingClick;
    private readonly Transform npcContainer;


    public CrowdPlayerController
    (
        MonoBehaviour monoBehaviour,
        CharacterController controller,
        Transform player,
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
        List<UICard> cards,
        List<GameObject> NPCs
    ) 
    {
        this.monoBehaviour = monoBehaviour;

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
            player,
            movementSpeed,
            appliedMovementSpeedPercentage
        );

        UImanagement = new(cardsUI, cardPanels, cards);
        npcContainer = player.parent.transform.GetChild(2);
        monoBehaviour.StartCoroutine(NPCsManagement.SpawnNPC(NPCs, npc, npcCount, npcContainer));    
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
        UImanagement.DisplayCards(monoBehaviour);
    }
    
    public void CardPanelNavigation() 
    {
        UImanagement.CardPanelNavigation();
    }

    public void ChooseLocation(List<UICard> cards, bool _bool)
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
                                Debug.Log($"Location: {chosenLocation}");
                                location = true;
                                // NPCsManagement.TriggerAllMovements(card.location);
                                
                                monoBehaviour.StartCoroutine(ResetClickState(1.0f)); // 1 second delay
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

    private IEnumerator ResetClickState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isProcessingClick = false;
    }

    private void ShowNavigationUI() 
    {
        // Fetch the position from the player to the location
        Vector3 endPosition = chosenLocation.position;
        
    }
}
