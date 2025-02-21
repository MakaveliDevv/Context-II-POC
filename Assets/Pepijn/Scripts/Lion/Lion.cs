using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class Lion : MonoBehaviour
{
    public int spheresRemaining, blocksRemaining, cylindersRemaining;
    public TextMeshProUGUI spheresRemainingText, blocksRemainingText, cylindersRemainingText;
    public GameObject sphere, block, cylinder;
    public GameObject selectedObject;
    public LayerMask floorLayer;
    public Sprite blockSprite, cylinderSprite, sphereSprite;
    public Image selectedObjIndicator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cylindersRemainingText.text = cylindersRemaining.ToString();
        spheresRemainingText.text = spheresRemaining.ToString();
        blocksRemainingText.text = blocksRemaining.ToString();

        Camera.main.backgroundColor = Color.blue; // Change background color to blue
    }

    // Update is called once per frame
    void Update()
    {
        PlaceObject();
    }

    void PlaceObject()
    {
        if (selectedObject == null) return;

        if(selectedObject == sphere && spheresRemaining <= 0) return;
        if(selectedObject == block && blocksRemaining <= 0) return;
        if(selectedObject == cylinder && cylindersRemaining <= 0) return;

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayer))
            {
                Instantiate(selectedObject, hit.point + new Vector3(0, 1, 0), Quaternion.identity);

                if(selectedObject == block)
                {
                    blocksRemaining--;
                    blocksRemainingText.text = blocksRemaining.ToString();
                }
                else if (selectedObject == sphere)
                {
                    spheresRemaining--;
                    spheresRemainingText.text = spheresRemaining.ToString();
                }
                else if(selectedObject == cylinder)
                {
                    cylindersRemaining--;
                    cylindersRemainingText.text = cylindersRemaining.ToString();
                }
            }
        }
    }

    public void ChangeSkyColor()
    {
        if(Camera.main.backgroundColor == Color.blue)
        {
            Camera.main.backgroundColor = Color.red;
        }

        else if(Camera.main.backgroundColor == Color.red)
        {
            Camera.main.backgroundColor = Color.blue;
        }
    }

    public void SelectBlock()
    {
        selectedObject = block;
        selectedObjIndicator.sprite = blockSprite;
    }

    public void SelectSphere()
    {
        selectedObject = sphere;
        selectedObjIndicator.sprite = sphereSprite;
    }

    public void SelectCylinder()
    {
        selectedObject = cylinder;
        selectedObjIndicator.sprite = cylinderSprite;
    }
}
