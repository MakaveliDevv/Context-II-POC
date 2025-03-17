using UnityEngine;

public class ObjectToTrack : MonoBehaviour 
{
    public RenderTexture renderTexture;
    [SerializeField] private float spawnHeightOffset;
    private MiniMapTracker miniMapTracker;

    private void Start()
    {
        Camera camera = transform.GetChild(0).gameObject.GetComponent<Camera>();
        renderTexture = new RenderTexture(256, 256, 16); 
        renderTexture.Create();

        camera.targetTexture = renderTexture;

        MGameManager.instance.trackables.Add(gameObject);

        miniMapTracker = FindFirstObjectByType<MiniMapTracker>();
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        // Remove from tracker dictionary
        if (miniMapTracker != null)
        {
            miniMapTracker.RemoveTrackedObject(transform);
        }
        else
        {
            // If our cached reference is null for some reason, try to find it again
            miniMapTracker = FindFirstObjectByType<MiniMapTracker>();
            if (miniMapTracker != null)
            {
                miniMapTracker.RemoveTrackedObject(transform);
            }
        }
    }

    public void InitializePosition(Transform location) 
    {
        transform.position = location.position; 
        MiniMapTracker tracker = FindFirstObjectByType<MiniMapTracker>();

        if (tracker != null) 
        {
            tracker.AddTrackedObject(gameObject.transform);
        }
        else 
        { 
            //Debug.LogError("No minimap found!"); 
        }
    }

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
    
}

