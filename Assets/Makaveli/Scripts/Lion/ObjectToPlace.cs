using UnityEngine;

public class ObjectToPlace : MonoBehaviour
{
    // private bool isInRange = false;

    private void OnTriggerEnter(Collider collider) 
    {       
        if(collider.CompareTag("Player")) 
        {
            Debug.Log($"Collision detected with {collider.gameObject.tag}");

            // isInRange = true;

            if(Input.GetKeyDown(KeyCode.Space)) 
            {
                Transform childTransform = collider.gameObject.transform.GetChild(1);
                Debug.Log(childTransform.gameObject.name);
                transform.SetParent(childTransform, true);
            }
        }
    }
}
