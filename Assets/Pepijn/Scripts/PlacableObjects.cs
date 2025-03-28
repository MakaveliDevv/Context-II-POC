using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlacableObjects : NetworkBehaviour
{
    public string objName;
    public bool placable, placed;
    [SerializeField] List<GameObject> collidingObjects = new();
    [SerializeField] Material unplacableMaterial, placableMaterial;
    [SerializeField] Material originalMaterial;
    MeshRenderer meshRenderer;
    public Vector3 spawnOffset;
    public bool isIndicator;
    [Range(0, 1)] public float weight;
    public Vector3 holdingScale;
    public Vector3 holdingOffset;

    // -------------------------------------
    public Task task;

    void OnTriggerEnter(Collider collider)
    {
        if(!isIndicator) return;

        if(!placed && collider.gameObject.layer != 9)
        {
            collidingObjects.Add(collider.gameObject);
            CheckIfPlacable();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(!isIndicator) return;

        if(!placed && collider.gameObject.layer != 9)
        {
            collidingObjects.Remove(collider.gameObject);
            CheckIfPlacable();
        }
    }

    public void CheckIfPlacable()
    {
        if(!isIndicator) return;

        bool touchingGround = false, touchingOther = false;

        foreach(GameObject obj in collidingObjects)
        {
            if(obj.layer == 12) touchingGround = true;
            else touchingOther = true;
        }

        if(meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();


        if(touchingGround && !touchingOther)
        {
            ChangePlacability(true);
        }
        else 
        {
            ChangePlacability(false);
        }
    }

    void ChangePlacability(bool _placable)
    {
        if(_placable)
        {
            Debug.Log($"Changing {meshRenderer.gameObject.name}'s material to {placableMaterial.name}");
            meshRenderer.material = placableMaterial;
            placable = true;
        }
        else
        {
            Debug.Log($"Changing {meshRenderer.gameObject.name}'s material to {unplacableMaterial.name}");
            meshRenderer.material = unplacableMaterial;
            placable = false;
        }
    }

    public void PlaceObject(Lion _lion)
    {
        // meshRenderer = GetComponent<MeshRenderer>();
        // Debug.Log($"Changing {meshRenderer.gameObject.name}'s material to {originalMaterial.name}");
        // meshRenderer.material = originalMaterial;

        placed = true;
        GetComponent<Collider>().isTrigger = false;

        bool foundValidLocation = false;
        float closestDistance = 1000f;

        TaskLocation[] _taskLocations = FindObjectsOfType<TaskLocation>();
        foreach(TaskLocation taskLoc in _taskLocations)
        {
            //if(taskLoc.isActive)
            //{
                Transform _task = taskLoc.transform;
                float distanceToTaskableLocation = Vector3.Distance(_task.position, transform.position);
                Debug.Log($"Object distance: {distanceToTaskableLocation} to {_task.gameObject.name}");
                
                if(distanceToTaskableLocation < 10f)
                {
                    foundValidLocation = true;

                    if(distanceToTaskableLocation < closestDistance)
                    {
                        closestDistance = distanceToTaskableLocation;
                        _lion.taskLocation = _task;
                        _lion.taskLocationRef = taskLoc;
                        UpdateTaskLocationServerRpc();
                    }
                }
            //}
        }

        if (!foundValidLocation)
        {
            MGameManager.instance.UpdatePoints(-1);
            Debug.Log("Object placed in a non-taskable location, and no valid location nearby. -1 point applied");
            StartCoroutine(DestroyObject(_lion.wrongObjectText, "Wrong location! Removing object..."));
        }
        else if (!_lion.taskLocationRef.tasks.Contains(task))
        {
            MGameManager.instance.UpdatePoints(-1);
            Debug.Log("Wrong placed in location, and no valid location nearby. -1 point applied");
            StartCoroutine(DestroyObject(_lion.wrongObjectText, "Wrong object! Removing object..."));
        }
        else
        {
            MGameManager.instance.lionPlacedObject = true;
            Debug.Log("Object placed in a valid location");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateTaskLocationServerRpc()
    {
        placed = true;
        GetComponent<Collider>().isTrigger = false;
        float closestDistance = 1000f;
        Lion _lion = FindFirstObjectByType<Lion>();

        TaskLocation[] _taskLocations = FindObjectsOfType<TaskLocation>();
        foreach(TaskLocation taskLoc in _taskLocations)
        {
            //if(taskLoc.isActive)
            //{
                Transform _task = taskLoc.transform;
                float distanceToTaskableLocation = Vector3.Distance(_task.position, transform.position);
                Debug.Log($"Object distance: {distanceToTaskableLocation} to {_task.gameObject.name}");
                
                if(distanceToTaskableLocation < 10f)
                {
                    if(distanceToTaskableLocation < closestDistance)
                    {
                        closestDistance = distanceToTaskableLocation;
                        _lion.taskLocation = _task;
                        _lion.taskLocationRef = taskLoc;
                    }
                }
            //}
        }
        UpdateTaskLocationClientRpc();
    }
    [ClientRpc]
    void UpdateTaskLocationClientRpc()
    {
        placed = true;
        GetComponent<Collider>().isTrigger = false;
        float closestDistance = 1000f;
                Lion _lion = FindFirstObjectByType<Lion>();

        TaskLocation[] _taskLocations = FindObjectsOfType<TaskLocation>();
        foreach(TaskLocation taskLoc in _taskLocations)
        {
            //if(taskLoc.isActive)
            //{
                Transform _task = taskLoc.transform;
                float distanceToTaskableLocation = Vector3.Distance(_task.position, transform.position);
                Debug.Log($"Object distance: {distanceToTaskableLocation} to {_task.gameObject.name}");
                
                if(distanceToTaskableLocation < 10f)
                {
                    if(distanceToTaskableLocation < closestDistance)
                    {
                        closestDistance = distanceToTaskableLocation;
                        _lion.taskLocation = _task;
                        _lion.taskLocationRef = taskLoc;
                    }
                }
            //}
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyThisServerRpc()
    {
        NetworkObject _netObj = GetComponent<NetworkObject>();
        Debug.Log("Object being deleted: " + _netObj.name);
        _netObj.Despawn();
        Destroy(_netObj);
    }

    IEnumerator DestroyObject(TextMeshProUGUI _lionText, string _text)
    {
        _lionText.text = _text;
        yield return new WaitForSeconds(3);
        if(_lionText.text == _text) _lionText.text = "";
        else
        {
            Debug.Log($"Text no longer the same: [{_text}] != [{_lionText.text}] ");
        }
        DestroyThisServerRpc();
    }
}
