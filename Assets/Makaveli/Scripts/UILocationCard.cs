using UnityEngine;
using UnityEngine.UI;

public class UILocationCard : MonoBehaviour
{
    public Transform objectTransform;
    public Vector3 objectPosition;
    public Button btn;    

    void Update()
    {
        if(btn == null) 
        {
            btn = gameObject.GetComponent<Button>();
        }

        if(!Manager.instance.UIPanel.Contains(this)) 
        {
            Manager.instance.UIPanel.Add(this);

            Debug.Log("List doesnt contain this card");

        }
        else 
        {
            Debug.Log("List already contains this card");
        }
    }
}
