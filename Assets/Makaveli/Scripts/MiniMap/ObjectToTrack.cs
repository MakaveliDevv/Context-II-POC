using UnityEngine;

public class ObjectToTrack : MonoBehaviour 
{
    public RenderTexture renderTexture;

    private void Start()
    {
        Camera camera = transform.GetChild(0).gameObject.GetComponent<Camera>();
        renderTexture = new RenderTexture(256, 256, 16); 
        renderTexture.Create();

        camera.targetTexture = renderTexture;
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}

