using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    public static MGameManager instance;
    public List<GameObject> markers = new();
    public List<GameObject> objectsToTrack = new();
    public List<UILocationCard> cards = new();
    private readonly List<ITriggerMovement> triggerMovements = new(); // Store all ITriggerMovement implementations


    // [SerializeField] private float discussionTimer;
    // [SerializeField] private GameObject cardsUI;
    // [SerializeField] private List<GameObject> cardPanels = new();
    // private int currentIndex = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.parent.gameObject); 
        }
        else
        {
            Destroy(transform.parent.gameObject); 
        }
    }

    // void Start()
    // {
    //     for (int i = 0; i < cardsUI.transform.childCount; i++)
    //     {
    //         GameObject cardPanel = cardsUI.transform.GetChild(0).transform.GetChild(i).gameObject;
    //         cardPanels.Add(cardPanel);
    //     }
    // }

    public void RegisterTriggerMovement(ITriggerMovement triggerMovement)
    {
        if (!triggerMovements.Contains(triggerMovement))
        {
            triggerMovements.Add(triggerMovement);
        }
    }

    public void UnregisterTriggerMovement(ITriggerMovement triggerMovement)
    {
        if (triggerMovements.Contains(triggerMovement))
        {
            triggerMovements.Remove(triggerMovement);
        }
    }

    public void TriggerAllMovements(Transform location)
    {
        foreach (var triggerMovement in triggerMovements)
        {
            triggerMovement.TriggerMovement(location);
        }
    }

    private void Update()
    {
        if(cards.Count > 0) 
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                card.btn.onClick.AddListener(() => 
                {
                    Debug.Log("Button clicked");
                    TriggerAllMovements(card.location);
                });
            }
        }

        // if(Input.GetKeyDown(KeyCode.G)) OpenUI();
    }

    public GameObject InstantiatePrefab(GameObject prefab, Transform parent) 
    {
        GameObject newGameObject = Instantiate(prefab);
        newGameObject.transform.SetParent(parent, true);

        return newGameObject;
    }

    // public void MoveUp() 
    // {
    //     cardPanels[currentIndex].SetActive(false);
    //     currentIndex = (currentIndex - 1 + cardPanels.Count) % cardPanels.Count;
    //     ActivateCurrentPanel(currentIndex);
    // }

    // public void MoveDown() 
    // {
    //     cardPanels[currentIndex].SetActive(false);
    //     currentIndex = (currentIndex + 1) % cardPanels.Count;
    //     ActivateCurrentPanel(currentIndex);
    // }

    // private void ActivateCurrentPanel(int index) 
    // {
    //     cardPanels[currentIndex].SetActive(true);
    // }

    // private void OpenUI() 
    // {
    //     cardsUI.SetActive(true);
    //     // cardsPanel.SetActive(true);

    //     int cardCount = cardsUI.transform.GetChild(0).transform.GetChild(0).transform.childCount;
    //     // int cardCount = cardsPanel.transform.childCount;
    //     if (cardCount == 0) return;

    //     RectTransform cardsPanelContainer = cardsUI.transform.GetChild(0).transform.gameObject.GetComponent<RectTransform>();
    //     // float panelWidth = cardsPanelContainer.rect.width; 

    //     // RectTransform firstCardTransform = cardsPanelContainer.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<RectTransform>();
    //     // float cardWidth = firstCardTransform.rect.width; 

    //     // float spacing = (panelWidth - cardWidth) / (cardCount - 1); 

    //     for (int i = 0; i < cardCount; i++)
    //     {     
    //         GameObject card = cardsPanelContainer.transform.GetChild(0).transform.GetChild(i).gameObject;
    //         card.SetActive(true);

    //         RectTransform cardTransform = card.GetComponent<RectTransform>();
    //         cardTransform.anchoredPosition = new Vector3(-Screen.width, cardTransform.anchoredPosition.y, 0); 

    //         RectTransform card_1 = cardsPanelContainer.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<RectTransform>();
    //         RectTransform card_2 = cardsPanelContainer.transform.GetChild(0).transform.GetChild(1).gameObject.GetComponent<RectTransform>();
    //         RectTransform card_3 = cardsPanelContainer.transform.GetChild(0).transform.GetChild(2).gameObject.GetComponent<RectTransform>();

    //         Vector3 targetPosition_1 = new(1225f, card_1.anchoredPosition.y, 0);
    //         Vector3 targetPosition_2 = new(15f, card_2.anchoredPosition.y, 0);
    //         Vector3 targetPosition_3 = new(-1225f, card_3.anchoredPosition.y, 0);

    //         StartCoroutine(MoveCardToPosition(card_1, targetPosition_1, .75f + (i * .1f)));
    //         StartCoroutine(MoveCardToPosition(card_2, targetPosition_2, 1f + (i * .1f)));
    //         StartCoroutine(MoveCardToPosition(card_3, targetPosition_3, 1.25f + (i * .1f)));

    //         // float targetX = panelWidth - cardWidth - (i * spacing); 
    //         // Vector3 targetPosition = new(targetX, cardTransform.anchoredPosition.y, 0);

    //         // StartCoroutine(MoveCardToPosition(cardTransform, targetPosition, 1f + (i * 0.1f))); 
    //     }
    // }

    // private IEnumerator MoveCardToPosition(RectTransform card, Vector3 targetPosition, float duration) 
    // {
    //     Vector3 startPosition = card.anchoredPosition;
    //     float elapsedTime = 0f;

    //     while (elapsedTime < duration) 
    //     {
    //         card.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
    //         elapsedTime += Time.deltaTime;
    //         yield return null;
    //     }

    //     card.anchoredPosition = targetPosition;
    // }
}

public interface ITriggerMovement 
{
    public void TriggerMovement(Transform transform);
}