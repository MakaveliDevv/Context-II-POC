using UnityEngine;

public class LionButton : MonoBehaviour
{
    Lion lion;
    void Start()
    {
        lion = FindFirstObjectByType<Lion>();
    }

    public void SelectObject(string objectName)
    {
        lion.SpawnObject(objectName);
    }

}
              