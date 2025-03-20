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
    [SerializeField] public Vector3 spawnOffset;

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

    public void PlaceObject()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Log($"Changing {meshRenderer.gameObject.name}'s material to {originalMaterial.name}");
        meshRenderer.material = originalMaterial;
        placed = true;
        GetComponent<Collider>().isTrigger = false;
    }
}
