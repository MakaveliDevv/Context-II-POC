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
            maskObj.transform.SetParent(parent);
            
            // Move the minimap image to be a child of the mask
            minimapImage.transform.SetParent(maskObj.transform);
            
            // Position mask at the same position as the original image
            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchoredPosition = Vector2.zero;
            maskRect.sizeDelta = minimapImage.rectTransform.sizeDelta;
            maskRect.anchorMin = minimapImage.rectTransform.anchorMin;
            maskRect.anchorMax = minimapImage.rectTransform.anchorMax;
            maskRect.pivot = minimapImage.rectTransform.pivot;
            
            // Reset minimap image position within mask
            minimapImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        // Add mask component if it doesn't exist
        Mask mask = maskObj.GetComponent<Mask>();
        if (mask == null)
        {
            mask = maskObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;
        }

        // Add or get image component for the mask
        Image maskImage = maskObj.GetComponent<Image>();
        if (maskImage == null)
        {
            maskImage = maskObj.AddComponent<Image>();
        }

        // Create a circular sprite for the mask
        maskImage.sprite = CreateCircleSprite();
        
        // Ensure minimap image fills the mask area
        minimapImage.rectTransform.anchorMin = Vector2.zero;
        minimapImage.rectTransform.anchorMax = Vector2.one;
        minimapImage.rectTransform.offsetMin = Vector2.zero;
        minimapImage.rectTransform.offsetMax = Vector2.zero;
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
        
        // Create sprite from texture
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
    }
}