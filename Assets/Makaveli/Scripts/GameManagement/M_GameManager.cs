using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGameManager : MonoBehaviour
{
    public static MGameManager instance;
    public List<GameObject> markers = new();
    public List<GameObject> objectsToTrack = new();
    // public List<UILocationCard> cards = new();
    private readonly List<ITriggerMovement> triggerMovements = new();

    // NPC related stuff
    public Transform walkableArea;
    public GameObject trackableObject;
    public Transform trackableObjectParent;
    public int trackableObjectAmount;
    public GameObject patrolArea;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.parent.gameObject); 
        }
        else
        {
            Destroy(transform.parent.gameObject); 
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

    private void TriggerAllMovements(Transform location)
    {
        foreach (var triggerMovement in triggerMovements)
        {
            triggerMovement.TriggerMovement(location);
        }
    }

    private void Update()
    {
        // Gotta move this to the crowd player class
        // if(cards.Count > 0) 
        // {
        //     for (int i = 0; i < cards.Count; i++)
        //     {
        //         var card = cards[i];
        //         card.btn.onClick.AddListener(() => 
        //         {
        //             Debug.Log("Button clicked");
        //             TriggerAllMovements(card.location);
        //         });
        //     }
        // }

        if(Input.GetKeyDown(KeyCode.Tab)) 
        {
            for (int i = 0; i < trackableObjectAmount; i++)
            {
                InstantiatePrefab(trackableObject, trackableObjectParent);
            }
        }
    }

    public GameObject InstantiatePrefab(GameObject prefab, Transform parent) 
    {
        GameObject newGameObject = Instantiate(prefab);
        newGameObject.transform.SetParent(parent, true);

        return newGameObject;
    }
}

public interface ITriggerMovement 
{
    public void TriggerMovement(Transform transform);
}