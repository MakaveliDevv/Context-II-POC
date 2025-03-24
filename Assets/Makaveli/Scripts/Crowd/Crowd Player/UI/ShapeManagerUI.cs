using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShapeManagerUI
{
    public bool shapeConfirmed = false;

    private GameObject UIShapePanel;            // Parent panel
    private GameObject[] panelObjects;          // Reference to the images
    private readonly Transform player;
    private TextMeshProUGUI selectedShapeText;  // Reference to the selected shape text on the button
    private Transform rearrangePanel;           // Reference to the gameobject UI of rearraning the shape

    // Buttons
    private Button openPanelBtn;
    private Button closePanelBtn;
    private Button previousBtn;          
    private Button nextBtn;

    private int currentIndex = 0;
    public string shapeName;
    private bool shapeSelected;

    public ShapeManagerUI
    (
        Transform player
    ) 
    {
        this.player = player;
    }

    public IEnumerator Start(CrowdPlayerManager playerManager)
    {
        InitializeShapePanelUI(playerManager);

        rearrangePanel = UIShapePanel.transform.parent.GetChild(3);
        
        yield return null;

        PopulatePanelObjects();
        
        InitializeButtons(playerManager);

        yield return null;
   
        // Initialize panel by activating only the first object
        UpdatePanel();

        yield break;
    }

    private void InitializeShapePanelUI(CrowdPlayerManager playerManager) 
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

    private void InitializeButtons(CrowdPlayerManager playerManager) 
    {
        // open shape panel button
        if(UIShapePanel.transform.parent.GetChild(5).TryGetComponent<Button>(out var openPanelBtn)) 
        {
            // Debug.Log($"Button open panel button: {openPanelBtn.name}");

            if(openPanelBtn != null) 
            {
                openPanelBtn.onClick.RemoveAllListeners();
                openPanelBtn.onClick.AddListener( () => 
                {
                    // Open the panel
                    OpenShapePanel(playerManager);

                    // Update the button visibility
                    // UpdatePanelButtons(true);
                });
            }

        } else { Debug.LogError("Couldn't fetch the 'open shape panel button'");  return; }

        // close shape panel button
        if(UIShapePanel.transform.parent.GetChild(5).TryGetComponent<Button>(out var closePanelBtn))
        {
            if(closePanelBtn != null) 
            {
                closePanelBtn.onClick.RemoveAllListeners();
                closePanelBtn.onClick.AddListener(() => 
                {
                    CloseShapePanel(playerManager);

                    // Update the button visibility
                    // UpdatePanelButtons(false);
                });
            }
            // Debug.Log($"Button closepanel button: {closePanelButton.name}");
        } else { Debug.LogError("Couldn't fetch the 'close shape panel button'"); return; }

        // next nav button
        if(UIShapePanel.transform.GetChild(3).transform.GetChild(1).TryGetComponent<Button>(out var nextBtn))
        {
            this.nextBtn = nextBtn;
            if (this.nextBtn != null)
            {
                this.nextBtn.onClick.RemoveAllListeners();
                this.nextBtn.onClick.AddListener(NavigateNext);
            }
            // Debug.Log($"Button next button: {nextButton.name}");
        } else { Debug.LogError("Couldn't fetch the 'next nav button' "); return; }

        // previous nav button
        if(UIShapePanel.transform.GetChild(3).transform.GetChild(0).TryGetComponent<Button>(out var previousBtn))
        {
            this.previousBtn = previousBtn;
            if (this.previousBtn != null)
            {
                this.previousBtn.onClick.RemoveAllListeners();
                this.previousBtn.onClick.AddListener(NavigatePrevious);
            }

            // Debug.Log($"Button prev button: {previousButton.name}");
        } else { Debug.LogError("Couldn't fetch the 'previous nav button' "); return; }

        // select shape button
        if(UIShapePanel.transform.GetChild(1).TryGetComponent<Button>(out var selectBtn))
        {
            if (selectBtn != null)
            {
                selectBtn.onClick.RemoveAllListeners();
                selectBtn.onClick.AddListener(SelectShape);
            }
            // Debug.Log($"Button selectButton: {selectButton.name}");
        } 
        else { Debug.LogError("Couldn't fetch the 'select shape button' "); return; }

        // selected shape text
        if(selectBtn.gameObject.transform.GetChild(0).TryGetComponent<TextMeshProUGUI>(out var selectedShapeText))
        {
            this.selectedShapeText = selectedShapeText;
            // Debug.Log($"text: {selectedShapeText.name}");
        } else { Debug.LogError("Couldn't fetch the 'selected shape text' "); return; }

        // confirm shape button
        if(UIShapePanel.transform.GetChild(2).TryGetComponent<Button>(out var confirmShapeBtn))
        {
            if(confirmShapeBtn != null)
            {
                confirmShapeBtn.onClick.RemoveAllListeners();
                confirmShapeBtn.onClick.AddListener(() => playerManager.StartCoroutine(ConfirmShape(playerManager)) );
            }
            // Debug.Log($"Button confirmButton: {confirmButton.name}");
        } else { Debug.LogError("Couldn't fetch the 'confirm shape button' "); return; }

        // confirm rearrange shape button
        if(rearrangePanel.GetChild(0).GetChild(1).TryGetComponent<Button>(out var confirmRearrangeBtn)) 
        {
            if(confirmRearrangeBtn != null) 
            {
                confirmRearrangeBtn.onClick.RemoveAllListeners();
                confirmRearrangeBtn.onClick.AddListener(() => ConfirmRearrangedShape(playerManager));
            }
        } else { Debug.LogError("Couldn't fetch the 'rearrange shape button' "); return; }

        // open rearrange shape button
        if(rearrangePanel.GetChild(1).TryGetComponent<Button>(out var openRearrangePanelBtn))
        {
            if(openRearrangePanelBtn != null) 
            {
                openRearrangePanelBtn.onClick.RemoveAllListeners();
                openRearrangePanelBtn.onClick.AddListener(() => 
                {
                    OpenRearrangePanel();
                    playerManager.playerState = CrowdPlayerManager.PlayerState.REARRANGE_SHAPE;
                });
            }
        } else { Debug.LogError("Couldn't fetch the 'rearrange shape button' "); return; }
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

    public void UpdatePanelButtons(bool display) 
    {
        if(display) 
        {   
            openPanelBtn.gameObject.SetActive(false);
            closePanelBtn.gameObject.SetActive(true);
        }
        else 
        {
            openPanelBtn.gameObject.SetActive(true);
            closePanelBtn.gameObject.SetActive(false);
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
        if (previousBtn != null)
        {
            previousBtn.interactable = currentIndex > 0;
            panelObjects[currentIndex].SetActive(true);
        }
        
        if (nextBtn != null)
        {
            nextBtn.interactable = currentIndex < panelObjects.Length - 1;
            panelObjects[currentIndex].SetActive(true);
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
        ShapeCardUI shapeCard = currentShapeCard.GetComponent<ShapeCardUI>();
        
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
            // Debug.Log($"Shape selected: {this.shapeName}");
        }

        shapeSelected = true;
    }

    private IEnumerator ConfirmShape(CrowdPlayerManager playerManager) 
    {
        if (shapeConfirmed) yield break;

        shapeConfirmed = true;
        // Debug.Log("Confirming shape, deactivating UI panel");

        CloseShapePanel(playerManager);
        
        playerManager.playerState = CrowdPlayerManager.PlayerState.REARRANGE_SHAPE;

        yield return new WaitForSeconds(2f);

        OpenRearrangePanel();

        //MGameManager.instance.gamePlayManagement = MGameManager.GamePlayManagement.SOLVING_TASK;

        yield break;
    }

    private void ConfirmRearrangedShape(CrowdPlayerManager playerManager) 
    {
        CloseRearrangePanel();
        playerManager.ConfirmShapeServerRpc();
        playerManager.playerState = CrowdPlayerManager.PlayerState.SIGNAL;
    }

    private void OpenRearrangePanel() 
    {
        // Hide button to open box
        rearrangePanel.GetChild(1).gameObject.SetActive(false);

        // Show box
        rearrangePanel.GetChild(0).gameObject.SetActive(true);
    }

    private void CloseRearrangePanel() 
    {
        // Hide box
        rearrangePanel.GetChild(0).gameObject.SetActive(false);

        // Show button to open box
        rearrangePanel.GetChild(1).gameObject.SetActive(true);
    }
}
