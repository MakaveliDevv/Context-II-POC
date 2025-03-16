using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class Lion : NetworkBehaviour
{
    public int spheresRemaining, blocksRemaining, cylindersRemaining;
    public TextMeshProUGUI spheresRemainingText, blocksRemainingText, cylindersRemainingText;
    public string selectedObject = "";
    public LayerMask floorLayer;
    public Sprite blockSprite, cylinderSprite, sphereSprite;
    public Image selectedObjIndicator;
    public Camera lionCam;
    bool instantiatedPrefabs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        Debug.Log("Lion spawned on network");

        cylindersRemainingText.text = cylindersRemaining.ToString();
        spheresRemainingText.text = spheresRemaining.ToString();
        blocksRemainingText.text = blocksRemaining.ToString();

        lionCam.backgroundColor = Color.blue; // Change background color to blue
    }

    // Update is called once per frame
    void Update()
    {
        PlaceObject();
    }

    void PlaceObject()
    {
        if (selectedObject == "")
        {
            Debug.Log("No object selected");
            return;
        }

        if(selectedObject == "sphere" && spheresRemaining <= 0) return;
        if(selectedObject == "block" && blocksRemaining <= 0) return;
        if(selectedObject == "cylinder" && cylindersRemaining <= 0) return;

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = lionCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayer))
            {
                //Instantiate(selectedObject, hit.point + new Vector3(0, 1, 0), Quaternion.identity);
                ClientServerRefs.instance.localClient.SendObjectToServer(selectedObject, hit.point);

                if(selectedObject == "block")
                {
                    blocksRemaining--;
                    blocksRemainingText.text = blocksRemaining.ToString();
                }
                else if (selectedObject == "sphere")
                {
                    spheresRemaining--;
                    spheresRemainingText.text = spheresRemaining.ToString();
                }
                else if(selectedObject == "cylinder")
                {
                    cylindersRemaining--;
                    cylindersRemainingText.text = cylindersRemaining.ToString();
                }
            }
        }
    }

    public void ChangeSkyColor()
    {
        if(lionCam.backgroundColor == Color.blue)
        {
            lionCam.backgroundColor = Color.red;
        }

        else if(lionCam.backgroundColor == Color.red)
        {
            lionCam.backgroundColor = Color.blue;
        }
    }

    public void SelectBlock()
    {
        selectedObject = "block";
        selectedObjIndicator.sprite = blockSprite;
    }

    public void SelectSphere()
    {
        selectedObject = "sphere";
        selectedObjIndicator.sprite = sphereSprite;
    }

    public void SelectCylinder()
    {
        selectedObject = "cylinder";
        selectedObjIndicator.sprite = cylinderSprite;
    }
}
