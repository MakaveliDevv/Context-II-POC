using System.Collections.Generic;
using UnityEngine;

public class TaskLocation : MonoBehaviour
{
    public List<Task> tasks = new();
    public bool fixable = true;
    public bool locationFixed;
}
