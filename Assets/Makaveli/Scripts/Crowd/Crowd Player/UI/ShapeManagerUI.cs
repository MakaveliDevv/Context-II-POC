using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShapeManagerUI
{
    private GameObject[] panelObjects;
    public GameObject UIShapePanel;
    private readonly Transform player;
    private TextMeshProUGUI selectedShapeText;

    private Button previousButton;
    private Button nextButton;
    private Button selectButton;   
    public Button confirmButton; 
    private int currentIndex = 0;

    public bool shapeSelected;
    public string shapeName;

    public ShapeManagerUI
    (
        Transform player
    ) 
    {
        this.player = player;
    }

    public IEnumerator Start(CrowdPlayerManager playerManager)
    {
        InitializeUIShapePanel(playerManager);

        yield return null;

        PopulatePanelObjects();
        
        previousButton = UIShapePanel.transform.GetChild(3).transform.GetChild(0).GetComponent<Button>();
        nextButton = UIShapePanel.transform.GetChild(3).transform.GetChild(1).GetComponent<Button>();

        selectButton = UIShapePanel.transform.GetChild(1).GetComponent<Button>();
        selectedShapeText = selectButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        confirmButton = UIShapePanel.transform.GetChild(2).GetComponent<Button>();

        yield return null;

        // Set up button listeners
        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(NavigatePrevious);
        }
        
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NavigateNext);
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(SelectShape);
        }

        if(confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => ConfirmShape(playerManager) );
        }
        
        // Initialize panel by activating only the first object
        UpdatePanel();

        yield break;
    }

    private void InitializeUIShapePanel(CrowdPlayerManager playerManager) 
    {
        UIShapePanel = player.GetChild(4).GetChild(2).gameObject;

        if(!MGameManager.instance.playerShapeUI.ContainsKey(playerManager))
        {
            MGameManager.instance.playerShapeUI.Add(playerManager, UIShapePanel);
        }
        else 
        {
            Debug.LogWarning("Player already exist in the dictionary");
        }

        // Extra code for the entry in the inspector
        // DELETE LATER
        if(MGameManager.instance.playerShapeUI.ContainsKey(playerManager)) 
        {
            var entry = new DictionaryEntry<CrowdPlayerManager, GameObject> 
            {
                Key = playerManager,
                Value = UIShapePanel
            };

            if(!MGameManager.instance.PlayerShapeUI.Contains(entry)) 
            {
                MGameManager.instance.PlayerShapeUI.Add(entry);
            }
            else 
            {
                Debug.LogWarning("Entry already exist");
            }
        }
        else 
        {
            Debug.LogWarning("No player found in the dictionary");
        }
    }

    public void OpenShapePanel() 
    {
        UIShapePanel.SetActive(true);
    }

    public void CloseShapePanel() 
    {
        UIShapePanel.SetActive(false);
    }

    private void PopulatePanelObjects()
    {
        if (UIShapePanel == null)
        {
            Debug.LogError("UIShapePanel is not assigned!");
            return;
        }
        
        // Get the container that holds all the panel objects
        Transform container = UIShapePanel.transform.GetChild(0);
        int childCount = container.childCount;
        
        // Create array of the correct size
        panelObjects = new GameObject[childCount];
        
        // Populate array with all children
        for (int i = 0; i < childCount; i++)
        {
            panelObjects[i] = container.GetChild(i).gameObject;
        }
        
        // Debug.Log($"Found {childCount} panel objects to navigate");
    }

    private void UpdatePanel()
    {
        // Ensure we have objects to navigate through
        if (panelObjects == null || panelObjects.Length == 0)
            return;
        
        // Deactivate all objects
        for (int i = 0; i < panelObjects.Length; i++)
        {
            if (panelObjects[i] != null)
                panelObjects[i].SetActive(false);
        }
        
        // // Activate only the current object
        // if (panelObjects[currentIndex] != null)
        
        // Optional: Update button interactability based on current index
        if (previousButton != null)
        {
            previousButton.interactable = currentIndex > 0;
            panelObjects[currentIndex].SetActive(true);
        }
        
        if (nextButton != null)
        {
            nextButton.interactable = currentIndex < panelObjects.Length - 1;
            panelObjects[currentIndex].SetActive(true);
        }
    }

    private void NavigatePrevious()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdatePanel();
            selectedShapeText.text = "Select";
        }
    }
    
    private void NavigateNext()
    {
        if (currentIndex < panelObjects.Length - 1)
        {
            currentIndex++;
            UpdatePanel();
            selectedShapeText.text = "Select";
        }
    }

    private void SelectShape() 
    {
        // Make sure we have valid objects
        if (panelObjects == null || panelObjects.Length == 0 || currentIndex >= panelObjects.Length)
        {
            Debug.LogWarning("No valid panel objects to select from");
            return;
        }

        // Fetch the current shape card that's active
        GameObject currentShapeCard = panelObjects[currentIndex];
        
        // Fetch the UIShapeCard script on that GameObject
        UIShapeCard shapeCard = currentShapeCard.GetComponent<UIShapeCard>();
        
        if (shapeCard == null)
        {
            Debug.LogWarning("Current shape card does not have a UIShapeCard component");
            return;
        }
        
        // Get the shape name
        string shapeName = shapeCard.shape;
        
        // Update the UI to show the selected shape
        if (selectedShapeText != null)
        {
            selectedShapeText.text = $"{shapeName}";
            this.shapeName = shapeName;
        }

        shapeSelected = true;
    }

    private bool hasConfirmedShape = false;

    public void ConfirmShape(CrowdPlayerManager crowdPlayerManager) 
    {
        if (hasConfirmedShape) return;

        hasConfirmedShape = true;
        Debug.Log("Confirming shape, deactivating UI panel");

        // Close the UI shape panel
        // if(UIShapePanel.activeInHierarchy) UIShapePanel.SetActive(false); 
        // UIShapePanel.SetActive(false);
        // UIShapePanel.transform.localScale = Vector3.zero; // Force visibility off
        // crowdPlayerManager.inUIMode = false;

        Debug.Log("UI panel deactivated, checking if any part re-enables it");
    
        MGameManager.instance.gamePlayManagement = MGameManager.GamePlayManagement.SIGNAL;

    }
}
