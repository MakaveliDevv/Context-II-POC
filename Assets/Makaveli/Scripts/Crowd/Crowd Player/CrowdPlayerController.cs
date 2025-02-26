using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdPlayerController 
{
    private readonly CrowdPlayerMovement movement;
    private readonly CrowdPlayerUIManager UImanagement;
    private Vector2 movementInput;

    // NPC stuff
    private readonly List<GameObject> crowd = new();
    private readonly Transform npcContainer;

    public CrowdPlayerController
    (
        MonoBehaviour monoBehaviour,
        CharacterController controller,
        Transform player,
        Transform camera,
        Vector2 angleLimit,
        float movementSpeed,
        float appliedMovementSpeedPercentage,
        float jumpForce,
        float sensivity,
        bool invert,
        GameObject npc,
        int npcCount,
        GameObject cardsUI,
        List<GameObject> cardPanels,
        List<UILocationCard> cards
    ) 
    {
        movement = new
        (
            controller,
            player,
            camera,
            angleLimit,
            movementSpeed,
            appliedMovementSpeedPercentage,
            jumpForce,
            sensivity,
            invert
        );

        UImanagement = new(cardsUI, cardPanels, cards);
        
        npcContainer = player.parent.transform.GetChild(2);
        monoBehaviour.StartCoroutine(SpawnNPC(npc, npcCount));
    }

    public void MovementInput(ref bool ableToLook) 
    {
        movementInput = InputActionHandler.GetMovementInput();
        movement.OverallMovement(movementInput, InputActionHandler.IsSprinting(), InputActionHandler.IsJumping(), ableToLook);
    }

    private IEnumerator SpawnNPC(GameObject npc, int npcCount) 
    {
        for (int i = 0; i < npcCount; i++)
        {
            GameObject newNPC = MGameManager.instance.InstantiatePrefab(npc, npcContainer);
            crowd.Add(newNPC);

            yield return null;
        }

        yield break;
    }

    public void HideCards() 
    {
        UImanagement.HideCards();
    }

    public void OpenCardUI(MonoBehaviour monoBehaviour) 
    {
        monoBehaviour.StartCoroutine(UImanagement.DisplayCards(monoBehaviour));
    }
    
    public void UIPanelNavigation() 
    {
        UImanagement.ButtonHandler();
    }
}