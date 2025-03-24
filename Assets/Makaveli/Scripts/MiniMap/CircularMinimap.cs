using UnityEngine;
using UnityEngine.UI;

public class CircularMinimap : MonoBehaviour
{
    [Header("Minimap Components")]
    public RawImage minimapImage;        // Your existing RawImage that displays the minimap

    void Start()
    {
        // Create the circular mask
        SetupCircularMask();
    }

    void SetupCircularMask()
    {
        // Get original minimap rect and size information
        RectTransform minimapRect = minimapImage.rectTransform;
        Vector2 originalSize = minimapRect.sizeDelta;
        Vector2 originalPosition = minimapRect.anchoredPosition;
        Vector2 originalAnchorMin = minimapRect.anchorMin;
        Vector2 originalAnchorMax = minimapRect.anchorMax;
        Vector2 originalPivot = minimapRect.pivot;
        
        // Get or create parent container
        Transform parent = minimapImage.transform.parent;
        GameObject maskObj;

        // Check if parent already has a mask component
        if (parent.GetComponent<Mask>() != null)
        {
            maskObj = parent.gameObject;
        }
        else
        {
            // Create a new GameObject to be the mask container
            maskObj = new GameObject("CircularMask");
            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            
            // Set the mask as a child of the minimap's parent
            maskObj.transform.SetParent(parent);
            
            // Copy exact position and size properties from original minimap
            maskRect.anchoredPosition = originalPosition;
            maskRect.sizeDelta = originalSize;
            maskRect.anchorMin = originalAnchorMin;
            maskRect.anchorMax = originalAnchorMax;
            maskRect.pivot = originalPivot;
            maskRect.localScale = minimapRect.localScale;
            
            // Add mask component
            Mask mask = maskObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;
            
            // Add image component for the mask
            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.sprite = CreateCircleSprite();
            
            // Move the minimap image to be a child of the mask
            minimapImage.transform.SetParent(maskRect);
            
            // Reset minimap position within the mask
            minimapRect.anchoredPosition = Vector2.zero;
            minimapRect.anchorMin = new Vector2(0, 0);
            minimapRect.anchorMax = new Vector2(1, 1);
            minimapRect.offsetMin = Vector2.zero;
            minimapRect.offsetMax = Vector2.zero;
            minimapRect.sizeDelta = Vector2.zero; // This makes it fill the parent completely
        }
    }

    Sprite CreateCircleSprite()
    {
        // Create a texture for our circle
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        // Center and radius
        Vector2 center = new Vector2(textureSize / 2, textureSize / 2);
        float radius = textureSize / 2;
        float radiusSq = radius * radius;
        
        // Create the circular image
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float distSq = dx * dx + dy * dy;
                
                // Inside circle = white, outside = transparent
                Color color = distSq <= radiusSq ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }
        
        texture.Apply();
        
        // Create sprite from texture with no border padding
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
    }
}