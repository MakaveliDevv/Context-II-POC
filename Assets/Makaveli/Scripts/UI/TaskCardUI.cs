using UnityEngine;
using UnityEngine.UI;

public class TaskCardUI : MonoBehaviour
{
    public Task task;
    private Image image;

    private void Start() 
    {
        image = GetComponent<Image>();
        image.sprite = task.sprite;
    }
}
