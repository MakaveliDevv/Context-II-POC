using System.Collections.Generic;
using UnityEngine;

public class CrowdPlayerUIManager 
{
    private readonly CardManagerUI cardManagerUI;
    public readonly ShapeManagerUI shapeManagerUI;
    
    public CrowdPlayerUIManager
    (
        Transform player,
        GameObject cardsUI,
        List<GameObject> cardPanels,
        List<UILocationCard> cards
    ) 
    {
        cardManagerUI = new
        (
            cardsUI,
            cardPanels,
            cards
        );

        shapeManagerUI = new
        (
            player
        ); 
    }

    public void Start(MonoBehaviour mono, CrowdPlayerManager playerManager) 
    {
        mono.StartCoroutine(shapeManagerUI.Start(playerManager));
    }

    public void DisplayCards(MonoBehaviour monoBehaviour) 
    {
        monoBehaviour.StartCoroutine(cardManagerUI.DisplayCards(monoBehaviour));
    }

    public void HideCards() 
    {
        cardManagerUI.HideCards();
    }
    
    public void CardPanelNavigation() 
    {
        cardManagerUI.ButtonHandler();
    }

    public void OpenShapePanelUI() 
    {
        shapeManagerUI.OpenShapePanel();
    }

    public void CloseShapePanelUI() 
    {
        shapeManagerUI.CloseShapePanel();
    }
}