using UnityEngine;
using UnityEngine.UI;

public class UICard : MonoBehaviour
{
    public Transform location;
    public Vector3 objectPosition;
    public Button btn;    
    public RenderTexture renderTexture; 

    void Start()
    {
        if(btn == null) 
        {
            btn = gameObject.GetComponent<Button>();
        }
    }
}
