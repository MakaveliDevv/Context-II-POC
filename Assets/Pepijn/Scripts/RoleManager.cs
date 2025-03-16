using System.Collections.Generic;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    [SerializeField] public bool isLion;
    [SerializeField] List<GameObject> lionObjects, littleGuysObjects;

    void Start()
    {
        if(isLion) InstantiateLion();
        else InstantiateLittleGuy();
    }

    void InstantiateLion()
    {
        foreach(GameObject _go in littleGuysObjects)
        {
            _go.SetActive(false);
        }
    }

    void InstantiateLittleGuy()
    {
        foreach(GameObject _go in lionObjects)
        {
            _go.SetActive(false);
        }
    }

}
