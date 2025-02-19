using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    // public GameObject[] objectsToSpawn;
    public GameObject objectToSpawn; 
    public Transform spawnPoint; 
    public float spawnInterval = 2f; 

    private void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point not assigned! Using default position.");
            spawnPoint = transform;
        }
        // StartCoroutine(SpawnObjects());
        SpawnObject();
    }

    // private IEnumerator SpawnObjects()
    // {
    //     for (int i = 0; i < objectsToSpawn.Length; i++)
    //     {
    //         if (objectsToSpawn[i] != null && spawnPoint != null)
    //         {
    //             Instantiate(objectsToSpawn[i], spawnPoint.position, spawnPoint.rotation);
    //         }
    //         else
    //         {
    //             Debug.LogError("Missing object to spawn or spawn point!");
    //         }
    //         yield return new WaitForSeconds(spawnInterval);
    //     }
    // }

    public void SpawnObject() 
    {
        if(objectToSpawn != null && spawnPoint != null) 
        {
            Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("Missing object to spawn or spawn point!");
        }
    }
}

