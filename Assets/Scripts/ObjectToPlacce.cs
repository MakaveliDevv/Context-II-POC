using UnityEngine;

public class ObjectToPlacce : MonoBehaviour
{
    private bool isInRange = false;

    private void OnTriggerEnter(Collider collider) 
    {
        Debug.Log("Trigger entered with: " + collider.gameObject.name);  // Logs when the trigger event is fired.
        
        if(collider.CompareTag("Player")) 
        {
            Debug.Log("Collision detected!");
            isInRange = true;

            if(Input.GetKeyDown(KeyCode.Space)) 
            {
                Transform childTransform = collider.gameObject.transform.GetChild(1);
                Debug.Log(childTransform.gameObject.name);
                transform.SetParent(childTransform, true);
            }
        }
    }

}
