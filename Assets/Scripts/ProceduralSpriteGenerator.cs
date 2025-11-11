using UnityEngine;

public static class ProceduralSpriteGenerator
{
    public static Sprite CreateEllipse(int width, int height, Color color, float variation = 0f, System.Random rng = null)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = new Color[width * height];
        
        float centerX = width / 2f;
        float centerY = height / 2f;
        float radiusX = width / 2f * 0.9f;
        float radiusY = height / 2f * 0.9f;
        
        float noiseOffsetX = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        float noiseOffsetY = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - centerX) / radiusX;
                float dy = (y - centerY) / radiusY;
                
                float distVariation = 1f;
                if (variation > 0 && rng != null)
                {
                    float angle = Mathf.Atan2(dy, dx);
                    float noiseValue = Mathf.PerlinNoise(
                        noiseOffsetX + Mathf.Cos(angle * 6f) * 0.5f, 
                        noiseOffsetY + Mathf.Sin(angle * 6f) * 0.5f
                    );
                    distVariation = 1f + (noiseValue - 0.5f) * variation * 2f;
                }
                
                float distance = Mathf.Sqrt(dx * dx + dy * dy) / distVariation;
                
                if (distance <= 1f)
                {
                    float alpha = Mathf.Clamp01((1f - distance) * 3f);
                    pixels[y * width + x] = new Color(color.r, color.g, color.b, alpha);
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
    
    public static Sprite CreateRoundedRectangle(int width, int height, Color color, float roundness = 0.3f, float variation = 0f, System.Random rng = null)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = new Color[width * height];
        
        float centerX = width / 2f;
        float centerY = height / 2f;
        float halfWidth = width / 2f * 0.9f;
        float halfHeight = height / 2f * 0.9f;
        float cornerRadius = Mathf.Min(halfWidth, halfHeight) * roundness;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Abs(x - centerX);
                float dy = Mathf.Abs(y - centerY);
                
                float distance = 0f;
                
                if (dx > halfWidth - cornerRadius && dy > halfHeight - cornerRadius)
                {
                    float cornerX = halfWidth - cornerRadius;
                    float cornerY = halfHeight - cornerRadius;
                    distance = Mathf.Sqrt(Mathf.Pow(dx - cornerX, 2) + Mathf.Pow(dy - cornerY, 2));
                    distance = (distance - cornerRadius) / cornerRadius;
                }
                else if (dx <= halfWidth - cornerRadius || dy <= halfHeight - cornerRadius)
                {
                    distance = -1f;
                }
                else
                {
                    distance = Mathf.Max((dx - halfWidth) / cornerRadius, (dy - halfHeight) / cornerRadius);
                }
                
                if (distance <= 0f)
                {
                    pixels[y * width + x] = color;
                }
                else if (distance < 1f)
                {
                    float alpha = Mathf.Clamp01(1f - distance);
                    pixels[y * width + x] = new Color(color.r, color.g, color.b, alpha);
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
}