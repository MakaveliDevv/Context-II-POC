using System.Collections.Generic;
using UnityEngine;

public class TaskLocation : MonoBehaviour
{
    public Transform playerCam;
    public GameObject indicator;
    Transform pin;
    public List<Task> tasks = new();
    public bool fixable = true;
    public bool locationFixed;
    public bool isActive;

    void Start()
    {
        pin = indicator.transform.GetChild(0);  
    }
    void Update()
    {
        if(playerCam != null)
        {
            Vector3 direction = playerCam.position - transform.position;
            direction.y = 0; // Ignore vertical difference to keep rotation on the Y-axis

            pin.rotation = Quaternion.LookRotation(direction);
        }
        else pin.rotation = Quaternion.identity;
    }
}
