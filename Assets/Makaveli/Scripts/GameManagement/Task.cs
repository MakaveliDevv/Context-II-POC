using UnityEngine;

[CreateAssetMenu(fileName = "NewTask", menuName =  "Task System")]
public class Task : ScriptableObject
{
    public Sprite sprite;
    public string taskName;
    public string description;
    public string taskText;
    public bool isCompleted;
}
