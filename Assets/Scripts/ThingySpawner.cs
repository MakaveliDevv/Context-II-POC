using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThingySpawner : MonoBehaviour
{
    public GameObject objectToSpawn; 
    public Transform spawnArea; 
    public Transform patrolArea;
    public List<Transform> randomLocations;

    public int spawnCount = 5; 
    public float spawnInterval = 2f; 
    public float minSpawnDistance = 1.5f; 
    public float patrolSpeed = 3f; 

    private void Start()
    {
        if (spawnArea == null)
        {
            Debug.LogWarning("Spawn area not assigned! Using default position.");
            spawnArea = transform;
        }

        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            if (objectToSpawn != null && spawnArea != null)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                GameObject spawnedObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
                // spawnedObject.layer = LayerMask.NameToLayer("Obstacle");

              
                if (spawnedObject.TryGetComponent<Renderer>(out var objectRenderer))
                {
                    objectRenderer.material = new Material(objectRenderer.material)
                    {
                        color = new Color(Random.value, Random.value, Random.value)
                    };
                }
                
                // Add a patrol script to the spawned object
                // spawnedObject.AddComponent<ThingyPatrol>();
            }
            else
            {
                Debug.LogError("Missing object to spawn or spawn area!");
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPosition;
        bool positionValid;
        int attempt = 0;

        // Get the spawn area size based on the collider bounds
        // Vector3 areaSize = spawnArea.GetComponent<Collider>()?.bounds.size ?? Vector3.one;
        Vector3 areaSize;

        if(spawnArea.TryGetComponent<BoxCollider>(out var boxCollider)) { areaSize = boxCollider.bounds.size; }
        else { areaSize = Vector3.one; }

        do
        {
            float randomX = Random.Range(-areaSize.x / 2, areaSize.x / 2);
            float randomZ = Random.Range(-areaSize.z / 2, areaSize.z / 2); // Use z for width
            randomPosition = spawnArea.position + new Vector3(randomX, .5f, randomZ);

            positionValid = !Physics.CheckSphere(randomPosition, minSpawnDistance);
            attempt++;
        }
        while (!positionValid && attempt < 10);

        return randomPosition;
    }
}