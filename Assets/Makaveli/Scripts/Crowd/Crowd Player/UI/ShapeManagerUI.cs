using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShapeManagerUI
{
    private readonly CrowdPlayerManager player;

    public bool shapeConfirmed = false;
    private readonly Transform canvas;
    private Transform parentPanel;
    private Transform shapePanel;
    private GameObject[] panelObjects;          // Reference to the image buttons
    private TextMeshProUGUI selectedShapeText;  
    private Transform customizePanel;           

    // Buttons
    private Button openShapePanelBtn;
    private Button openCustomizePanelBtn;
    private Button previousBtn;          
    private Button nextBtn;

    private int currentIndex = 0;
    public string shapeName;
    public bool shapeSelected;
    private bool goToCustomize;

    public ShapeManagerUI(Transform canvas, CrowdPlayerManager player) { this.canvas = canvas; this.player = player; }

    public IEnumerator Start()
    {
        InitializeShapePanelUI();
        
        yield return null;

        PopulatePanelObjects();
        
        InitializeButtons();

        yield return null;
   
        // Initialize panel by activating only the first object
        UpdatePanel();

        yield break;
    }

    private void InitializeShapePanelUI() 
    {
        parentPanel = canvas.Find("ShapeManagement");
        shapePanel = parentPanel.Find("ShapePanel");
        customizePanel = parentPanel.Find("CustomizePanel");
        
        Debug.Log($"parentPanel = {parentPanel.name}, shapePanel = {shapePanel.name}, customizePanel = {customizePanel.name}");

        if(!MGameManager.instance.playerShapeUI.ContainsKey(player))
        {
            MGameManager.instance.playerShapeUI.Add(player, shapePanel.gameObject);
        }
        else 
        {
            Debug.LogWarning("Player already exist in the dictionary");
        }

        // Extra code for the entry in the inspector
        // DELETE LATER
        if(MGameManager.instance.playerShapeUI.ContainsKey(player)) 
        {
            var entry = new DictionaryEntry<CrowdPlayerManager, GameObject> 
            {
                Key = player,
                Value = shapePanel.gameObject
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
        if(parentPanel.Find("OpenShapePanelBtn").TryGetComponent<Button>(out var openShapePanelBtn)) 
        {
            Debug.Log($"Button open panel button: {openShapePanelBtn.name}");

            this.openShapePanelBtn = openShapePanelBtn;
            this.openShapePanelBtn.onClick.RemoveAllListeners();
            this.openShapePanelBtn.onClick.AddListener( () => 
            {
                // Open the panel
                OpenShapePanel();

                // Update the button visibility
                // UpdatePanelButtons(true);
            });
        } else { Debug.LogError("Couldn't fetch the 'open shape panel button'");  return; }

        // close shape panel button
        if(shapePanel.Find("CloseShapePanelButton").TryGetComponent<Button>(out var closeShapePanelButton))
        {
            Debug.Log($"CloseShapePanelButton: {closeShapePanelButton.name}");

            closeShapePanelButton.onClick.RemoveAllListeners();
            closeShapePanelButton.onClick.AddListener(() => 
            {
                CloseShapePanel();
            });         
        } else { Debug.LogError("Couldn't fetch the 'close shape panel button'"); return; }

        // next nav button
        if(shapePanel.Find("Panel Navigation").Find("Arrow-down").TryGetComponent<Button>(out var arrow_down))
        {
            Debug.Log($"Arrow-down: {arrow_down.name}");
            
            nextBtn = arrow_down;
            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(NavigateNext);  
        } else { Debug.LogError("Couldn't fetch the 'next nav button' "); return; }

        // previous nav button
        if(shapePanel.Find("Panel Navigation").Find("Arrow-up").TryGetComponent<Button>(out var arrow_up))
        {
            Debug.Log($"Arrow-up: {arrow_up.name}");

            previousBtn = arrow_up;
            previousBtn.onClick.RemoveAllListeners();
            previousBtn.onClick.AddListener(NavigatePrevious);
        } else { Debug.LogError("Couldn't fetch the 'previous nav button' "); return; }

        // select shape button
        if(shapePanel.Find("Select Button").TryGetComponent<Button>(out var selectBtn))
        {
            Debug.Log($"Button selectButton: {selectBtn.name}");

            selectBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.AddListener(SelectShape);    
        } else { Debug.LogError("Couldn't fetch the 'select shape button' "); return; }

        // selected shape text
        if(selectBtn.gameObject.transform.GetChild(0).TryGetComponent<TextMeshProUGUI>(out var selectedShapeText))
        {
            Debug.Log($"text: {selectedShapeText.name}");
            this.selectedShapeText = selectedShapeText;
        } else { Debug.LogError("Couldn't fetch the 'selected shape text' "); return; }

        // confirm shape button
        // if(shapePanel.Find("Confirm Button").TryGetComponent<Button>(out var confirmShapeBtn))
        // {
        //     Debug.Log($"Button confirmButton: {confirmShapeBtn.name}");

        //     if(confirmShapeBtn != null)
        //     {
        //         confirmShapeBtn.onClick.RemoveAllListeners();
        //         confirmShapeBtn.onClick.AddListener(() => player.StartCoroutine(ConfirmShape()) );
        //     }
        // } else { Debug.LogError("Couldn't fetch the 'confirm shape button' "); return; }

        // confirm customize button
        if(customizePanel.Find("Customize").Find("ConfirmButton").TryGetComponent<Button>(out var confirmCustomizeBtn)) 
        {
            Debug.Log($"Customize confirm btn: {confirmCustomizeBtn.name}");

            confirmCustomizeBtn.onClick.RemoveAllListeners();
            confirmCustomizeBtn.onClick.AddListener(() => ConfirmCustomizedShape());
            
        } else { Debug.LogError("Couldn't fetch the 'rearrange shape button' "); return; }

        // open customize panel button
        if(parentPanel.Find("OpenCustomizePanel").TryGetComponent<Button>(out var openCustomizePanelBtn))
        {
            Debug.Log($"OpenCustomizePanel btn: {openCustomizePanelBtn.name}");

            this.openCustomizePanelBtn  = openCustomizePanelBtn;
            this.openCustomizePanelBtn.onClick.RemoveAllListeners();
            this.openCustomizePanelBtn.onClick.AddListener(() => 
            {
                OpenCustomizePanel();
            });
            
        } else { Debug.LogError("Couldn't fetch the 'rearrange shape button' "); return; }
    }

    private void PopulatePanelObjects()
    {
        if (shapePanel == null)
        {
            Debug.LogError("UIShapePanel is not assigned!");
            return;
        }
        
        // Get the container that holds all the panel objects
        Transform imagePanel = shapePanel.transform.GetChild(0);
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

    public void OpenShapePanel() 
    {
        player.choosingShape = true;
        shapePanel.gameObject.SetActive(true);
        openShapePanelBtn.gameObject.SetActive(false);
        player.UIMode(true);
        player.playerState = CrowdPlayerManager.PlayerState.CHOOSE_SHAPE;
    }

    public void CloseShapePanel() 
    {
        player.choosingShape = false;
        shapePanel.gameObject.SetActive(false);
        openShapePanelBtn.gameObject.SetActive(true);
        player.UIMode(false);

        // if(!goToCustomize) 
        // {
            player.playerState = CrowdPlayerManager.PlayerState.ROAM_AROUND;
        // } 
    }

    public void Update() 
    {
        if(player.playerState == CrowdPlayerManager.PlayerState.CUSTOMIZE_SHAPE || player.playerState == CrowdPlayerManager.PlayerState.CHOOSE_LOCATION) 
        {
            UpdatePanelButton(false);
        }
        else { UpdatePanelButton(true); }
    }

    public void UpdatePanelButton(bool display) 
    {
        if(openShapePanelBtn != null) 
        {
            if(display) 
            {   
                openShapePanelBtn.gameObject.SetActive(true);
            }
            else 
            {
                openShapePanelBtn.gameObject.SetActive(false);
            }
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
        
        if (!currentShapeCard.TryGetComponent<ShapeCardUI>(out var shapeCard))
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

        if(this.shapeName.Contains("Customize")) 
        {
            Debug.Log($"Changing state to customize State {player.playerState}");
            goToCustomize = true;
            player.choosingShape = false;
            shapePanel.gameObject.SetActive(false);
            OpenCustomizePanel();
            player.playerState = CrowdPlayerManager.PlayerState.CUSTOMIZE_SHAPE;
        }
    }

    // private IEnumerator ConfirmShape() 
    // {
    //     if (shapeConfirmed) yield break;

    //     shapeConfirmed = true;
        
    //     // Debug.Log("Confirming shape, deactivating UI panel");

    //     CloseShapePanel();
        
    //     player.playerState = CrowdPlayerManager.PlayerState.CUSTOMIZE_SHAPE;

    //     yield return new WaitForSeconds(2f);

    //     OpenCustomizePanel();

    //     //MGameManager.instance.gamePlayManagement = MGameManager.GamePlayManagement.SOLVING_TASK;

    //     yield break;
    // }

    private void ConfirmCustomizedShape() 
    {
        CloseCustomizePanel();
        player.ConfirmShapeServerRpc();
        player.playerState = CrowdPlayerManager.PlayerState.ROAM_AROUND;
    }

    private void OpenCustomizePanel() 
    {
        player.customizingShape = true;
        customizePanel.gameObject.SetActive(true);
        openCustomizePanelBtn.gameObject.SetActive(false);

        player.playerState = CrowdPlayerManager.PlayerState.CUSTOMIZE_SHAPE;
    }

    private void CloseCustomizePanel() 
    {
        player.customizingShape = false;
        customizePanel.gameObject.SetActive(false);
        openCustomizePanelBtn.gameObject.SetActive(true);
        player.RepositionCamera();
    }
}
