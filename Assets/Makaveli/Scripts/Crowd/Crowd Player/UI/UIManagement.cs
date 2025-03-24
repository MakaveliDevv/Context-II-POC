using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManagement 
{
    public ShapeManagerUI shapeManagerUI;
    private readonly LocationManagerUI locationManagerUI;
    private readonly TaskManagerUI taskManagerUI;
    public bool taskCreated;
    
    // Emotes
    public Emotes selectedEmote;
    public GameObject emotePanel;
    public List<GameObject> emoteButtons = new();

    public UIManagement(Transform player, GameObject cardsUI, List<GameObject> cardPanels, List<LocationCardUI> cards) 
    {
        locationManagerUI = new (cardsUI ,cardPanels ,cards);
        shapeManagerUI = new (player); 
        taskManagerUI = new (player);
    }

    public void Start(MonoBehaviour mono, CrowdPlayerManager playerManager) 
    {
        mono.StartCoroutine(shapeManagerUI.Start(playerManager));
        taskManagerUI.Start();
        locationManagerUI.Start();
        
        playerManager.StartCoroutine(InitializeEmoteButtons(playerManager));
    }

    private IEnumerator InitializeEmoteButtons(CrowdPlayerManager playerManager) 
    {
        // Wait before initializing
        yield return new WaitForSeconds(2f);

        emotePanel = playerManager.transform.Find("Player Canvas").transform.GetChild(8).gameObject;
        emotePanel.SetActive(true);
        Debug.Log($"emotePanel: {emotePanel.name}");

        // Clear the existing list to prevent duplicates if this runs multiple times
        emoteButtons.Clear();
        
        for (int i = 0; i < emotePanel.transform.childCount; i++)
        {
            GameObject buttonObject = emotePanel.transform.GetChild(i).gameObject;
            emoteButtons.Add(buttonObject);  
            Debug.Log($"Added button {i}: {buttonObject.name}");
        }

        yield return null;

        Debug.Log($"btns count: {emoteButtons.Count}");

        if(emoteButtons != null && emoteButtons.Count > 0) 
        {
            foreach (var btn in emoteButtons)
            {
                Debug.Log($"Fetching buttons: {btn.name}");

                Button btnComponent = btn.GetComponent<Button>();
                Debug.Log($"Setting up button component: {btnComponent.gameObject.name}");
                
                btnComponent.onClick.RemoveAllListeners();

                // Store the emote name for THIS button
                if(btn.TryGetComponent<Emotes>(out var emoteComponent)) 
                {
                    string currentEmoteName = emoteComponent.emoteName;
                    Debug.Log($"Button has emote: {currentEmoteName}");

                    btnComponent.onClick.AddListener(() =>
                    {
                        Debug.Log($"Button clicked! Emote: {currentEmoteName}");
                        foreach (var npc in playerManager.playerController.npcs)
                        {
                            NPCManager npcManager = npc.GetComponent<NPCManager>();
                            if (npcManager != null)
                            {
                                npcManager.EmoteNPC(currentEmoteName);
                            }
                        }
                    });

                }
                else Debug.LogError("Couldn't fetch the Emotes component from the emote button");
            }
            
        }
        else { Debug.LogError("No button found!");}
       
        yield break;
    } 

    public void DisplayCards(MonoBehaviour monoBehaviour) 
    {
        monoBehaviour.StartCoroutine(locationManagerUI.DisplayCards(monoBehaviour));
    }

    public void HideCards() 
    {
        locationManagerUI.HideCards();
    }
    
    public void CardPanelNavigation() 
    {
        locationManagerUI.ButtonHandler();
    }

    public void Update(CrowdPlayerManager playerManager) 
    {
        if(!playerManager.signal) 
        {
            playerManager.transform.GetChild(4).GetChild(8).gameObject.SetActive(false);
        }

        taskManagerUI.DisplayTaskBtn(playerManager);

        if(playerManager.playerState == CrowdPlayerManager.PlayerState.TRAVELING) 
        {
            if(!taskCreated) 
            {
                taskManagerUI.CreateTaskCard(playerManager);
                taskCreated = true;
            }
        }

        if(MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.END) 
        {
            taskCreated = false;
        }
    }

    // public void OpenShapePanelUI() 
    // {
    //     shapeManagerUI.OpenShapePanel();
    // }

    // public void CloseShapePanelUI() 
    // {
    //     shapeManagerUI.CloseShapePanel();
    // }
}