using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NPCsManagement 
{
    private static readonly List<ITriggerMovement> triggerMovements = new();

    public static IEnumerator SpawnNPC
    (
        List<GameObject> npcs, 
        int npcCount, 
        GameObject npc,
        Vector3 position, 
        Transform npcContainer
    ) 
    {
        for (int i = 0; i < npcCount; i++)
        {
            GameObject newNPC = MGameManager.instance.InstantiatePrefab(npc, position, npc.transform.rotation, npcContainer);
            npcs.Add(newNPC);

            yield return null;
        }

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

    public static IEnumerator StopNPCMovement(List<GameObject> npcs) 
    {
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < npcs.Count; i++)
        {
            var npc = npcs[i].GetComponent<NPCManager>();
            npc.moveable = false;

        }

        yield break;
    }

    public static IEnumerator ResumeNPCMovement(List<GameObject> npcs, Transform formationManag) 
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            var npc = npcs[i].GetComponent<NPCManager>();
            npc.moveable = true;
        }

        NPCFormationManager formationManager = formationManag.GetComponent<NPCFormationManager>();
        formationManager.currentFormation = FormationType.Follow;

        yield break;
    }

}
