using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
public class Lion : NetworkBehaviour
{
    [SerializeField] public CustomNetworkBehaviour customNetworkBehaviour;
    public int spheresRemaining, blocksRemaining, cylindersRemaining;
    public TextMeshProUGUI spheresRemainingText, blocksRemainingText, cylindersRemainingText;
    public string selectedObject = "";
    public LayerMask floorLayer;
    public Sprite blockSprite, cylinderSprite, sphereSprite;
    public Image selectedObjIndicator;
    public Camera lionCam;
    public GameObject lionCanvas;
    [SerializeField] List<GameObject> objectsToPickup = new();
    public PlacableObjects carryingObject;
    Vector3 cameraOffset = new(0, 20.7600002f,-4.61000013f);
    Dictionary<string, GameObject> objectPrefabsDict;
    [SerializeField] List<GameObject> objectsPrefabs;
    [SerializeField] List<string> objectNames;

    public Task lastObjectTask = null;
    public Transform taskLocation;
    public TaskLocation taskLocationRef;
    public bool objectPlaced;
    public bool encounter;
    public bool objectDropped;
    [SerializeField] Animator animator;
    [SerializeField] NetworkObject serverCarryingObject;
    [SerializeField] Transform holdingTransform;
    [SerializeField] public TextMeshProUGUI wrongObjectText;

    public bool makaveli;

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

        MGameManager.instance.lion = this;
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
            if(elapsedTime > 1f) 
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
        if(Input.GetKeyDown(KeyCode.E) && !objectDropped)
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

                DestroyCarryingObjectServerRpc();
                ChangeAnimatorCarryWeightServerRpc(0);
                serverCarryingObject = null;
            }
        }
    }

    void DropObject()
    {
        if(!carryingObject.placable) return;

        //RequestReparentServerRpc(carryingObject.gameObject, gameObject, true);
        //objectsToPickup.Insert(0, carryingObject.gameObject);
        bool solvingTask = false;
        if(MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.SOLVING_TASK) solvingTask = true;
        SpawnObjectOnServerRpc(carryingObject.objName, carryingObject.transform.position, carryingObject.transform.rotation, solvingTask);
        ChangeObjectPlacedBoolOnServerRpc(true);

        lastObjectTask = carryingObject.task;
        Destroy(carryingObject.gameObject);
        carryingObject = null;
        DestroyCarryingObjectServerRpc();
        ChangeAnimatorCarryWeightServerRpc(0);
        serverCarryingObject = null;
        objectDropped = true;
        StartCoroutine(ReseetBool());
    }

    private IEnumerator ReseetBool() 
    {
        yield return new WaitForSeconds(2f);
        objectDropped = false;
    }

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

        if(collider.CompareTag("TaskableLocation")) 
        {
            Debug.Log("Lion collided with task location");
            if(MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.SOLVING_TASK) 
            {
                taskLocation = collider.transform;
                encounter = true;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(collider.gameObject.CompareTag("Pickup"))
        {
            if(objectsToPickup.Contains(collider.gameObject)) objectsToPickup.Remove(collider.gameObject);
        }

        if(collider.CompareTag("TaskableLocation"))
        {
            encounter = false;
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
        if(serverCarryingObject != null)
        {
            DestroyCarryingObjectServerRpc();
            ChangeAnimatorCarryWeightServerRpc(0);
            serverCarryingObject = null;
        }
        GameObject _newObj = Instantiate(objectPrefabsDict[_objName], transform.position + (transform.forward * 4), Quaternion.identity, transform);
        carryingObject = _newObj.GetComponent<PlacableObjects>();
        carryingObject.transform.position += carryingObject.spawnOffset;

        //carryingObject.transform.position = new Vector3(carryingObject.transform.position.x, (transform.position + carryingObject.spawnOffset).y, carryingObject.transform.position.z);
        carryingObject.isIndicator = true;
        carryingObject.CheckIfPlacable();

        CarryObjectOnServerRpc(_objName);
        //SpawnObjectOnServerRpc(_objName, transform.position + (transform.forward * 4));
    }

    [ServerRpc(RequireOwnership = false)]
    void CarryObjectOnServerRpc(string _objName)
    {   
        GameObject _newObj = Instantiate(objectPrefabsDict[_objName], transform.position + (transform.forward * 2), Quaternion.identity); //should be 2 instead of 4
        NetworkObject _newObjInstance = _newObj.GetComponent<NetworkObject>();
        _newObjInstance.Spawn();
        _newObjInstance.transform.SetParent(transform);

        PlacableObjects placedObject = _newObjInstance.gameObject.GetComponent<PlacableObjects>();

        serverCarryingObject = _newObjInstance;

        float weight = placedObject.weight;
        animator.SetLayerWeight(1, weight);
        ChangeAnimatorCarryWeightClientRpc(weight);


        CarryObjectOnClientRpc(_newObjInstance.NetworkObjectId);
    }
    [ClientRpc]
    void CarryObjectOnClientRpc(ulong spawnedObjectId)
    {
        NetworkObject spawnedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[spawnedObjectId];
        serverCarryingObject = spawnedObject;
        PlacableObjects placedObject = spawnedObject.gameObject.GetComponent<PlacableObjects>();
        spawnedObject.gameObject.GetComponent<Collider>().enabled = false;

        placedObject.transform.localScale = placedObject.transform.localScale * placedObject.holdingScale.x;
        Debug.Log($"Local Position before change: {placedObject.transform.localPosition} , applying {placedObject.holdingOffset}");
        placedObject.transform.localPosition += placedObject.holdingOffset + new Vector3(0, 3, 0);
        Debug.Log($"Local Position after change: {placedObject.transform.localPosition}");
        //placedObject.transform.position += new Vector3(0, 3f, 0) + placedObject.holdingOffset;
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyCarryingObjectServerRpc()
    {
        Destroy(serverCarryingObject);
    }

    [ServerRpc]
    void ChangeAnimatorCarryWeightServerRpc(float _weight)
    {
        ChangeAnimatorCarryWeightClientRpc(_weight);
    }

    [ClientRpc]
    void ChangeAnimatorCarryWeightClientRpc(float _weight)
    {
        animator.SetLayerWeight(1, _weight);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnObjectOnServerRpc(string _objName, Vector3 _position, Quaternion _rotation, bool solvingTask)
    {   
        Debug.Log($"Trying to spawn {_objName}");
        GameObject _newObj = Instantiate(objectPrefabsDict[_objName], _position, _rotation);
        // placableObject = _newObj;
        NetworkObject _newObjInstance = _newObj.GetComponent<NetworkObject>();

        PlacableObjects placedObject = _newObjInstance.gameObject.GetComponent<PlacableObjects>();
        placedObject.PlaceObject(this);
        _newObjInstance.Spawn();
        NotifyClientOfSpawnClientRpc(_newObjInstance.NetworkObjectId, solvingTask);
    }

    [ClientRpc]
    void NotifyClientOfSpawnClientRpc(ulong spawnedObjectId, bool _success)
    {
        // Find the spawned object by ID
        NetworkObject spawnedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[spawnedObjectId];
        PlacableObjects placedObject = spawnedObject.gameObject.GetComponent<PlacableObjects>();
        placedObject.PlaceObject(this);
    }

    void OnDestroy()
    {
        Destroy(lionCam.gameObject);
    }
}