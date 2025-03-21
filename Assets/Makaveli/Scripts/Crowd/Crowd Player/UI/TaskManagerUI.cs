using UnityEngine;
using UnityEngine.UI;
public class TaskManagerUI 
{
    private readonly Transform player;
    private GameObject tasksPanelUI;
    private Button openPanelBtn;
    private bool panelOpen;

    public TaskManagerUI(Transform player) 
    {
        this.player = player;
    }

    public void Start() 
    {
        tasksPanelUI = player.Find("Player Canvas").GetChild(4).gameObject;
        openPanelBtn = player.transform.Find("Player Canvas").GetChild(10).GetComponent<Button>();
        if(openPanelBtn != null) 
        {
            openPanelBtn.onClick.RemoveAllListeners();
            openPanelBtn.onClick.AddListener(TogglePanel);
        }
        else { Debug.LogError("The button couldn't be fetched"); return; }
    }

    public void CreateTaskCard(CrowdPlayerManager playerManager) 
    {
        if(playerManager.playerController.tasks.Count > 0) 
        {
            // Fetch the tasks the player has
            foreach (var task in playerManager.playerController.tasks)
            {
                // Create for each tasks a task card
                GameObject taskCard = Object.Instantiate(MGameManager.instance.taskCard);
                taskCard.transform.SetParent(tasksPanelUI.transform.GetChild(0)); 
                
                // Assign the the tasks to the task card
                if(taskCard.TryGetComponent<TaskCardUI>(out var taskCardUI))
                {
                    taskCardUI.task = task;
                }
                else { Debug.LogError("Couldn't fetch the 'TaskCardUI' component"); return; }
            }
        }
    }

    private void TogglePanel() 
    {
        if(openPanelBtn != null && player.TryGetComponent<CrowdPlayerManager>(out var playerManager)) 
        {
            if(playerManager.playerState != CrowdPlayerManager.PlayerState.CHOOSE_SHAPE ||
            playerManager.playerState != CrowdPlayerManager.PlayerState.CHOOSE_LOCATION ||
            playerManager.playerState != CrowdPlayerManager.PlayerState.REARRANGE_SHAPE) 
            {
                if(!panelOpen) 
                {
                    tasksPanelUI.SetActive(true);
                    panelOpen = true;
                }
                else 
                {
                    tasksPanelUI.SetActive(false);
                    panelOpen = false;
                }
            }
        } 
    }

    public void DisplayTaskBtn(CrowdPlayerManager playerManager) 
    {
        Debug.Log($"PlayerManager -> {playerManager.gameObject.name}");

        // If player has task
        if(playerManager.playerController.tasks.Count <= 0) 
        {
            openPanelBtn.gameObject.SetActive(false);   
        }
        else { openPanelBtn.gameObject.SetActive(true); }
    }
}
