// using System.Collections;
// using UnityEngine;

// public class MiniMapTracker : MonoBehaviour
// {
//     public Transform objectToTrack;
//     public Transform miniMapCamera; 
//     public RectTransform miniMapPanel; 
//     private RectTransform markerTransform;
//     private Camera miniMapCam;
//     // private ObjectToTrack objectToTrackComponent; 

//     void Start()
//     {
//         markerTransform = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
//         miniMapCam = miniMapCamera.GetComponent<Camera>(); 
//     }

//     void Update()
//     {
//         if (objectToTrack == null || miniMapCam == null) return;

//         float worldMapHalfSize = miniMapCam.orthographicSize;
//         float worldMapSize = worldMapHalfSize * 2;

//         Vector3 relativePos = objectToTrack.position - miniMapCamera.position;

//         float x = relativePos.x / worldMapSize * miniMapPanel.sizeDelta.x;
//         float y = relativePos.z / worldMapSize * miniMapPanel.sizeDelta.y;

//         float clampedX = Mathf.Clamp(x, -miniMapPanel.sizeDelta.x / 2, miniMapPanel.sizeDelta.x / 2);
//         float clampedY = Mathf.Clamp(y, -miniMapPanel.sizeDelta.y / 2, miniMapPanel.sizeDelta.y / 2);

//         Vector3 objectScreenPos = miniMapCam.WorldToViewportPoint(objectToTrack.position);
//         bool isObjectVisible = objectScreenPos.x >= 0 && objectScreenPos.x <= 1 && objectScreenPos.y >= 0 && objectScreenPos.y <= 1;

//         if(objectToTrack != null) 
//         {
//             if (isObjectVisible)
//             {
//                 if (!markerTransform.gameObject.activeSelf)
//                 {
//                     markerTransform.gameObject.SetActive(true); 
//                 }

//                 markerTransform.anchoredPosition = new Vector2(clampedX, clampedY); 
//             }
//             else
//             {
//                 if (markerTransform.gameObject.activeSelf)
//                 {
//                     markerTransform.gameObject.SetActive(false); 
//                 }
//             }
//         }
//     }

//     public void SetTrackedObject(Transform newObject)
//     {
//         objectToTrack = newObject;
//     }
// }


using System.Collections.Generic;
using UnityEngine;

public class MiniMapTracker : MonoBehaviour
{
    public Transform miniMapCamera; 
    public RectTransform miniMapPanel; 
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

            float x = relativePos.x / worldMapSize * miniMapPanel.sizeDelta.x;
            float y = relativePos.z / worldMapSize * miniMapPanel.sizeDelta.y;

            float clampedX = Mathf.Clamp(x, -miniMapPanel.sizeDelta.x / 2, miniMapPanel.sizeDelta.x / 2);
            float clampedY = Mathf.Clamp(y, -miniMapPanel.sizeDelta.y / 2, miniMapPanel.sizeDelta.y / 2);

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
            GameObject newMarker = Instantiate(markerPrefab, miniMapPanel);
            Manager.instance.markers.Add(newMarker);
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
