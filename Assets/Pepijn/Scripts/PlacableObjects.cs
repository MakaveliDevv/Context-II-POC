using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacableObjects : MonoBehaviour
{
    public string objName;
    public bool placable, placed;
    List<GameObject> collidingObjects = new();
    [SerializeField] Material unplacableMaterial, placableMaterial;
    [SerializeField] Material originalMaterial;
    MeshRenderer meshRenderer;
    public Vector3 spawnOffset;

    // -------------------------------------
    public Task task;

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.layer != 12 && !placed && collider.gameObject.layer != 9)
        {
            collidingObjects.Add(collider.gameObject);
            CheckIfPlacable();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(collider.gameObject.layer != 12 && !placed && collider.gameObject.layer != 9)
        {
            collidingObjects.Remove(collider.gameObject);
            CheckIfPlacable();
        }
    }

    public void CheckIfPlacable()
    {
        if(meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if(collidingObjects.Count == 0)
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

    public void PlaceObject(GameObject go)
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Log($"Changing {meshRenderer.gameObject.name}'s material to {originalMaterial.name}");
        meshRenderer.material = originalMaterial;
        placed = true;
        GetComponent<Collider>().isTrigger = false;

        if(MGameManager.instance.gamePlayManagement != MGameManager.GamePlayManagement.SOLVING_TASK) 
        {
            MGameManager.instance.currentPoint -= 1;
            Debug.Log("object placed in not solving state so -1 point is applied");
            
            // DESTROY GAME OBJECT
            // this object needs to be destroyed if it is placed outside the solving task state
            // Destroy the object after a few seconds
            // Destroy(go, 5f);           
        }
        else if(MGameManager.instance.gamePlayManagement == MGameManager.GamePlayManagement.SOLVING_TASK) 
        {
            if(go.TryGetComponent<PlacableObjects>(out var placableObject)) 
            {
                Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 1.5f); 
                bool foundValidLocation = false;

                foreach (var nearbyCollider in nearbyColliders)
                {
                    if (nearbyCollider.CompareTag("TaskableLocation"))
                    {
                        foundValidLocation = true;
                        break; 
                    }
                }

                if (!foundValidLocation)
                {
                    MGameManager.instance.currentPoint -= 1;
                    Debug.Log("Object placed in a non-taskable location, and no valid location nearby. -1 point applied");

                    // DESTROY GAME OBJECT
                    // this object needs to be destroyed if it is placed in the solving task state but not inside a task location
                    // Destroy the object after a few seconds
                    // Destroy(go, 5f);
                }
            }
            else 
            {
                Debug.LogError("Couldnt fetch the PlacableObjects component");
            }
        }
    }
}
