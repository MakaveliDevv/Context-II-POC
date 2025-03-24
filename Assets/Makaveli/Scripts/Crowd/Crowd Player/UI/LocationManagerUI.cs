using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LocationManagerUI
{
    private readonly GameObject cardsUI;
    private Dictionary<GameObject, List<LocationCardUI>> panelCardMap = new();
    private readonly List<LocationCardUI> cards;
    private readonly List<GameObject> cardPanels;
    private int currentIndex = 0;
    private int totalTrackedObjects = 0;
    private int totalActivePanels = 0;
    
    public LocationManagerUI
    (
        GameObject cardsUI,
        List<GameObject> cardPanels,
        List<LocationCardUI> cards
    ) 
    {
        this.cardsUI = cardsUI;
        this.cardPanels = cardPanels;
        this.cards = cards;

    }

    public void Start() 
    {
        InitializeCardElements();
    }

    private void InitializeCardElements()
    {
        cardPanels.Clear();
        cards?.Clear();
        
        if (panelCardMap == null)
            panelCardMap = new Dictionary<GameObject, List<LocationCardUI>>();
        else
            panelCardMap.Clear();
        
        // Get the CardsPanel container (which is the first child of cardsUI)
        Transform cardsPanel = cardsUI.transform.GetChild(0);
        int panelCount = cardsPanel.childCount;

        // Debug.Log($"Panel count: {panelCount}");
        
        // Loop through children of CardsPanel
        for (int i = 0; i < panelCount; i++)
        {
            Transform childTransform = cardsPanel.GetChild(i);
            
            // Skip PanelNavigation or any non-Panel objects
            if (childTransform.name == "PanelNavigation" || !childTransform.name.Contains("Panel"))
            {
                // Debug.Log($"Skipping {childTransform.name} - not a card panel");
                continue;
            }
            
            GameObject panel = childTransform.gameObject;
            cardPanels.Add(panel);
            // Debug.Log($"Added panel: {panel.name}");
            
            // Create a new list for this panel's cards
            List<LocationCardUI> panelCards = new();
            
            int cardCount = panel.transform.childCount;
            // Debug.Log($"Card count in {panel.name}: {cardCount}");
            
            // Loop through all cards in this panel
            for (int j = 0; j < cardCount; j++)
            {
                GameObject cardMask = panel.transform.GetChild(j).gameObject;
                GameObject cardObject = cardMask.transform.GetChild(0).gameObject;
                
                if (cardObject.TryGetComponent<LocationCardUI>(out var _card))
                {
                    // Add to the panel-specific list
                    panelCards.Add(_card);
                    cards?.Add(_card);
                    
                    // Debug.Log($"Added card: {cardObject.name} from panel: {panel.name}");
                }
                else
                {
                    Debug.LogWarning($"Object {cardMask.name} in {panel.name} does not have UILocationCard component");
                }
            }
            
            // Add the panel and its cards to the dictionary
            panelCardMap.Add(panel, panelCards);
        }
        
        // Debug.Log($"Initialization complete. Found {cardPanels.Count} panels and {panelCardMap.Sum(pair => pair.Value.Count)} cards total.");
        
        // Extra debugging to verify the dictionary contents
        // foreach (var pair in panelCardMap)
        // {
        //     Debug.Log($"Panel {pair.Key.name} has {pair.Value.Count} cards");
        // }
    }

    public IEnumerator DisplayCards(MonoBehaviour monoBehaviour)
    {
        // Ensure UI is initialized
        if (cardPanels == null || cardPanels.Count == 0 || panelCardMap == null || panelCardMap.Count == 0)
        {
            InitializeCardElements();
            yield return null; // Wait a frame after initialization
        }

        // Activate the main UI
        cardsUI.SetActive(true);
        
        // Check if we have any panels
        if (cardPanels.Count <= 0)
        { 
            Debug.LogError("No card panels were found during initialization"); 
            yield break; 
        }
        
        // Get total objects to track
        if (MGameManager.instance.trackables == null || MGameManager.instance.trackables.Count == 0)
        {
            Debug.LogError("No objects to track in GameManager");
            yield break;
        }
        
        totalTrackedObjects = MGameManager.instance.trackables.Count;
        // Debug.Log($"Total objects to track: {totalTrackedObjects}");
        
        // Calculate how many panels we need to activate based on tracked objects
        CalculateActivePanels();
        
        // Deactivate all panels first
        foreach (var panel in cardPanels)
        {
            panel.SetActive(false);
        }
        
        // Activate only the first panel initially
        currentIndex = 0;
        ActivateCurrentPanel(cardPanels, currentIndex);
        
        // Initialize all cards for all panels that will be used
        InitializeAllPanelsData(monoBehaviour);
        
        // Set up navigation buttons
        ButtonHandler();
        
        yield break;
    }

    private void CalculateActivePanels()
    {
        int totalCards = 0;
        totalActivePanels = 0;
        
        // Count how many panels we need to show all tracked objects
        for (int i = 0; i < cardPanels.Count; i++)
        {
            int cardsInPanel = panelCardMap[cardPanels[i]].Count;
            totalCards += cardsInPanel;
            
            if (totalCards >= totalTrackedObjects)
            {
                totalActivePanels = i + 1;
                break;
            }
        }
        
        // If we can't fit all objects, use all panels
        if (totalCards < totalTrackedObjects)
        {
            totalActivePanels = cardPanels.Count;
        }
        
        // Debug.Log($"Will activate {totalActivePanels} panels for {totalTrackedObjects} objects");
    }

    private void InitializeAllPanelsData(MonoBehaviour monoBehaviour)
    {
        int objectIndex = 0;
        
        // Loop through all panels that should be active
        for (int panelIndex = 0; panelIndex < totalActivePanels; panelIndex++)
        {
            GameObject panel = cardPanels[panelIndex];
            List<LocationCardUI> panelCards = panelCardMap[panel];
            
            // Loop through each card in the panel
            for (int cardIndex = 0; cardIndex < panelCards.Count; cardIndex++)
            {
                // If we've run out of objects to track, disable remaining cards
                if (objectIndex >= totalTrackedObjects)
                {
                    panelCards[cardIndex].gameObject.SetActive(false);
                    continue;
                }
                
                LocationCardUI card = panelCards[cardIndex];
                // Debug.Log($"Card -> {card.name}");
                
                GameObject objectToTrack = MGameManager.instance.trackables[objectIndex];
                
                // Set up the card with the object data
                SetupCard(card, objectToTrack);
                
                // Prepare for animation if this is the first panel
                if (panelIndex == 0)
                {
                    RectTransform cardTransform = card.transform.parent.GetComponent<RectTransform>();
                    // Debug.Log($"CardTransform -> {cardTransform.gameObject.name}");

                    cardTransform.anchoredPosition = new Vector3(-Screen.width, cardTransform.anchoredPosition.y, 0);
                    
                    // Add animation only for up to first 3 cards
                    if (cardIndex < 3)
                    {
                        Vector3 targetPosition;
                        float animationDuration;
                        
                        switch(cardIndex)
                        {
                            case 0:
                                targetPosition = new Vector3(1225f, cardTransform.anchoredPosition.y, 0);
                                animationDuration = 0.75f;
                                break;
                            case 1:
                                targetPosition = new Vector3(15f, cardTransform.anchoredPosition.y, 0);
                                animationDuration = 1f;
                                break;
                            case 2:
                                targetPosition = new Vector3(-1225f, cardTransform.anchoredPosition.y, 0);
                                animationDuration = 1.25f;
                                break;
                            default:
                                targetPosition = new Vector3(0f, cardTransform.anchoredPosition.y, 0);
                                animationDuration = 1f;
                                break;
                        }
                        
                        card.gameObject.SetActive(true);
                        monoBehaviour.StartCoroutine(MoveCardToPosition(cardTransform, targetPosition, animationDuration));
                    }
                    else
                    {
                        card.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // For other panels, just set them up but keep them inactive until panel is shown
                    card.gameObject.SetActive(false);
                }
                
                objectIndex++;
            }
        }
        
        // Disable any cards in panels that won't be used
        for (int panelIndex = totalActivePanels; panelIndex < cardPanels.Count; panelIndex++)
        {
            GameObject panel = cardPanels[panelIndex];
            List<LocationCardUI> panelCards = panelCardMap[panel];
            
            foreach (var card in panelCards)
            {
                card.gameObject.SetActive(false);
            }
        }
    }

    private void SetupCard(LocationCardUI card, GameObject objectToTrack)
    {
        // Initialize the card data
        card.objectPosition = objectToTrack.transform.position;
        card.location = objectToTrack.transform.parent.transform;

        // Assign the tasks to the card for that location
        if(objectToTrack.transform.parent.TryGetComponent<TaskLocation>(out var taskLocation)) 
        {
            card.tasks = taskLocation.tasks;
        }
        else 
        {
            Debug.LogError("Couldn't fetch the 'TaskLocation' component!");
        }
        
        // Get the render texture from the object to track
        ObjectToTrack trackComponent = objectToTrack.GetComponent<ObjectToTrack>();
        if (trackComponent != null && trackComponent.renderTexture != null)
        {
            card.renderTexture = trackComponent.renderTexture;
            if (card.gameObject.TryGetComponent<RawImage>(out var image))
            {
                image.texture = card.renderTexture;
                // Debug.Log($"Set render texture for card {card.gameObject.name}");
            }
            else
            {
                Debug.LogError($"RawImage component not found on card {card.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError($"ObjectToTrack component or renderTexture not found on object {objectToTrack.name}");
        }
    }

    public void HideCards()
    {
        // Hide all cards in all panels
        foreach (var panel in cardPanels)
        {
            List<LocationCardUI> panelCards = panelCardMap[panel];
            foreach (var card in panelCards)
            {
                card.gameObject.SetActive(false);
            }
            panel.SetActive(false);
        }

        cardsUI.SetActive(false); // Deactivate the main UI
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

    // Card panel navigation
    public void ButtonHandler() 
    {
        // Fetch the buttons
        Button btnUp = cardsUI.transform.GetChild(1).transform.GetChild(0).gameObject.GetComponent<Button>();
        Button btnDown = cardsUI.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<Button>();

        btnUp.onClick.RemoveAllListeners();
        btnDown.onClick.RemoveAllListeners();

        btnUp.onClick.AddListener(MoveUp); 
        btnDown.onClick.AddListener(MoveDown);
        
        // Update button states based on current panel
        UpdateButtonStates();
    }

    private void MoveUp() 
    {
        if (currentIndex > 0)
        {
            cardPanels[currentIndex].SetActive(false);
            currentIndex--;
            ActivateCurrentPanel(cardPanels, currentIndex);
            UpdateButtonStates();
        }
    }

    private void MoveDown() 
    {
        if (currentIndex < totalActivePanels - 1)
        {
            cardPanels[currentIndex].SetActive(false);
            currentIndex++;
            ActivateCurrentPanel(cardPanels, currentIndex);
            UpdateButtonStates();
        }
    }
    
    private void UpdateButtonStates()
    {
        // Get button references
        Button btnUp = cardsUI.transform.GetChild(1).transform.GetChild(0).gameObject.GetComponent<Button>();
        Button btnDown = cardsUI.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<Button>();
        
        // Disable up button if we're on the first panel
        btnUp.interactable = currentIndex > 0;
        
        // Disable down button if we're on the last active panel
        btnDown.interactable = currentIndex < totalActivePanels - 1;
    }
    
    private void ActivateCurrentPanel(List<GameObject> cardPanels, int index) 
    {
        GameObject panel = cardPanels[index];
        panel.SetActive(true);
        
        // Activate all cards in this panel that have data
        List<LocationCardUI> panelCards = panelCardMap[panel];
        int startIndex = 0;
        
        // Calculate the starting index for objects to track based on previous panels
        for (int i = 0; i < index; i++)
        {
            startIndex += panelCardMap[cardPanels[i]].Count;
        }
        
        // Activate each card in this panel if there's an object to track for it
        for (int i = 0; i < panelCards.Count; i++)
        {
            int objectIndex = startIndex + i;
            
            // Only activate if we have an object to track
            if (objectIndex < totalTrackedObjects)
            {
                panelCards[i].gameObject.SetActive(true);
            }
            else
            {
                panelCards[i].gameObject.SetActive(false);
            }
        }
        
        // Debug.Log($"Activated panel {index}: {panel.name}");
    }
}
