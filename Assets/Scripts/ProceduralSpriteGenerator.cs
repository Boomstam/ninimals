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

    public static Sprite CreateTriangle(int width, int height, Color color, float variation = 0f, System.Random rng = null)
    {
        Debug.Log($"[CreateTriangle] Creating triangle: {width}x{height}, variation: {variation}");
        
        if (width <= 0 || height <= 0)
        {
            Debug.LogError($"[CreateTriangle] Invalid dimensions: {width}x{height}");
            return null;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = new Color[width * height];
        
        float centerX = width / 2f;
        float topY = height * 0.9f;
        float bottomY = height * 0.1f;
        float baseWidth = width * 0.8f;
        
        float noiseOffsetX = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        float noiseOffsetY = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        
        int pixelsFilled = 0;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedY = (y - bottomY) / (topY - bottomY);
                normalizedY = Mathf.Clamp01(normalizedY);
                
                float widthAtY = baseWidth * (1f - normalizedY);
                float leftEdge = centerX - widthAtY / 2f;
                float rightEdge = centerX + widthAtY / 2f;
                
                if (variation > 0 && rng != null)
                {
                    float noiseValue = Mathf.PerlinNoise(noiseOffsetX + x * 0.02f, noiseOffsetY + y * 0.02f);
                    float offset = (noiseValue - 0.5f) * variation * width * 0.3f;
                    leftEdge += offset;
                    rightEdge += offset;
                }
                
                if (x >= leftEdge && x <= rightEdge && y >= bottomY && y <= topY)
                {
                    float distToEdge = Mathf.Min(x - leftEdge, rightEdge - x);
                    float alpha = Mathf.Clamp01(distToEdge / 2f);
                    pixels[y * width + x] = new Color(color.r, color.g, color.b, alpha);
                    if (alpha > 0.1f) pixelsFilled++;
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }
        
        Debug.Log($"[CreateTriangle] Filled {pixelsFilled} pixels out of {width * height}");
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite CreateBlob(int width, int height, Color color, float irregularity = 0.3f, System.Random rng = null)
    {
        Debug.Log($"[CreateBlob] Creating blob: {width}x{height}, irregularity: {irregularity}");
        
        if (width <= 0 || height <= 0)
        {
            Debug.LogError($"[CreateBlob] Invalid dimensions: {width}x{height}");
            return null;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = new Color[width * height];
        
        float centerX = width / 2f;
        float centerY = height / 2f;
        float radiusX = width / 2f * 0.8f;
        float radiusY = height / 2f * 0.8f;
        
        float noiseOffsetX = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        float noiseOffsetY = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        
        int pixelsFilled = 0;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - centerX);
                float dy = (y - centerY);
                float angle = Mathf.Atan2(dy, dx);
                
                float noiseValue = 1f;
                if (rng != null)
                {
                    noiseValue = Mathf.PerlinNoise(
                        noiseOffsetX + Mathf.Cos(angle * 3f + noiseOffsetX) * 2f,
                        noiseOffsetY + Mathf.Sin(angle * 3f + noiseOffsetY) * 2f
                    );
                    noiseValue = 0.7f + noiseValue * irregularity * 0.6f;
                }
                
                float adjustedRadiusX = radiusX * noiseValue;
                float adjustedRadiusY = radiusY * noiseValue;
                
                float normalizedDist = Mathf.Sqrt((dx / adjustedRadiusX) * (dx / adjustedRadiusX) + 
                                                   (dy / adjustedRadiusY) * (dy / adjustedRadiusY));
                
                if (normalizedDist <= 1f)
                {
                    float alpha = Mathf.Clamp01((1f - normalizedDist) * 2f);
                    pixels[y * width + x] = new Color(color.r, color.g, color.b, alpha);
                    if (alpha > 0.1f) pixelsFilled++;
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }
        
        Debug.Log($"[CreateBlob] Filled {pixelsFilled} pixels out of {width * height}");
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite ApplyPattern(Sprite baseSprite, int patternType, Color patternColor, 
        float intensity, float scale, System.Random rng)
    {
        Texture2D baseTexture = baseSprite.texture;
        Texture2D newTexture = new Texture2D(baseTexture.width, baseTexture.height);
        newTexture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = baseTexture.GetPixels();
        Color[] newPixels = new Color[pixels.Length];
        
        float noiseOffsetX = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        float noiseOffsetY = rng != null ? (float)rng.NextDouble() * 1000f : 0f;
        
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.1f)
            {
                newPixels[i] = pixels[i];
                continue;
            }
            
            int x = i % baseTexture.width;
            int y = i / baseTexture.width;
            
            float patternValue = 0f;
            
            switch (patternType)
            {
                case 1: // Spots
                    patternValue = GenerateSpots(x, y, scale, noiseOffsetX, noiseOffsetY, rng);
                    break;
                case 2: // Stripes
                    patternValue = GenerateStripes(x, y, scale, noiseOffsetX, noiseOffsetY);
                    break;
                case 3: // Gradient
                    patternValue = GenerateGradient(x, y, baseTexture.width, baseTexture.height);
                    break;
                case 4: // Scales
                    patternValue = GenerateScales(x, y, scale, noiseOffsetX, noiseOffsetY);
                    break;
                case 5: // Patches
                    patternValue = GeneratePatches(x, y, scale, noiseOffsetX, noiseOffsetY);
                    break;
            }
            
            patternValue = Mathf.Clamp01(patternValue * intensity);
            newPixels[i] = Color.Lerp(pixels[i], patternColor, patternValue);
        }
        
        newTexture.SetPixels(newPixels);
        newTexture.Apply();
        
        // Pivot should be normalized (0.5, 0.5 for center), NOT pixel coordinates
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        
        Debug.Log($"[ApplyPattern] Creating sprite: {newTexture.width}x{newTexture.height}, pivot: {pivot}");
        
        Sprite result = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), pivot, 100f);
        
        Debug.Log($"[ApplyPattern] Result sprite pivot: {result.pivot}");
        
        return result;
    }

    private static float GenerateSpots(int x, int y, float scale, float offsetX, float offsetY, System.Random rng)
    {
        float noiseValue = Mathf.PerlinNoise(offsetX + x * 0.05f / scale, offsetY + y * 0.05f / scale);
        return noiseValue > 0.6f ? 1f : 0f;
    }

    private static float GenerateStripes(int x, int y, float scale, float offsetX, float offsetY)
    {
        float angle = 0.3f; // Diagonal stripes
        float stripePos = (x * Mathf.Cos(angle) + y * Mathf.Sin(angle)) / (10f * scale);
        float noiseValue = Mathf.PerlinNoise(offsetX + stripePos, offsetY);
        return Mathf.Sin(stripePos * Mathf.PI * 2f) * 0.5f + 0.5f > noiseValue ? 1f : 0f;
    }

    private static float GenerateGradient(int x, int y, int width, int height)
    {
        float normalizedX = (float)x / width;
        float normalizedY = (float)y / height;
        return normalizedX * 0.5f + normalizedY * 0.5f;
    }

    private static float GenerateScales(int x, int y, float scale, float offsetX, float offsetY)
    {
        float cellSize = 15f * scale;
        float cellX = x / cellSize;
        float cellY = y / cellSize;
        
        float offset = (Mathf.Floor(cellY) % 2) * 0.5f;
        cellX += offset;
        
        float fx = cellX - Mathf.Floor(cellX) - 0.5f;
        float fy = cellY - Mathf.Floor(cellY) - 0.5f;
        
        float dist = Mathf.Sqrt(fx * fx + fy * fy);
        return dist < 0.4f ? 1f : 0f;
    }

    private static float GeneratePatches(int x, int y, float scale, float offsetX, float offsetY)
    {
        float noiseValue = Mathf.PerlinNoise(offsetX + x * 0.03f / scale, offsetY + y * 0.03f / scale);
        return noiseValue > 0.5f ? noiseValue : 0f;
    }

    public static Sprite CreateCircle(int size, Color color)
    {
        return CreateEllipse(size, size, color, 0f, null);
    }
}