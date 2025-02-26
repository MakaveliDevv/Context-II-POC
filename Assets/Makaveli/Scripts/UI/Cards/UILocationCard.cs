using UnityEngine;
using UnityEngine.UI;

public class UILocationCard : MonoBehaviour
{
    public Transform location;
    public Vector3 objectPosition;
    public Button btn;    
    public RenderTexture renderTexture; 

    void Update()
    {
        if(btn == null) 
        {
            btn = gameObject.GetComponent<Button>();
        }

        if(!MGameManager.instance.cards.Contains(this)) 
        {
            MGameManager.instance.cards.Add(this);
        }
    }
}
