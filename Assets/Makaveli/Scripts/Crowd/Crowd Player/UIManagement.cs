using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrowdPlayerUIManager 
{
    private readonly GameObject cardsUI;
    private readonly List<UILocationCard> cards = new();
    private readonly List<GameObject> cardPanels = new();
    private int currentIndex = 0;
    
    public CrowdPlayerUIManager
    (
        GameObject cardsUI,
        List<GameObject> cardPanels
    ) 
    {
        this.cardsUI = cardsUI;
        this.cardPanels = cardPanels;
    }

    public void DisplayCards() 
    {
        if(MGameManager.instance.cards.Count < 0) return;

        foreach (var card in MGameManager.instance.cards)
        {
            if(!cards.Contains(card)) cards.Add(card);
        }
        
        if(cards.Count <= 0) return;
        
        for (int i = 0; i < cards.Count && i < MGameManager.instance.objectsToTrack.Count; i++)
        {
            var card = cards[i];
            var @object = MGameManager.instance.objectsToTrack[i];

            card.gameObject.SetActive(true);
            card.objectPosition = @object.transform.position;

            // Assign the rendertexture from the object to track to the render texture of the card
            ObjectToTrack objectToTrack = @object.GetComponent<ObjectToTrack>();
            card.renderTexture = objectToTrack.renderTexture; 

            // Assign the renderTexture from the card to the image slot
            RawImage image = card.gameObject.GetComponent<RawImage>();
            image.texture = card.renderTexture;

            // Transform
            // card.location = @object.transform;
            // Transform newTransform = card.location;
            // card.objectPosition = newTransform.position;
        }
    }

    public void HideLocationCards()
    {
        for (int i = 0; i < cards.Count && i < MGameManager.instance.objectsToTrack.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
        }
    }

    public void OpenUI(MonoBehaviour monoBehaviour) 
    {
        cardsUI.SetActive(true);
        // cardsPanel.SetActive(true);

        int cardCount = cardsUI.transform.GetChild(0).transform.GetChild(0).transform.childCount;
        // int cardCount = cardsPanel.transform.childCount;
        if (cardCount == 0) return;

        RectTransform cardsPanelContainer = cardsUI.transform.GetChild(0).transform.gameObject.GetComponent<RectTransform>();
        // float panelWidth = cardsPanelContainer.rect.width; 

        // RectTransform firstCardTransform = cardsPanelContainer.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        // float cardWidth = firstCardTransform.rect.width; 

        // float spacing = (panelWidth - cardWidth) / (cardCount - 1); 

        for (int i = 0; i < cardCount; i++)
        {     
            GameObject card = cardsPanelContainer.transform.GetChild(0).transform.GetChild(i).gameObject;
            card.SetActive(true);

            RectTransform cardTransform = card.GetComponent<RectTransform>();
            cardTransform.anchoredPosition = new Vector3(-Screen.width, cardTransform.anchoredPosition.y, 0); 

            RectTransform card_1 = cardsPanelContainer.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<RectTransform>();
            RectTransform card_2 = cardsPanelContainer.transform.GetChild(0).transform.GetChild(1).gameObject.GetComponent<RectTransform>();
            RectTransform card_3 = cardsPanelContainer.transform.GetChild(0).transform.GetChild(2).gameObject.GetComponent<RectTransform>();

            Vector3 targetPosition_1 = new(1225f, card_1.anchoredPosition.y, 0);
            Vector3 targetPosition_2 = new(15f, card_2.anchoredPosition.y, 0);
            Vector3 targetPosition_3 = new(-1225f, card_3.anchoredPosition.y, 0);

            monoBehaviour.StartCoroutine(MoveCardToPosition(card_1, targetPosition_1, .75f + (i * .1f)));
            monoBehaviour.StartCoroutine(MoveCardToPosition(card_2, targetPosition_2, 1f + (i * .1f)));
            monoBehaviour.StartCoroutine(MoveCardToPosition(card_3, targetPosition_3, 1.25f + (i * .1f)));

            // float targetX = panelWidth - cardWidth - (i * spacing); 
            // Vector3 targetPosition = new(targetX, cardTransform.anchoredPosition.y, 0);

            // StartCoroutine(MoveCardToPosition(cardTransform, targetPosition, 1f + (i * 0.1f))); 
        }
    }

    public void ButtonHandler() 
    {
        // Fetch the buttons
        Button btnUp = cardsUI.transform.GetChild(1).transform.GetChild(0).gameObject.GetComponent<Button>();
        Button btnDown = cardsUI.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<Button>();

        btnUp.onClick.AddListener(MoveUp); 
        btnDown.onClick.AddListener(MoveDown);
    }

    private void MoveUp() 
    {
        cardPanels[currentIndex].SetActive(false);
        currentIndex = (currentIndex - 1 + cardPanels.Count) % cardPanels.Count;
        ActivateCurrentPanel(cardPanels, currentIndex);
    }

    private void MoveDown() 
    {
        cardPanels[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % cardPanels.Count;
        ActivateCurrentPanel(cardPanels, currentIndex);
    }

    private void ActivateCurrentPanel(List<GameObject> cardPanels, int index) 
    {
        cardPanels[index].SetActive(true);
    }

    private IEnumerator MoveCardToPosition(RectTransform card, Vector3 targetPosition, float duration) 
    {
        Vector3 startPosition = card.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration) 
        {
            card.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        card.anchoredPosition = targetPosition;
    }
}
