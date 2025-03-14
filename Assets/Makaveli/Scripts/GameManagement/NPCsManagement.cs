using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NPCsManagement 
{
    private static readonly List<ITriggerMovement> triggerMovements = new();

    public static IEnumerator SpawnNPC
    (
        List<GameObject> NPCs, 
        int npcCount, 
        GameObject npc,
        Vector3 position, 
        Transform npcContainer
    ) 
    {
        for (int i = 0; i < npcCount; i++)
        {
            GameObject newNPC = MGameManager.instance.InstantiatePrefab(npc, position, npc.transform.rotation, npcContainer);
            NPCs.Add(newNPC);

            yield return null;
        }

        yield return null;

        // Debug.Log($"Trigger movements list count: {triggerMovements.Count}");
        // Debug.Log($"NPC list count: {NPCs.Count}");
        yield break;
    }

    // Movement
    public static void RegisterTriggerMovement(ITriggerMovement triggerMovement) 
    {
        if(!triggerMovements.Contains(triggerMovement)) triggerMovements.Add(triggerMovement);
    }

    public static void UnregisterTriggerMovement(ITriggerMovement triggerMovement)
    {
        if(triggerMovements.Contains(triggerMovement)) triggerMovements.Remove(triggerMovement);
    }

    public static void TriggerAllMovements(Transform location) 
    {
        foreach(var triggerMovement in triggerMovements)
        {
            triggerMovement.TriggerMovement(location);
        } 
    }
}
