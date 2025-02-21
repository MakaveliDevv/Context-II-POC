using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager instance;
    public List<GameObject> markers = new();
    public List<GameObject> objectsToTrack = new();
    public List<UILocationCard> UIPanel = new();
    private List<ITriggerMovement> triggerMovements = new(); // Store all ITriggerMovement implementations

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    public void RegisterTriggerMovement(ITriggerMovement triggerMovement)
    {
        if (!triggerMovements.Contains(triggerMovement))
        {
            triggerMovements.Add(triggerMovement);
        }
    }

    public void UnregisterTriggerMovement(ITriggerMovement triggerMovement)
    {
        if (triggerMovements.Contains(triggerMovement))
        {
            triggerMovements.Remove(triggerMovement);
        }
    }

    public void TriggerAllMovements(Transform transform)
    {
        foreach (var triggerMovement in triggerMovements)
        {
            triggerMovement.TriggerMovement(transform);
        }
    }

    private void Update()
    {
        if(UIPanel.Count > 0) 
        {
            for (int i = 0; i < UIPanel.Count; i++)
            {
                var card = UIPanel[i];
                card.btn.onClick.AddListener(() => 
                {
                    Debug.Log("Button clicked");
                    TriggerAllMovements(card.objectTransform);
                });
            }
        }
    }
}

public interface ITriggerMovement 
{
    public void TriggerMovement(Transform transform);
}