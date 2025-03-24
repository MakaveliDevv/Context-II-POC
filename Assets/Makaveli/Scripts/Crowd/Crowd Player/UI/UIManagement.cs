using System.Collections.Generic;
using UnityEngine;

public class UIManagement 
{
    public ShapeManagerUI shapeManagerUI;
    private readonly LocationManagerUI locationManagerUI;
    private readonly TaskManagerUI taskManagerUI;
    public bool taskCreated;

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