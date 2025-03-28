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
    private readonly EmoteUI emoteManagerUI;
    public bool taskCreated;
    private readonly Transform canvas;
    
    // Emotes
    public List<Button> emoteButtons = new();

    // Buttons
    private Button signalBtn;
    public Button NPCsFollowBtn;
    public bool followButtonPressed;

    public UIManagement(Transform player, GameObject cardsUI, List<GameObject> cardPanels, List<LocationCardUI> cards) 
    {
        if(player.TryGetComponent<CrowdPlayerManager>(out var playerManager)) 
        {
            canvas = player.transform.Find("Player Canvas");
            Debug.Log($"canvas = {canvas.name}");

            locationManagerUI = new (cardsUI ,cardPanels ,cards);
            taskManagerUI = new (canvas, playerManager);
            shapeManagerUI = new (canvas, playerManager); 
            emoteManagerUI = new(canvas, playerManager, emoteButtons);


        } else { Debug.LogError("Couldn't fetch the playermanager component from the player!"); }

    }

    public void Start(MonoBehaviour mono, CrowdPlayerManager playerManager) 
    {
        locationManagerUI.Start();
        taskManagerUI.Start();
        mono.StartCoroutine(shapeManagerUI.Start());
        emoteManagerUI.Start();

        // signal button
        if(canvas.Find("Buttons").Find("SignalBtn").TryGetComponent<Button>(out var signalBtn))
        {
            this.signalBtn = signalBtn;
            this.signalBtn.onClick.RemoveAllListeners();
            this.signalBtn.onClick.AddListener(() => 
            {
                Debug.Log("Pressed signal: " + playerManager.playerController.npcs.Count);
                foreach (var e in playerManager.playerController.npcs)
                {
                    NPCManager npc = e.GetComponent<NPCManager>();
                    mono.StartCoroutine(npc.Signal(5f));
                }    
            });
            
        } else { Debug.LogError("Couldn't fetch the 'signalBtn shape button' "); return; }

        // NPCsFollow button
        if(canvas.Find("Buttons").Find("NPCsFollowBtn").TryGetComponent<Button>(out var NPCsFollowBtn))
        {
            this.NPCsFollowBtn = NPCsFollowBtn;
            this.NPCsFollowBtn.onClick.RemoveAllListeners();
            this.NPCsFollowBtn.onClick.AddListener(() => 
            {
                // Resume npc movement
                followButtonPressed = true;
                shapeManagerUI.shapeSelected = false;
                // mono.StartCoroutine(NPCsManagement.ResumeNPCMovement(playerManager.playerController.npcs, playerManager.transform));
                playerManager.playerState = CrowdPlayerManager.PlayerState.DEFAULT;
                
            });
            
        } else { Debug.LogError("Couldn't fetch the 'signalBtn shape button' "); return; }
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
        shapeManagerUI.Update();
        taskManagerUI.DisplayTaskBtn(playerManager);
        
        if(!taskCreated) 
        {
            taskManagerUI.CreateTaskCard(this);
        }
        
        if(MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.END) 
        {
            taskCreated = false;
        }
    }
}