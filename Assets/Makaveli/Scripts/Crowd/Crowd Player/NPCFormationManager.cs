using System.Collections.Generic;
using UnityEngine;

public enum FormationType
{
    Follow, // Default flocking behavior
    Triangle,
    Square,
    Circle,
    Line,
    Arrow,
    // Add more formations as needed
}

public class NPCFormationManager : MonoBehaviour
{
    [Header("Formation Settings")]
    public FormationType currentFormation = FormationType.Follow;
    public float formationSpacing = 1.5f; 
    public float minSpacing = 1.0f; // Minimum spacing between NPCs to prevent overlap
    public float transitionSpeed = 2.0f; 
    public float formationScale = 1.0f; 
    
    [Header("Location Boundaries")]
    public Vector3 locationSize = new Vector3(10f, 0f, 10f); // Size of the target location (x,z dimensions)
    
    [Header("References")]
    public Transform playerTransform; 
    public Transform targetLocation; 
    
    private Dictionary<FormationType, System.Func<int, int, Vector3>> formationGenerators;

    [Header("Behavior Settings")]
    public bool staticFormations = true; 
    private Vector3 formationOriginPosition; 
    private Quaternion formationOriginRotation; 
    private bool useCustomLocation = false;
    
    // Debug visualization
    public bool showDebugVisualization = true;
    public Color locationBoundsColor = Color.yellow;
    public Color formationBoundsColor = Color.green;
    
    private void Awake()
    {
        // Initialize the dictionary with formation generation functions
        formationGenerators = new Dictionary<FormationType, System.Func<int, int, Vector3>>
        {
            { FormationType.Triangle, GetTrianglePosition },
            { FormationType.Square, GetRectanglePosition },
            { FormationType.Circle, GetCirclePosition },
            { FormationType.Line, GetLinePosition },
            { FormationType.Arrow, GetArrowPosition }
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
    
    public void SetTargetLocation(Transform location)
    {
        if (location != null)
        {
            targetLocation = location;
            useCustomLocation = true;
            
            // Get the location's bounds if it has a collider
            if (location.TryGetComponent<Collider>(out var locationCollider))
            {
                // Get actual size from the collider
                locationSize = locationCollider.bounds.size;
                Debug.Log($"Location size detected: {locationSize}");
            }
        }
        else
        {
            useCustomLocation = false;
        }
    }
    
    public void ChangeFormation(FormationType newFormation)
    {
        // If switching to any formation other than Follow, record the current position
        if (newFormation != FormationType.Follow && currentFormation == FormationType.Follow)
        {
            // Only update origin if not using custom location
            if (!useCustomLocation)
            {
                UpdateFormationOrigin();
            }
        }
        
        currentFormation = newFormation;
    }
    
    // Calculate all positions for a formation type to determine its bounds
    private Bounds CalculateFormationBounds(FormationType formation, int totalNPCs, float spacing)
    {
        // Create temporary list to hold all positions
        List<Vector3> positions = new List<Vector3>();
        
        // Store original spacing to restore it later
        float originalSpacing = formationSpacing;
        formationSpacing = spacing;
        
        // Generate all formation positions
        for (int i = 0; i < totalNPCs; i++)
        {
            if (formationGenerators.TryGetValue(formation, out var positionFunc))
            {
                positions.Add(positionFunc(i, totalNPCs));
            }
        }
        
        // Restore original spacing
        formationSpacing = originalSpacing;
        
        // Calculate bounds from all positions
        Bounds bounds = new Bounds();
        if (positions.Count > 0)
        {
            bounds = new Bounds(positions[0], Vector3.zero);
            foreach (Vector3 pos in positions)
            {
                bounds.Encapsulate(pos);
            }
        }
        
        return bounds;
    }
    
    // Calculate the optimal spacing to fit within the location boundaries
    private float CalculateOptimalSpacing(FormationType formation, int totalNPCs)
    {
        // Get available space (use only x and z components for ground formations)
        float availableWidth = Mathf.Max(locationSize.x, 1f);
        float availableDepth = Mathf.Max(locationSize.z, 1f);
        
        // Start with default spacing
        float testSpacing = formationSpacing;
        
        // Special case for arrow formation
        if (formation == FormationType.Arrow)
        {
            int wingSize = totalNPCs - 1;
            int halfWing = wingSize / 2;
            
            // For an arrow, the width is determined by the wing spread
            float requiredWidth = halfWing * 2 * testSpacing;
            // The depth is determined by the wing length
            float requiredDepth = halfWing * testSpacing;
            
            // Calculate scaling factor to fit within available space (with 10% margin)
            float xScale = (availableWidth * 0.9f) / requiredWidth;
            float zScale = (availableDepth * 0.9f) / requiredDepth;
            float scaleFactor = Mathf.Min(xScale, zScale);
            
            // Only scale down, not up
            if (scaleFactor < 1.0f)
            {
                testSpacing *= scaleFactor;
                testSpacing = Mathf.Max(testSpacing, minSpacing);
                
                Debug.Log($"Arrow formation resized. Original spacing: {formationSpacing}, " +
                        $"New spacing: {testSpacing}, Scale factor: {scaleFactor}");
            }
            
            return testSpacing;
        }
        
        // For other formations, use the original bounds calculation
        Bounds bounds = CalculateFormationBounds(formation, totalNPCs, testSpacing);
        float formationWidth = bounds.size.x;
        float formationDepth = bounds.size.z;
        
        // Check if formation fits within available space
        if (formationWidth > availableWidth * 0.9f || formationDepth > availableDepth * 0.9f)
        {
            // Calculate scaling factor to fit within available space (with 10% margin)
            float xScale = (availableWidth * 0.9f) / formationWidth;
            float zScale = (availableDepth * 0.9f) / formationDepth;
            float scaleFactor = Mathf.Min(xScale, zScale);
            
            // Apply scale to spacing
            testSpacing *= scaleFactor;
            
            // Ensure we respect minimum spacing
            testSpacing = Mathf.Max(testSpacing, minSpacing);

            Debug.Log($"Formation {formation} resized. Original spacing: {formationSpacing}, " +
                    $"New spacing: {testSpacing}, Scale factor: {scaleFactor}");
        }
        
        return testSpacing;
    }
    // private float CalculateOptimalSpacing(FormationType formation, int totalNPCs)
    // {
    //     // Get available space (use only x and z components for ground formations)
    //     float availableWidth = Mathf.Max(locationSize.x, 1f);
    //     float availableDepth = Mathf.Max(locationSize.z, 1f);
        
    //     // Start with default spacing
    //     float testSpacing = formationSpacing;
        
    //     // Calculate formation bounds at current spacing
    //     Bounds bounds = CalculateFormationBounds(formation, totalNPCs, testSpacing);
    //     float formationWidth = bounds.size.x;
    //     float formationDepth = bounds.size.z;
        
    //     // Check if formation fits within available space
    //     if (formationWidth > availableWidth * 0.9f || formationDepth > availableDepth * 0.9f)
    //     {
    //         // Calculate scaling factor to fit within available space (with 10% margin)
    //         float xScale = (availableWidth * 0.9f) / formationWidth;
    //         float zScale = (availableDepth * 0.9f) / formationDepth;
    //         float scaleFactor = Mathf.Min(xScale, zScale);
            
    //         // Apply scale to spacing
    //         testSpacing *= scaleFactor;
            
    //         // Ensure we respect minimum spacing
    //         testSpacing = Mathf.Max(testSpacing, minSpacing);

    //         Debug.Log($"Formation {formation} resized. Original spacing: {formationSpacing}, " +
    //                   $"New spacing: {testSpacing}, Scale factor: {scaleFactor}");
    //     }
        
    //     return testSpacing;
    // }
    
    // Get the target position for an NPC in the current formation
    public Vector3 GetFormationPosition(int npcIndex, int totalNPCs)
    {
        if (currentFormation == FormationType.Follow || totalNPCs <= 1)
        {
            // Return zero to indicate using default following behavior
            return Vector3.zero;
        }
        
        // Calculate optimal spacing for this formation and number of NPCs
        float optimalSpacing = CalculateOptimalSpacing(currentFormation, totalNPCs);
        
        // Store original spacing to restore it later
        float originalSpacing = formationSpacing;
        formationSpacing = optimalSpacing;
        
        // Get the formation position from the appropriate function
        Vector3 localPosition = Vector3.zero;
        if (formationGenerators.TryGetValue(currentFormation, out var positionFunc))
        {
            localPosition = positionFunc(npcIndex, totalNPCs);
        }
        
        // Restore original spacing
        formationSpacing = originalSpacing;
        
        // Apply formation scale
        localPosition *= formationScale;
        
        // Calculate formation center offset to ensure proper centering within location
        Vector3 formationCenterOffset = CalculateFormationCenterOffset(currentFormation, totalNPCs, optimalSpacing);
        
        // Apply the center offset to the local position
        localPosition -= formationCenterOffset;
        
        if (staticFormations && currentFormation != FormationType.Follow)
        {
            // Use custom location if available, otherwise use stored origin
            Vector3 basePosition;
            Quaternion baseRotation;
            
            if (useCustomLocation && targetLocation != null)
            {
                targetLocation.GetPositionAndRotation(out basePosition, out baseRotation);
            }
            else
            {
                basePosition = formationOriginPosition;
                baseRotation = formationOriginRotation;
            }
            
            // Calculate world position
            Vector3 right = Vector3.Cross(Vector3.up, baseRotation * Vector3.forward).normalized;
            Vector3 forward = baseRotation * Vector3.forward;
            
            Vector3 worldOffset = right * localPosition.x + 
                                Vector3.up * localPosition.y + 
                                forward * localPosition.z;
            
            return basePosition + worldOffset;
        }
        else
        {
            // Following behavior
            Vector3 forward = playerTransform.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            
            Vector3 worldOffset = right * localPosition.x + 
                                Vector3.up * localPosition.y + 
                                forward * localPosition.z;
            
            return playerTransform.position + worldOffset;
        }
    }

    // Add this new method to calculate the center offset for each formation
    private Vector3 CalculateFormationCenterOffset(FormationType formation, int totalNPCs, float spacing)
    {
        // Calculate the bounds of the formation with the given spacing
        Bounds bounds = CalculateFormationBounds(formation, totalNPCs, spacing);
        
        // Return the center of the bounds
        // This will help us center the formation properly
        return bounds.center;
    }
    // public Vector3 GetFormationPosition(int npcIndex, int totalNPCs)
    // {
    //     if (currentFormation == FormationType.Follow || totalNPCs <= 1)
    //     {
    //         // Return zero to indicate using default following behavior
    //         return Vector3.zero;
    //     }
        
    //     // Calculate optimal spacing for this formation and number of NPCs
    //     float optimalSpacing = CalculateOptimalSpacing(currentFormation, totalNPCs);
        
    //     // Store original spacing to restore it later
    //     float originalSpacing = formationSpacing;
    //     formationSpacing = optimalSpacing;
        
    //     // Get the formation position from the appropriate function
    //     Vector3 localPosition = Vector3.zero;
    //     if (formationGenerators.TryGetValue(currentFormation, out var positionFunc))
    //     {
    //         localPosition = positionFunc(npcIndex, totalNPCs);
    //     }
        
    //     // Restore original spacing
    //     formationSpacing = originalSpacing;
        
    //     // Apply formation scale
    //     localPosition *= formationScale;
        
    //     if (staticFormations && currentFormation != FormationType.Follow)
    //     {
    //         // Use custom location if available, otherwise use stored origin
    //         Vector3 basePosition;
    //         Quaternion baseRotation;
            
    //         if (useCustomLocation && targetLocation != null)
    //         {
    //             basePosition = targetLocation.position;
    //             baseRotation = targetLocation.rotation;
    //         }
    //         else
    //         {
    //             basePosition = formationOriginPosition;
    //             baseRotation = formationOriginRotation;
    //         }
            
    //         // Calculate world position
    //         Vector3 right = Vector3.Cross(Vector3.up, baseRotation * Vector3.forward).normalized;
    //         Vector3 forward = baseRotation * Vector3.forward;
            
    //         Vector3 worldOffset = right * localPosition.x + 
    //                              Vector3.up * localPosition.y + 
    //                              forward * localPosition.z;
            
    //         return basePosition + worldOffset;
    //     }
    //     else
    //     {
    //         // Following behavior
    //         Vector3 forward = playerTransform.forward;
    //         Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            
    //         Vector3 worldOffset = right * localPosition.x + 
    //                              Vector3.up * localPosition.y + 
    //                              forward * localPosition.z;
            
    //         return playerTransform.position + worldOffset;
    //     }
    // }
    
    #region Formation Position Generators
    
    private Vector3 GetTrianglePosition(int npcIndex, int totalNPCs)
    {
        // Calculate how many rows we need based on total NPCs
        int rowCount = Mathf.CeilToInt(Mathf.Sqrt(totalNPCs * 2));
        
        // Find which row this NPC belongs to
        int currentRow = 0;
        int positionInRow = 0;
        int countInCurrentRow = 1;
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
        
        // Position z-coordinate with the apex at z=0
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
        
        // Calculate position (centered on both axes)
        float xPos = col * formationSpacing - width / 2;
        float zPos = -row * formationSpacing + height / 2;
        
        return new Vector3(xPos, 0, zPos);
    }
    
    private Vector3 GetCirclePosition(int npcIndex, int totalNPCs)
    {
        // Calculate angle for this NPC
        float angle = (npcIndex * Mathf.PI * 2f) / totalNPCs;
        
        // Calculate radius based on number of NPCs
        float radius = formationSpacing * Mathf.Sqrt(totalNPCs) / Mathf.PI;
        radius = Mathf.Max(radius, formationSpacing); // Ensure minimum radius
        
        // Calculate position on circle (already centered at origin)
        float xPos = Mathf.Sin(angle) * radius;
        float zPos = -Mathf.Cos(angle) * radius;
        
        return new Vector3(xPos, 0, zPos);
    }
    
    private Vector3 GetLinePosition(int npcIndex, int totalNPCs)
    {
        // Simple line formation perpendicular to player's forward direction
        float width = (totalNPCs - 1) * formationSpacing;
        float xPos = npcIndex * formationSpacing - width / 2;
        
        return new Vector3(xPos, 0, 0); // Center on x-axis
    }
    
    private Vector3 GetArrowPosition(int npcIndex, int totalNPCs)
    {
        if (totalNPCs <= 2)
        {
            // Not enough NPCs for arrow shape, use line instead
            return GetLinePosition(npcIndex, totalNPCs);
        }
        
        // Calculate arrow dimensions
        int wingSize = totalNPCs - 1;
        int halfWing = wingSize / 4;
        float wingLength = halfWing * formationSpacing;
        
        // Leader at the front
        if (npcIndex == 0)
        {
            // Position leader at the front
            return new Vector3(0, 0, 0);
        }
        
        // Calculate positions for the arrow wings
        int wingIndex = npcIndex - 1;
        
        // Adjust positioning to ensure everything stays within bounds
        if (wingIndex < wingSize / 2 + wingSize % 2)
        {
            // Left wing
            float wingPosition = (wingIndex + 1) * formationSpacing;
            // Cap the wing position to maintain the formation integrity
            wingPosition = Mathf.Min(wingPosition, wingLength);
            
            float xOffset = -wingPosition;
            float zOffset = -wingPosition; // For 45-degree angle
            
            return new Vector3(xOffset, 0, zOffset);
        }
        else
        {
            // Right wing
            float wingPosition = (wingIndex - (wingSize / 2) - (wingSize % 2) + 1) * formationSpacing;
            // Cap the wing position to maintain the formation integrity
            wingPosition = Mathf.Min(wingPosition, wingLength);
            
            float xOffset = wingPosition;
            float zOffset = -wingPosition; // For 45-degree angle
            
            return new Vector3(xOffset, 0, zOffset);
        }
    }
    
    // Visualize formation bounds and location boundaries
    private void OnDrawGizmos()
    {
        if (!showDebugVisualization) return;
        
        if (currentFormation != FormationType.Follow && useCustomLocation && targetLocation != null)
        {
            // Draw location boundaries
            Gizmos.color = locationBoundsColor;
            Vector3 locationCenter = targetLocation.position;
            Vector3 boundsSize = new Vector3(locationSize.x, 0.1f, locationSize.z);
            Gizmos.DrawWireCube(locationCenter, boundsSize);
            
            // Draw formation boundaries
            int totalNPCs = 0;
            if (Application.isPlaying && MGameManager.instance != null && MGameManager.instance.allNPCs != null)
            {
                totalNPCs = MGameManager.instance.allNPCs.Count;
            }
            else
            {
                totalNPCs = 10; // Default for editor visualization
            }
            
            if (totalNPCs > 0)
            {
                // Calculate optimal spacing
                float optimalSpacing = CalculateOptimalSpacing(currentFormation, totalNPCs);
                
                // Calculate bounds with this spacing
                Bounds formationBounds = CalculateFormationBounds(currentFormation, totalNPCs, optimalSpacing);
                
                // Calculate formation center offset
                Vector3 centerOffset = formationBounds.center;
                
                // Transform bounds to world space
                Vector3 worldCenter = targetLocation.position;
                
                // Draw formation bounds
                Gizmos.color = formationBoundsColor;
                Matrix4x4 originalMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(
                    worldCenter,
                    targetLocation.rotation,
                    Vector3.one
                );
                
                Gizmos.DrawWireCube(Vector3.zero, formationBounds.size);
                Gizmos.matrix = originalMatrix;
                
                // Draw each formation position
                Gizmos.color = Color.blue;
                for (int i = 0; i < totalNPCs; i++)
                {
                    // This simulates what GetFormationPosition would return for this NPC
                    // so the spheres match the actual positions
                    
                    // Store original spacing
                    float originalSpacing = formationSpacing;
                    formationSpacing = optimalSpacing;
                    
                    // Generate position
                    Vector3 localPos = Vector3.zero;
                    if (formationGenerators.TryGetValue(currentFormation, out var positionFunc))
                    {
                        localPos = positionFunc(i, totalNPCs);
                    }
                    
                    // Apply center offset correction
                    localPos -= centerOffset;
                    
                    // Restore original spacing
                    formationSpacing = originalSpacing;
                    
                    // Apply formation scale
                    localPos *= formationScale;
                    
                    // Transform to world space
                    Vector3 right = Vector3.Cross(Vector3.up, targetLocation.rotation * Vector3.forward).normalized;
                    Vector3 forward = targetLocation.rotation * Vector3.forward;
                    
                    Vector3 worldOffset = right * localPos.x + 
                                        Vector3.up * localPos.y + 
                                        forward * localPos.z;
                    
                    Vector3 worldPos = targetLocation.position + worldOffset;
                    
                    Gizmos.DrawSphere(worldPos, 0.2f);
                }
            }
        }
    }
    
    #endregion
}