using UnityEngine;

public class ObjectToTrack : MonoBehaviour 
{
    public RenderTexture renderTexture;
    [SerializeField] private float spawnHeightOffset;

    private void Start()
    {
        Camera camera = transform.GetChild(0).gameObject.GetComponent<Camera>();
        renderTexture = new RenderTexture(256, 256, 16); 
        renderTexture.Create();

        camera.targetTexture = renderTexture;

        MGameManager.instance.objectsToTrack.Add(gameObject);

        // transform.position = InitializePosition();
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    public void InitializePosition(Transform location) 
    {
        // Vector3 randomPosition = GetRandomPositionWithinBounds(location);
        // transform.position = randomPosition; // Set the position here

        transform.position = location.position; // Set the position here


        MiniMapTracker tracker = FindFirstObjectByType<MiniMapTracker>();

        if (tracker != null) 
        {
            tracker.AddTrackedObject(gameObject.transform);
        }
        else 
        { 
            Debug.LogError("No minimap found!"); 
        }
    }

    // private Vector3 InitializePosition(Transform location) 
    // {
    //     Vector3 randomPosition = GetRandomPositionWithinBounds(location);
    //     MiniMapTracker tracker = FindFirstObjectByType<MiniMapTracker>();

    //     if(tracker != null) 
    //     {
    //         tracker.AddTrackedObject(gameObject.transform);
    //     }
    //     else { Debug.LogError("No minimap found!"); }

    //     return randomPosition;
    // }

    private Vector3 GetRandomPositionWithinBounds(Transform location)
    {
        // if (MGameManager.instance.walkableArea == null)
        // {
        //     Debug.LogError("Spawn area collider is missing!");
        //     return transform.position;
        // }

        Bounds bounds = location.gameObject.GetComponent<Collider>().bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        float spawnY = bounds.min.y + spawnHeightOffset;

        return new Vector3(randomX, spawnY, randomZ);
    }

    // void OnDrawGizmos()
    // {
    //     Vector3 halfExtents = new(2f, 1f, 2f); 
    //     Gizmos.color = Color.red;
    //     Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
    //     Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
    // }
}

