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
    private Transform rearrange;

    public Button openPanelButton;
    public Button closePanelButton;
    private Button previousButton;
    private Button nextButton;
    private Button selectButton;   
    public Button confirmButton; 
    private Button confirmBtn2;

    private int currentIndex = 0;

    public string shapeName;
    public bool shapeSelected;
    public bool shapeConfirmed = false;

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

        rearrange = UIShapePanel.transform.parent.GetChild(3);
        
        yield return null;

        PopulatePanelObjects();
        
        InitializeButtons();

        yield return null;
        
        // Set up button listeners
        playerManager.StartCoroutine(AddListeners(playerManager));
   
        // Initialize panel by activating only the first object
        UpdatePanel();

        yield break;
    }

    private IEnumerator AddListeners(CrowdPlayerManager playerManager) 
    {
        if(this.openPanelButton != null) 
        {
            openPanelButton.onClick.RemoveAllListeners();
            openPanelButton.onClick.AddListener( () => 
            {
                // Open the panel
                OpenShapePanel(playerManager);

                // Update the button visibility
                UpdatePanelButtons(true);

            });
        }

        yield return null;

        if(this.closePanelButton != null) 
        {
            closePanelButton.onClick.RemoveAllListeners();
            closePanelButton.onClick.AddListener(() => 
            {
                CloseShapePanel(playerManager);

                // Update the button visibility
                UpdatePanelButtons(false);
            });
        }
        
        yield return null;

        if (this.previousButton != null)
        {
            this.previousButton.onClick.RemoveAllListeners();
            this.previousButton.onClick.AddListener(NavigatePrevious);
        }

        yield return null;
        
        if (this.nextButton != null)
        {
            this.nextButton.onClick.RemoveAllListeners();
            this.nextButton.onClick.AddListener(NavigateNext);
        }

        yield return null;

        if (this.selectButton != null)
        {
            this.selectButton.onClick.RemoveAllListeners();
            this.selectButton.onClick.AddListener(SelectShape);
        }
        
        yield return null;

        if(this.confirmButton != null)
        {
            this.confirmButton.onClick.RemoveAllListeners();
            this.confirmButton.onClick.AddListener(() => ConfirmShape(playerManager) );
        }

        yield return null;

        if(this.confirmBtn2 != null) 
        {
            this.confirmBtn2.onClick.RemoveAllListeners();
            this.confirmBtn2.onClick.AddListener(() => ConfirmRearrangedShape(playerManager));
        }

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

    private void InitializeButtons() 
    {
        // open shape panel button
        if(UIShapePanel.transform.parent.GetChild(4).TryGetComponent<Button>(out var openPanelButton)) 
        {
            this.openPanelButton = openPanelButton;
            Debug.Log($"Button open panel button: {openPanelButton.name}");
        }
        else { Debug.LogError("Couldn't fetch the 'open shape panel button'");  return; }

        // close shape panel button
        if(UIShapePanel.transform.parent.GetChild(5).TryGetComponent<Button>(out var closePanelButton))
        {
            this.closePanelButton = closePanelButton;
            Debug.Log($"Button closepanel button: {closePanelButton.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'close shape panel button'"); return; }

        // next nav button
        if(UIShapePanel.transform.GetChild(3).transform.GetChild(1).TryGetComponent<Button>(out var nextButton))
        {
            this.nextButton = nextButton;
            Debug.Log($"Button next button: {nextButton.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'next nav button' "); return; }

        // previous nav button
        if(UIShapePanel.transform.GetChild(3).transform.GetChild(0).TryGetComponent<Button>(out var previousButton))
        {
            this.previousButton = previousButton;
            Debug.Log($"Button prev button: {previousButton.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'previous nav button' "); return; }

        // select shape button
        if(UIShapePanel.transform.GetChild(1).TryGetComponent<Button>(out var selectButton))
        {
            this.selectButton = selectButton;
            Debug.Log($"Button selectButton: {selectButton.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'select shape button' "); return; }

        // selected shape text
        if(selectButton.gameObject.transform.GetChild(0).TryGetComponent<TextMeshProUGUI>(out var selectedShapeText))
        {
            this.selectedShapeText = selectedShapeText;
            Debug.Log($"text: {selectedShapeText.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'selected shape text' "); return; }

        // confirm shape button
        if(UIShapePanel.transform.GetChild(2).TryGetComponent<Button>(out var confirmButton))
        {
            this.confirmButton = confirmButton;
            Debug.Log($"Button confirmButton: {confirmButton.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'confirm shape button' "); return; }

        // Rearrange shape button
        if(UIShapePanel.transform.parent.GetChild(3).GetChild(1).TryGetComponent<Button>(out var confrimBtn2))
        {
            this.confirmBtn2 = confrimBtn2;
            Debug.Log($"Button confrimBtn2: {confrimBtn2.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'rearrange shape button' "); return; }
    }

    public void UpdatePanelButtons(bool display) 
    {
        if(display) 
        {   
            openPanelButton.gameObject.SetActive(false);
            closePanelButton.gameObject.SetActive(true);
        }
        else 
        {
            openPanelButton.gameObject.SetActive(true);
            closePanelButton.gameObject.SetActive(false);
        }
    }

    public void OpenShapePanel(CrowdPlayerManager playerManager) 
    {
        UIShapePanel.SetActive(true);
        playerManager.UIMode(true);
    }

    public void CloseShapePanel(CrowdPlayerManager playerManager) 
    {
        UIShapePanel.SetActive(false);
        playerManager.UIMode(false);
    }

    private void PopulatePanelObjects()
    {
        if (UIShapePanel == null)
        {
            Debug.LogError("UIShapePanel is not assigned!");
            return;
        }
        
        // Get the container that holds all the panel objects
        Transform imagePanel = UIShapePanel.transform.GetChild(0);
        int childCount = imagePanel.childCount;
        
        // Create array of the correct size
        panelObjects = new GameObject[childCount];
        
        // Populate array with all children
        for (int i = 0; i < childCount; i++)
        {
            panelObjects[i] = imagePanel.GetChild(i).gameObject;
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
        
        // Update button interactability based on current index
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
        Debug.Log("Executed");
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
            Debug.Log($"Shape selected: {this.shapeName}");
        }

        shapeSelected = true;
    }

    public void ConfirmShape(CrowdPlayerManager playerManager) 
    {
        if (shapeConfirmed) return;

        shapeConfirmed = true;
        Debug.Log("Confirming shape, deactivating UI panel");

        CloseShapePanel(playerManager);
        
        playerManager.playerState = CrowdPlayerManager.PlayerState.REARRANGE_SHAPE;

        rearrange.gameObject.SetActive(true);

        // Display emote of npc

        // Close the UI shape panel
        // if(UIShapePanel.activeInHierarchy) UIShapePanel.SetActive(false); 
        // UIShapePanel.SetActive(false);
        // UIShapePanel.transform.localScale = Vector3.zero; // Force visibility off
        // crowdPlayerManager.inUIMode = false;

        Debug.Log("UI panel deactivated, checking if any part re-enables it");
    
        // MGameManager.instance.gamePlayManagement = MGameManager.GamePlayManagement.SIGNAL;
    }

    public void ConfirmRearrangedShape(CrowdPlayerManager playerManager) 
    {
        Debug.Log("method executed...");
        rearrange.gameObject.SetActive(false);
        playerManager.playerState = CrowdPlayerManager.PlayerState.SIGNAL;
    }
}
