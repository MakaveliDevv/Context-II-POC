using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class Lion : NetworkBehaviour
{
    [SerializeField] CustomNetworkBehaviour customNetworkBehaviour;
    public int spheresRemaining, blocksRemaining, cylindersRemaining;
    public TextMeshProUGUI spheresRemainingText, blocksRemainingText, cylindersRemainingText;
    public string selectedObject = "";
    public LayerMask floorLayer;
    public Sprite blockSprite, cylinderSprite, sphereSprite;
    public Image selectedObjIndicator;
    public Camera lionCam;
    public GameObject lionCanvas;
    [SerializeField] List<GameObject> objectsToPickup = new();
    [SerializeField] GameObject carryingObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if(customNetworkBehaviour == null) customNetworkBehaviour = GetComponent<CustomNetworkBehaviour>();
        StartCoroutine(InstantiateCorrectly());
        Debug.Log("Lion network spawn");
    }

    // Update is called once per frame
    void Update()
    {
        if(customNetworkBehaviour.CustomIsOwner())
        {
            PlaceObject();
            PickupObject();
        }
    }

    IEnumerator InstantiateCorrectly()
    {
        bool timeout = false;
        float elapsedTime = 0;

        while(!customNetworkBehaviour.CustomIsOwner())
        {
            yield return new WaitForFixedUpdate();
            elapsedTime += 0.02f;
            if(elapsedTime > 1) 
            {
                timeout = true;
                break;
            }
        }

        Debug.Log("Timeout: " + timeout + ", IsOwner " + customNetworkBehaviour.CustomIsOwner());

        if(!timeout)
        {
            if(!customNetworkBehaviour.CustomIsOwner()) 
            {
                lionCanvas.SetActive(false);
                lionCam.gameObject.SetActive(false);
            }
            else
            {
                cylindersRemainingText.text = cylindersRemaining.ToString();
                spheresRemainingText.text = spheresRemaining.ToString();
                blocksRemainingText.text = blocksRemaining.ToString();
                lionCam.transform.SetParent(null);
            }
        }
        else
        {
            lionCanvas.SetActive(false);
            lionCam.gameObject.SetActive(false);
        }
    }

    void PickupObject()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(carryingObject == null)
            {
                if(objectsToPickup.Count > 0)
                {
                    Transform objectToPickup = objectsToPickup[0].transform;
                    objectsToPickup.Remove(objectToPickup.gameObject);
                    objectToPickup.transform.position = transform.position;
                    carryingObject = objectToPickup.gameObject;

                    Transform firstChild = objectToPickup.transform.GetChild(0);
                    firstChild.gameObject.GetComponent<Collider>().enabled = false;
                    objectToPickup.gameObject.GetComponent<Rigidbody>().isKinematic = true;

                    //objectToPickup.transform.SetParent(transform);
                    RequestReparentServerRpc(objectToPickup.gameObject, gameObject, false);
                }
            }
            else
            {
                RequestReparentServerRpc(carryingObject.gameObject, gameObject, true);
                objectsToPickup.Insert(0, carryingObject.gameObject);
                Transform firstChild = carryingObject.transform.GetChild(0);
                firstChild.gameObject.GetComponent<Collider>().enabled = true;
                carryingObject.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                carryingObject = null;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestReparentServerRpc(NetworkObjectReference objectRef, NetworkObjectReference newParentRef, bool unparent)
    {
        if (objectRef.TryGet(out NetworkObject obj))
        {
            if (unparent)
            {
                obj.transform.SetParent(null); // Remove parent
            }
            else if (newParentRef.TryGet(out NetworkObject newParent))
            {
                obj.transform.SetParent(newParent.transform); // Set new parent
            }
        }
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

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Pickup"))
        {
            if(!objectsToPickup.Contains(collider.gameObject)) objectsToPickup.Insert(0, collider.gameObject);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(collider.gameObject.CompareTag("Pickup"))
        {
            if(objectsToPickup.Contains(collider.gameObject)) objectsToPickup.Remove(collider.gameObject);
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
