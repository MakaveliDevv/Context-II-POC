using System.Collections.Generic;
using UnityEngine;

public class CrowdPlayerUIManager 
{
    private readonly UICardManagement UICardManagement;

    public CrowdPlayerUIManager
    (
        GameObject cardsUI,
        List<GameObject> cardPanels,
        List<UICard> cards
    ) 
    {
        UICardManagement = new
        (
            cardsUI,
            cardPanels,
            cards
        );
    }

    public void DisplayCards(MonoBehaviour monoBehaviour) 
    {
        monoBehaviour.StartCoroutine(UICardManagement.DisplayCards(monoBehaviour));
    }

    public void HideCards() 
    {
        UICardManagement.HideCards();
    }
    
    public void CardPanelNavigation() 
    {
        UICardManagement.ButtonHandler();
    }
}