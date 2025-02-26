using System.Collections.Generic;
using UnityEngine;

public class MiniMapTracker : MonoBehaviour
{
    public Transform miniMapCamera; 
    public RectTransform miniMapUI; 
    public GameObject markerPrefab;  

    private Camera miniMapCam;
    private readonly Dictionary<Transform, RectTransform> trackedObjects = new();

    void Start()
    {
        miniMapCam = miniMapCamera.GetComponent<Camera>(); 
    }

    void Update()
    {
        if (miniMapCam == null || trackedObjects.Count == 0) return;

        float worldMapHalfSize = miniMapCam.orthographicSize;
        float worldMapSize = worldMapHalfSize * 2;

        foreach (var entry in trackedObjects)
        {
            Transform objectToTrack = entry.Key;
            RectTransform markerTransform = entry.Value;

            if (objectToTrack == null)
            {
                Destroy(markerTransform.gameObject);
                continue;
            }

            Vector3 relativePos = objectToTrack.position - miniMapCamera.position;

            float x = relativePos.x / worldMapSize * miniMapUI.sizeDelta.x;
            float y = relativePos.z / worldMapSize * miniMapUI.sizeDelta.y;

            float clampedX = Mathf.Clamp(x, -miniMapUI.sizeDelta.x / 2, miniMapUI.sizeDelta.x / 2);
            float clampedY = Mathf.Clamp(y, -miniMapUI.sizeDelta.y / 2, miniMapUI.sizeDelta.y / 2);

            Vector3 objectScreenPos = miniMapCam.WorldToViewportPoint(objectToTrack.position);
            bool isObjectVisible = objectScreenPos.x >= 0 && objectScreenPos.x <= 1 && objectScreenPos.y >= 0 && objectScreenPos.y <= 1;

            if (isObjectVisible)
            {
                if (!markerTransform.gameObject.activeSelf)
                {
                    markerTransform.gameObject.SetActive(true);
                }

                markerTransform.anchoredPosition = new Vector2(clampedX, clampedY);
            }
            else
            {
                if (markerTransform.gameObject.activeSelf)
                {
                    markerTransform.gameObject.SetActive(false);
                }
            }
        }
    }

    public void AddTrackedObject(Transform newObject)
    {
        if (!trackedObjects.ContainsKey(newObject))
        {
            GameObject newMarker = Instantiate(markerPrefab, miniMapUI);
            MGameManager.instance.markers.Add(newMarker);
            RectTransform markerTransform = newMarker.GetComponent<RectTransform>();
            trackedObjects.Add(newObject, markerTransform);
        }
    }

    public void RemoveTrackedObject(Transform objectToRemove)
    {
        if (trackedObjects.ContainsKey(objectToRemove))
        {
            Destroy(trackedObjects[objectToRemove].gameObject);
            trackedObjects.Remove(objectToRemove);
        }
    }
}
