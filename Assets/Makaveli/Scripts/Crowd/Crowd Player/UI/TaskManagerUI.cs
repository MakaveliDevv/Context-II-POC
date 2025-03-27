using UnityEngine;
using UnityEngine.UI;
public class TaskManagerUI 
{
    private readonly CrowdPlayerManager player;
    private readonly Transform canvas;
    private Transform parentPanel;
    private Transform taskPanel;
    private Button openPanelBtn;
    private bool panelOpen;

    public TaskManagerUI(Transform canvas, CrowdPlayerManager player) { this.canvas = canvas; this.player = player; }

    public void Start() 
    {
        parentPanel = canvas.Find("TaskManagement");
        taskPanel = parentPanel.Find("TasksPanel");
        Debug.Log($"parentPanel = {parentPanel.name}, taskPanel = {taskPanel.name}");
        
        if(parentPanel.Find("TaskPanelButton").TryGetComponent<Button>(out var openPanelBtn)) 
        {
            Debug.Log($"TaskPanelButton: {openPanelBtn.name}");

            this.openPanelBtn = openPanelBtn;
            this.openPanelBtn.onClick.RemoveAllListeners();
            this.openPanelBtn.onClick.AddListener(TogglePanel);
        } else { Debug.LogError("Couldn't fetch the 'open task panel button'");  return; }
    }

    public void CreateTaskCard() 
    {
        if(player.playerController.tasks.Count > 0) 
        {
            // Fetch the tasks the player has
            foreach (var task in player.playerController.tasks)
            {
                // Create for each tasks a task card
                GameObject taskCard = Object.Instantiate(MGameManager.instance.taskCard);
                taskCard.transform.SetParent(taskPanel.transform.GetChild(0)); 
                
                // Assign the the tasks to the task card
                if(taskCard.TryGetComponent<TaskCardUI>(out var taskCardUI))
                {
                    taskCardUI.task = task;
                    taskCardUI.SetCardText(task.taskText);
                }
                else { Debug.LogError("Couldn't fetch the 'TaskCardUI' component"); return; }
            }
        }
    }

    private void TogglePanel() 
    {
        if(openPanelBtn != null) 
        {
            if(player.playerState != CrowdPlayerManager.PlayerState.CHOOSE_SHAPE ||
            player.playerState != CrowdPlayerManager.PlayerState.CHOOSE_LOCATION ||
            player.playerState != CrowdPlayerManager.PlayerState.CUSTOMIZE_SHAPE) 
            {
                if(!panelOpen) 
                {
                    taskPanel.gameObject.SetActive(true);
                    panelOpen = true;
                }
                else 
                {
                    taskPanel.gameObject.SetActive(false);
                    panelOpen = false;
                }
            }
        } 
    }

    public void DisplayTaskBtn(CrowdPlayerManager playerManager) 
    {
        // Debug.Log($"PlayerManager -> {playerManager.gameObject.name}");

        // If player has task
        if(playerManager.playerController.tasks.Count <= 0) 
        {
            openPanelBtn.gameObject.SetActive(false);   
        }
        else { openPanelBtn.gameObject.SetActive(true); }
    }
}
