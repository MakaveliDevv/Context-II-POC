using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskCardUI : MonoBehaviour
{
    public Task task;
    private Image image;
    TextMeshProUGUI taskText;

    private void Start() 
    {
        image = GetComponent<Image>();
        image.sprite = task.sprite;
    }

    public void SetCardText(string _text)
    {
        if(taskText == null) taskText = transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        taskText.text = _text;
    }
}
