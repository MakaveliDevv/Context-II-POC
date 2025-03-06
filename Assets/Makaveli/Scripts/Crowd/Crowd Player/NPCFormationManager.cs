using System.Collections.Generic;
using UnityEngine;

public enum FormationType
{
    Follow, // Default flocking behavior
    Triangle,
    Rectangle,
    Circle,
    Line,
    Arrow,
    // Add more formations as needed
}

public class NPCFormationManager : MonoBehaviour
{
    [Header("Formation Settings")]
    public FormationType currentFormation = FormationType.Follow;
    public float formationSpacing = 1.5f; // Space between NPCs in formation
    public float transitionSpeed = 2.0f; // How quickly NPCs move to formation positions
    public float formationScale = 1.0f; // Overall size of the formation
    
    [Header("References")]
    public Transform playerTransform; // Reference to the player
    
    private Dictionary<FormationType, System.Func<int, int, Vector3>> formationGenerators;

    // Add this new flag to track if formations should stay in place
    [Header("Behavior Settings")]
    public bool staticFormations = true; // When true, formations stay in place and don't follow the player
    private Vector3 formationOriginPosition; // Position where the formation was created
    private Quaternion formationOriginRotation; // Rotation at formation creation
    
    private void Awake()
    {
        // Initialize the dictionary with formation generation functions
        formationGenerators = new Dictionary<FormationType, System.Func<int, int, Vector3>>
        {
            { FormationType.Triangle, GetTrianglePosition },
            { FormationType.Rectangle, GetRectanglePosition },
            { FormationType.Circle, GetCirclePosition },
            { FormationType.Line, GetLinePosition },
            { FormationType.Arrow, GetArrowPosition }
            // Add more formations as they're implemented
        };
        
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    public void UpdateFormationOrigin()
    {
        formationOriginPosition = transform.position;
        formationOriginRotation = transform.rotation;
    }
    
    public void ChangeFormation(FormationType newFormation)
    {
        // If switching to any formation other than Follow, record the current position
        if (newFormation != FormationType.Follow && currentFormation == FormationType.Follow)
        {
            UpdateFormationOrigin();
        }
        
        currentFormation = newFormation;
    }
    
    // Get the target position for an NPC in the current formation
    public Vector3 GetFormationPosition(int npcIndex, int totalNPCs)
    {
        if (currentFormation == FormationType.Follow || totalNPCs <= 1)
        {
            // Return null to indicate using default following behavior
            return Vector3.zero;
        }
        
        // Get the formation position from the appropriate function
        if (formationGenerators.TryGetValue(currentFormation, out var positionFunc))
        {
            Vector3 localPosition = positionFunc(npcIndex, totalNPCs);
            
            // Apply formation scale
            localPosition *= formationScale;
            
            if (staticFormations && currentFormation != FormationType.Follow)
            {
                // Use the stored position/rotation when formation was created
                Vector3 right = Vector3.Cross(Vector3.up, formationOriginRotation * Vector3.forward).normalized;
                Vector3 forward = formationOriginRotation * Vector3.forward;
                
                Vector3 worldOffset = right * localPosition.x + 
                                      Vector3.up * localPosition.y + 
                                      forward * localPosition.z;
                
                return formationOriginPosition + worldOffset;
            }
            else
            {
                // Original code for following behavior
                Vector3 forward = playerTransform.forward;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                
                Vector3 worldOffset = right * localPosition.x + 
                                      Vector3.up * localPosition.y + 
                                      forward * localPosition.z;
                
                return playerTransform.position + worldOffset;
            }
        }
        
        // Default - return zero to use existing behavior
        return Vector3.zero;
    }
    
    #region Formation Position Generators
    
    private Vector3 GetTrianglePosition(int npcIndex, int totalNPCs)
    {
        // Calculate how many rows we need based on total NPCs
        int rowCount = Mathf.CeilToInt(Mathf.Sqrt(totalNPCs * 2));
        
        // Start with a perfect triangle
        int currentRow = 0;
        int positionInRow = 0;
        int countInCurrentRow = 1;
        
        // Find which row this NPC belongs to
        int npcCounter = 0;
        for (int row = 0; row < rowCount; row++)
        {
            countInCurrentRow = row + 1; // Each row has (row+1) NPCs
            
            if (npcIndex < npcCounter + countInCurrentRow)
            {
                currentRow = row;
                positionInRow = npcIndex - npcCounter;
                break;
            }
            
            npcCounter += countInCurrentRow;
        }
        
        // Calculate position based on row and position in row
        float rowWidth = countInCurrentRow * formationSpacing;
        float xPos = positionInRow * formationSpacing - rowWidth / 2 + formationSpacing / 2;
        float zPos = -currentRow * formationSpacing * 0.866f; // 0.866 is approximately sin(60°)
        
        return new Vector3(xPos, 0, zPos);
    }
    
    private Vector3 GetRectanglePosition(int npcIndex, int totalNPCs)
    {
        // Calculate optimal rectangle dimensions
        int cols = Mathf.CeilToInt(Mathf.Sqrt(totalNPCs));
        int rows = Mathf.CeilToInt((float)totalNPCs / cols);
        
        // Calculate row and column for this NPC
        int row = npcIndex / cols;
        int col = npcIndex % cols;
        
        // Calculate width and height of the rectangle
        float width = (cols - 1) * formationSpacing;
        float height = (rows - 1) * formationSpacing;
        
        // Calculate position (centered)
        float xPos = col * formationSpacing - width / 2;
        float zPos = -row * formationSpacing;
        
        return new Vector3(xPos, 0, zPos);
    }
    
    private Vector3 GetCirclePosition(int npcIndex, int totalNPCs)
    {
        // Calculate angle for this NPC
        float angle = (npcIndex * Mathf.PI * 2f) / totalNPCs;
        
        // Calculate radius based on number of NPCs
        float radius = formationSpacing * Mathf.Sqrt(totalNPCs) / Mathf.PI;
        radius = Mathf.Max(radius, formationSpacing); // Ensure minimum radius
        
        // Calculate position on circle
        float xPos = Mathf.Sin(angle) * radius;
        float zPos = -Mathf.Cos(angle) * radius;
        
        return new Vector3(xPos, 0, zPos);
    }
    
    private Vector3 GetLinePosition(int npcIndex, int totalNPCs)
    {
        // Simple line formation perpendicular to player's forward direction
        float width = (totalNPCs - 1) * formationSpacing;
        float xPos = npcIndex * formationSpacing - width / 2;
        
        return new Vector3(xPos, 0, 0);
    }
    
    private Vector3 GetArrowPosition(int npcIndex, int totalNPCs)
    {
        if (totalNPCs <= 2)
        {
            // Not enough NPCs for arrow shape, use line instead
            return GetLinePosition(npcIndex, totalNPCs);
        }
        
        // Leader at the front
        if (npcIndex == 0)
        {
            return new Vector3(0, 0, formationSpacing);
        }
        
        // Calculate positions for the arrow wings
        int wingIndex = npcIndex - 1;
        int wingSize = totalNPCs - 1;
        int halfWing = wingSize / 2;
        
        if (wingIndex < halfWing)
        {
            // Left wing
            float offset = (wingIndex + 1) * formationSpacing;
            return new Vector3(-offset, 0, -offset);
        }
        else
        {
            // Right wing
            float offset = (wingIndex - halfWing + 1) * formationSpacing;
            return new Vector3(offset, 0, -offset);
        }
    }
    
    #endregion
}