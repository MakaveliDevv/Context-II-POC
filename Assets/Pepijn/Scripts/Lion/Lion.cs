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
    [SerializeField] public PlacableObjects carryingObject;
    Vector3 cameraOffset = new Vector3(0,11.7600002f,-4.61000013f);
    Dictionary<string, GameObject> objectPrefabsDict;
    [SerializeField] List<GameObject> objectsPrefabs;
    [SerializeField] List<string> objectNames;

    private bool objectPlaced;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if(customNetworkBehaviour == null) customNetworkBehaviour = GetComponent<CustomNetworkBehaviour>();
        StartCoroutine(InstantiateCorrectly());
        Debug.Log("Lion network spawn");

        objectPrefabsDict = new();

        for(int i = 0; i < objectsPrefabs.Count; i++)
        {
            objectPrefabsDict.Add(objectNames[i], objectsPrefabs[i]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(customNetworkBehaviour.CustomIsOwner())
        {
            MoveObjects();

            lionCam.transform.position = transform.position + cameraOffset;
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

    void MoveObjects()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            objectPlaced = false;

            if(carryingObject == null)
            {
                
            }
            else
            {
                DropObject();
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(carryingObject != null)
            {
                Destroy(carryingObject.gameObject);
                carryingObject = null;
            }
        }
    }

    void DropObject()
    {
        if(!carryingObject.placable) return;

        //RequestReparentServerRpc(carryingObject.gameObject, gameObject, true);
        //objectsToPickup.Insert(0, carryingObject.gameObject);
        SpawnObjectOnServerRpc(carryingObject.objName, carryingObject.transform.position, carryingObject.transform.rotation);
        ChangeObjectPlacedBoolOnServerRpc(true);
        Destroy(carryingObject.gameObject);
        carryingObject = null;
    }

    // [ServerRpc(RequireOwnership = false)]
    // void RequestReparentServerRpc(NetworkObjectReference objectRef, NetworkObjectReference newParentRef, bool unparent)
    // {
    //     if (objectRef.TryGet(out NetworkObject obj))
    //     {
    //         if (unparent)
    //         {
    //             obj.transform.SetParent(null); // Remove parent
    //         }
    //         else if (newParentRef.TryGet(out NetworkObject newParent))
    //         {
    //             obj.transform.SetParent(newParent.transform); // Set new parent
    //         }
    //     }
    // }

    [ServerRpc(RequireOwnership = false)]
    void ChangeObjectPlacedBoolOnServerRpc(bool _result)
    {
        objectPlaced = _result;
        ChangeObjectPlacedBoolOnClientRpc(_result);
    }
    [ClientRpc]
    void ChangeObjectPlacedBoolOnClientRpc(bool _result)
    {
        objectPlaced = _result;
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Pickup"))
        {
            if(!objectsToPickup.Contains(collider.gameObject)) objectsToPickup.Insert(0, collider.gameObject);
        }
    }

    void OnTriggerStay(Collider collider)
    {
        // If in range of location
        if(collider.CompareTag("TaskableLocation")) 
        {
            // If object placed
            if(objectPlaced) 
            {
                // Lion placed the object 
                MGameManager.instance.lionPlacedObject = true;
            }
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

    public void SpawnObject(string _objName)
    {
        if(carryingObject != null)
        {
            Destroy(carryingObject.gameObject);
            carryingObject = null;
        }
        GameObject _newObj = Instantiate(objectPrefabsDict[_objName], transform.position + (transform.forward * 4), Quaternion.identity, transform);
        carryingObject = _newObj.gameObject.GetComponent<PlacableObjects>();
        carryingObject.transform.position += carryingObject.spawnOffset;
        //carryingObject.transform.position = new Vector3(carryingObject.transform.position.x, (transform.position + carryingObject.spawnOffset).y, carryingObject.transform.position.z);
        carryingObject.CheckIfPlacable();
        //SpawnObjectOnServerRpc(_objName, transform.position + (transform.forward * 4));
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnObjectOnServerRpc(string _objName, Vector3 _position, Quaternion _rotation)
    {   
        Debug.Log($"Trying to spawn {_objName}");
        GameObject _newObj = Instantiate(objectPrefabsDict[_objName], _position, _rotation);
        NetworkObject _newObjInstance = _newObj.GetComponent<NetworkObject>();

        PlacableObjects placedObject = _newObjInstance.gameObject.GetComponent<PlacableObjects>();
        placedObject.PlaceObject();

        _newObjInstance.Spawn();

        NotifyClientOfSpawnClientRpc(_newObjInstance.NetworkObjectId);
    }

    [ClientRpc]
    void NotifyClientOfSpawnClientRpc(ulong spawnedObjectId)
    {
        // Find the spawned object by ID
        NetworkObject spawnedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[spawnedObjectId];
        PlacableObjects placedObject = spawnedObject.gameObject.GetComponent<PlacableObjects>();
        placedObject.PlaceObject();
        // Add it to the client's list
        //carryingObject = _objectToPickup.gameObject.GetComponent<PlacableObjects>();
    }
}