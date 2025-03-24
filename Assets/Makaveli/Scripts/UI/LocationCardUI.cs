using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationCardUI : MonoBehaviour
{
    public Transform location;
    public Vector3 objectPosition;
    public Button btn;    
    public RenderTexture renderTexture; 
    public List<Task> tasks = new();

    void Start()
    {
        if(btn == null) 
        {
            btn = gameObject.GetComponent<Button>();
        }
    }
}
