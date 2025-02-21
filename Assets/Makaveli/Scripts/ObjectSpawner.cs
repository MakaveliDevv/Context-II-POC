// using UnityEngine;

// public class ObjectSpawner : MonoBehaviour
// {
//     public GameObject objectToSpawn; 
//     public Transform spawnPoint; 
//     public float spawnInterval = 2f; 
//     public float amountOfObjectsToSpawn;

//     private void Start()
//     {
//         if (spawnPoint == null)
//         {
//             Debug.LogWarning("Spawn point not assigned! Using default position.");
//             spawnPoint = transform;
//         }

//         SpawnObject();
//     }

//     public void SpawnObject() 
//     {
//         for (int i = 0; i < amountOfObjectsToSpawn; i++)
//         {
//             if(objectToSpawn != null && spawnPoint != null) 
//             {
//                 GameObject spawnedObject = Instantiate(objectToSpawn, new Vector3(spawnPoint.position.x, spawnPoint.position.y - .5f, spawnPoint.position.z), spawnPoint.rotation);

//                 // Assign the spawned object to the mini-map tracker
//                 MiniMapTracker tracker = FindFirstObjectByType<MiniMapTracker>();
//                 if (tracker != null)
//                 {
//                     // tracker.SetTrackedObject(spawnedObject.transform);
//                     tracker.AddTrackedObject(spawnedObject.transform);
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Missing object to spawn or spawn point!");
//             }
//         }
//     }
// }



using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject trackableObject;
    public float amountOfObjectsToSpawn;
    public float spawnHeightOffset = 0.5f; 

    private Collider spawnArea;

    private void Start()
    {
        spawnArea = GetComponent<Collider>();

        if (spawnArea == null)
        {
            Debug.LogError("ObjectSpawner requires a Collider on the GameObject to define spawn bounds.");
            return;
        }

        SpawnObjects();
    }

    public void SpawnObjects()
    {
        for (int i = 0; i < amountOfObjectsToSpawn; i++)
        {
            if (trackableObject != null)
            {
                Vector3 randomPosition = GetRandomPositionWithinBounds();
                GameObject spawnedObject = Instantiate(trackableObject, randomPosition, Quaternion.identity);
                // ObjectToTrack objectToTrack = spawnedObject.GetComponent<ObjectToTrack>();

                MiniMapTracker tracker = FindFirstObjectByType<MiniMapTracker>();
                if (tracker != null)
                {
                    tracker.AddTrackedObject(spawnedObject.transform);
                    // Manager.instance.markers.Add(spawnedObject);

                    Manager.instance.objectsToTrack.Add(spawnedObject);
                    
                }
            }
            else
            {
                Debug.LogError("Missing object to spawn!");
            }
        }
    }

    private Vector3 GetRandomPositionWithinBounds()
    {
        if (spawnArea == null)
        {
            Debug.LogError("Spawn area collider is missing!");
            return transform.position;
        }

        Bounds bounds = spawnArea.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        float spawnY = bounds.min.y + spawnHeightOffset;

        return new Vector3(randomX, spawnY, randomZ);
    }
}
