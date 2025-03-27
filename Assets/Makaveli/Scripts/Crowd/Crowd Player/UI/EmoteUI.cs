using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmoteUI 
{
    private readonly Transform canvas;
    private readonly CrowdPlayerManager player;
    private Transform parentPanel;
    private Transform emotePanel;
    private readonly List<Button> emoteButtons;
    private Button openEmotePanelBtn;
    private bool panelOpen;

    public EmoteUI(Transform canvas, CrowdPlayerManager player, List<Button> emoteButtons)
    { 
        this.player = player; 
        this.emoteButtons = emoteButtons;
        this.canvas = canvas; 
    }

    public void Start() 
    {
        player.StartCoroutine(InitializeEmoteButtons());
    }

    private IEnumerator InitializeEmoteButtons()
    {
        parentPanel = canvas.Find("EmoteManagement");
        emotePanel = parentPanel.Find("EmotePanel");
        
        yield return new WaitForSeconds(1f);
        
        if(parentPanel.Find("OpenEmotePanelBtn").gameObject.TryGetComponent<Button>(out var openEmotePanelBtn)) 
        {
            this.openEmotePanelBtn = openEmotePanelBtn;
            this.openEmotePanelBtn.onClick.RemoveAllListeners();
            this.openEmotePanelBtn.onClick.AddListener(ToggleEmotePanel);
        }
        
        yield return null;

        emoteButtons.Clear();
        emoteButtons.AddRange(emotePanel.GetComponentsInChildren<Button>());

        foreach (var btn in emoteButtons)
        {
            if (btn.TryGetComponent<Emotes>(out var emoteComponent))
            {
                string currentEmoteName = emoteComponent.emoteName;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    foreach (var npc in player.playerController.npcs)
                    {
                        var npcManager = npc.GetComponent<NPCManager>();
                        npcManager.EmoteNPC(currentEmoteName);
                    }
                });
            }
        }
    }

    private void ToggleEmotePanel() 
    {
        if(!panelOpen) 
        {
            emotePanel.gameObject.SetActive(true);
            panelOpen = true;
        }
        else 
        {
            emotePanel.gameObject.SetActive(false);
            panelOpen = false;
        }
    }


    // private IEnumerator InitializeEmoteButtons() 
    // {
    //     // Wait before initializing
    //     yield return new WaitForSeconds(2f);

    //     emotePanel = player.transform.Find("Player Canvas").transform.GetChild(8).gameObject;
    //     emotePanel.SetActive(true);
    //     Debug.Log($"emotePanel: {emotePanel.name}");

    //     // Clear the existing list to prevent duplicates if this runs multiple times
    //     emoteButtons.Clear();
        
    //     for (int i = 0; i < emotePanel.transform.childCount; i++)
    //     {
    //         GameObject buttonObject = emotePanel.transform.GetChild(i).gameObject;
    //         emoteButtons.Add(buttonObject);  
    //         Debug.Log($"Added button {i}: {buttonObject.name}");
    //     }

    //     yield return null;

    //     Debug.Log($"btns count: {emoteButtons.Count}");

    //     if(emoteButtons != null && emoteButtons.Count > 0) 
    //     {
    //         foreach (var btn in emoteButtons)
    //         {
    //             Debug.Log($"Fetching buttons: {btn.name}");

    //             if(btn.TryGetComponent<Button>(out var btnComponent))
    //             {
    //                 Debug.Log($"Setting up button component: {btnComponent.gameObject.name}");
    //                 btnComponent.onClick.RemoveAllListeners();

    //                 // Store the emote name for THIS button
    //                 if(btn.TryGetComponent<Emotes>(out var emoteComponent)) 
    //                 {
    //                     string currentEmoteName = emoteComponent.emoteName;
    //                     Debug.Log($"Button has emote: {currentEmoteName}");

    //                     btnComponent.onClick.AddListener(() =>
    //                     {
    //                         Debug.Log($"Button clicked! Emote: {currentEmoteName}");
    //                         foreach (var npc in player.playerController.npcs)
    //                         {
    //                             if (npc.TryGetComponent<NPCManager>(out var npcManager))
    //                             {
    //                                 npcManager.EmoteNPC(currentEmoteName);
    //                             }
    //                         }
    //                     });
    //                 }
    //             }
    //             //else Debug.LogError("Couldn't fetch the Emotes component from the emote button");
    //         }
            
    //     }
    //     //else { Debug.LogError("No button found!");}
       
    //     yield break;
    // } 

}