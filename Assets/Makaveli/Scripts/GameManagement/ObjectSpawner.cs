using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject trackableObjectPrefab;
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
            if (trackableObjectPrefab != null)
            {
                Vector3 randomPosition = GetRandomPositionWithinBounds();
                GameObject spawnedObject = Instantiate(trackableObjectPrefab, randomPosition, Quaternion.identity);

                MiniMapTracker tracker = FindFirstObjectByType<MiniMapTracker>();
                if (tracker != null)
                {
                    tracker.AddTrackedObject(spawnedObject.transform);
                    MGameManager.instance.objectsToTrack.Add(spawnedObject);
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
