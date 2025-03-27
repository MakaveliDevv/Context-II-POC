using System.Collections;
using System.Collections.Generic;
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
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Log($"Changing {meshRenderer.gameObject.name}'s material to {originalMaterial.name}");
        meshRenderer.material = originalMaterial;
        placed = true;
        GetComponent<Collider>().isTrigger = false;

        if(MGameManager.instance.gamePlayManagement != MGameManager.GamePlayManagement.SOLVING_TASK) 
        {
            MGameManager.instance.UpdatePoints(-1);
            Debug.Log("object placed in not solving state so -1 point is applied");
            StartCoroutine(DestroyObject());           
        }
        else if(MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.SOLVING_TASK) 
        {
            Collider collider = GetComponent<Collider>();
            Vector3 colliderCenter = collider.bounds.center;
            Vector3 colliderExtents = collider.bounds.extents;
            Vector3 undersidePosition = new Vector3(colliderCenter.x, colliderCenter.y - colliderExtents.y, colliderCenter.z);

            // Collider[] nearbyColliders = Physics.OverlapSphere(undersidePosition, 1.5f); 
            bool foundValidLocation = false;
            // if(nearbyColliders.Length == 0) Debug.Log($"Object Found: no objects found");
            // foreach (var nearbyCollider in nearbyColliders)
            // {
            //     Debug.Log($"Object Found: {nearbyCollider.gameObject.name}");
            //     if (nearbyCollider.CompareTag("TaskableLocation"))
            //     {
            //         foundValidLocation = true;
            //         MGameManager.instance.lionPlacedObject = true;
            //         Debug.Log("Object placed in a valid location");
            //         break; 
            //     }
            // }

            TaskLocation[] _taskLocations = FindObjectsOfType<TaskLocation>();
            foreach(TaskLocation taskLoc in _taskLocations)
            {
                Transform _task = taskLoc.transform;
                float distanceToTaskableLocation = Vector3.Distance(_task.position, transform.position);
                Debug.Log($"Object distance: {distanceToTaskableLocation} to {_task.gameObject.name}");
                if(distanceToTaskableLocation < 10f)
                {
                    _lion.taskLocation = _task;
                    _lion.taskLocationRef = taskLoc;
                    foundValidLocation = true;
                    MGameManager.instance.lionPlacedObject = true;
                    Debug.Log("Object placed in a valid location");
                    break; 
                }
            }

            if (!foundValidLocation)
            {
                MGameManager.instance.UpdatePoints(-1);
                //for now, to advance the game anyways
                MGameManager.instance.lionPlacedObject = true;
                Debug.Log("Object placed in a non-taskable location, and no valid location nearby. -1 point applied");
                StartCoroutine(DestroyObject());
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyThisServerRpc()
    {
        NetworkObject _netObj = GetComponent<NetworkObject>();
        _netObj.Despawn();
        Destroy(_netObj);
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(5);
        DestroyThisServerRpc();
    }
}
