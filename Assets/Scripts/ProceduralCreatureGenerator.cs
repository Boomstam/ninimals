using UnityEngine;
using System.Collections.Generic;

public class ProceduralCreatureGenerator : MonoBehaviour
{
    public CreatureStats stats = new CreatureStats();
    
    [SerializeField] private GameObject creatureRoot;
    
    private System.Random rng;

    [ContextMenu("Generate Creature")]
    public void GenerateCreature()
    {
        if (creatureRoot != null)
        {
            if (Application.isPlaying)
                Destroy(creatureRoot);
            else
                DestroyImmediate(creatureRoot);
        }
        
        if (stats.randomizeSeed)
        {
            stats.seed = Random.Range(0, 100000);
        }
        
        rng = new System.Random(stats.seed);
        
        stats.RandomizeFromSeed();
        
        creatureRoot = new GameObject("Creature");
        creatureRoot.transform.SetParent(transform);
        creatureRoot.transform.localPosition = Vector3.zero;
        
        ColorPalette palette = GenerateColorPalette();
        
        CreateBody(palette);
    }
    
    public void GenerateFromCurrentStats()
    {
        if (creatureRoot != null)
        {
            if (Application.isPlaying)
                Destroy(creatureRoot);
            else
                DestroyImmediate(creatureRoot);
        }
        
        rng = new System.Random(stats.seed);
        
        creatureRoot = new GameObject("Creature");
        creatureRoot.transform.SetParent(transform);
        creatureRoot.transform.localPosition = Vector3.zero;
        
        ColorPalette palette = GenerateColorPalette();
        
        CreateBody(palette);
    }
    
    private ColorPalette GenerateColorPalette()
    {
        return new ColorPalette 
        { 
            primary = stats.primaryColor, 
            secondary = stats.secondaryColor, 
            accent = stats.accentColor,
            pattern = stats.patternColor
        };
    }
    
    private void CreateBody(ColorPalette palette)
    {
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(creatureRoot.transform);
        torso.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = torso.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 0;
        
        float torsoWidth = stats.spriteResolution * stats.bodySize * stats.bodyWidthRatio;
        float torsoHeight = stats.spriteResolution * stats.bodySize * 0.7f * stats.bodyHeightRatio;
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        
        torsoWidth *= widthVar;
        torsoHeight *= heightVar;
        
        Color torsoColor = VaryColor(palette.primary, 0.05f, 0.1f, 0.1f);
        
        // Create base body shape
        Sprite bodySprite = CreateBodyShape((int)torsoWidth, (int)torsoHeight, torsoColor);
        
        // Apply pattern if enabled
        if (stats.patternType > 0)
        {
            bodySprite = ProceduralSpriteGenerator.ApplyPattern(
                bodySprite, stats.patternType, palette.pattern, 
                stats.patternIntensity, stats.patternScale, rng);
        }
        
        sr.sprite = bodySprite;
        
        Vector2 torsoSize = new Vector2(torsoWidth / 100f, torsoHeight / 100f);
        
        // Add body features
        if (stats.bodyHumps > 0 && stats.bodyShape != 4) // Don't add humps to segmented bodies
        {
            CreateBodyHumps(torso.transform, torsoSize, palette);
        }
        
        // Add surface details
        if (stats.surfaceSpines > 0)
        {
            CreateSurfaceSpines(torso.transform, torsoSize, palette);
        }
        
        if (stats.hasArmorPlates)
        {
            CreateArmorPlates(torso.transform, torsoSize, palette);
        }
        
        // Create limbs and appendages
        CreateHead(torso.transform, torsoSize, palette);
        CreateLegs(torso.transform, torsoSize, palette);
        CreateTail(torso.transform, torsoSize, palette);
    }

    private Sprite CreateBodyShape(int width, int height, Color color)
    {
        switch (stats.bodyShape)
        {
            case 0: // Oval
                return ProceduralSpriteGenerator.CreateEllipse(
                    width, height, color, stats.shapeVariation, rng);
            
            case 1: // Rectangle
                return ProceduralSpriteGenerator.CreateRoundedRectangle(
                    width, height, color, 0.3f, stats.shapeVariation, rng);
            
            case 2: // Triangle
                return ProceduralSpriteGenerator.CreateTriangle(
                    width, height, color, stats.shapeVariation, rng);
            
            case 3: // Blob
                return ProceduralSpriteGenerator.CreateBlob(
                    width, height, color, stats.bodyIrregularity, rng);
            
            case 4: // Segmented
                return CreateSegmentedBody(width, height, color);
            
            default:
                return ProceduralSpriteGenerator.CreateEllipse(
                    width, height, color, stats.shapeVariation, rng);
        }
    }

    private Sprite CreateSegmentedBody(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = new Color[width * height];
        
        int segments = stats.bodySegments;
        float segmentHeight = height / (float)segments;
        
        for (int y = 0; y < height; y++)
        {
            int segment = Mathf.FloorToInt(y / segmentHeight);
            float segmentProgress = (y % segmentHeight) / segmentHeight;
            
            // Segments bulge in middle, narrow at ends, but never go to zero width
            float widthMultiplier = 0.75f + Mathf.Sin(segmentProgress * Mathf.PI) * 0.25f;
            float currentWidth = width * widthMultiplier;
            
            for (int x = 0; x < width; x++)
            {
                float distFromCenter = Mathf.Abs(x - width / 2f);
                
                if (distFromCenter < currentWidth / 2f)
                {
                    float edgeDist = (currentWidth / 2f - distFromCenter) / (currentWidth / 2f);
                    float alpha = Mathf.Clamp01(edgeDist * 3f);
                    
                    // Subtle segment separation with darkening, not gaps
                    if (segmentProgress > 0.9f || segmentProgress < 0.1f)
                    {
                        alpha *= 0.85f;
                    }
                    
                    Color segmentColor = color;
                    if (segment % 2 == 1)
                    {
                        segmentColor = Color.Lerp(color, Color.black, 0.08f);
                    }
                    
                    pixels[y * width + x] = new Color(segmentColor.r, segmentColor.g, segmentColor.b, alpha);
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

    private void CreateBodyHumps(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        float humpSpacing = torsoSize.x / (stats.bodyHumps + 1);
        
        for (int i = 0; i < stats.bodyHumps; i++)
        {
            GameObject hump = new GameObject($"Hump_{i}");
            hump.transform.SetParent(torsoTransform);
            
            SpriteRenderer sr = hump.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            
            float humpSize = stats.spriteResolution * stats.humpSize * stats.bodySize;
            humpSize *= 1f + ((float)rng.NextDouble() - 0.5f) * 0.2f;
            
            Color humpColor = VaryColor(palette.secondary, 0.05f, 0.1f, 0.1f);
            sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                (int)humpSize, (int)(humpSize * 1.2f), humpColor, stats.shapeVariation * 0.5f, rng);
            
            // Position humps so they sit ON TOP of the body, overlapping naturally
            Vector3 position = new Vector3(
                -torsoSize.x * 0.5f + humpSpacing * (i + 1),
                torsoSize.y * 0.25f, // Lower so bottom of hump is on torso edge
                0f
            );
            hump.transform.localPosition = position;
        }
    }

    private void CreateSurfaceSpines(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        float spineSpacing = torsoSize.x / (stats.surfaceSpines + 1);
        
        for (int i = 0; i < stats.surfaceSpines; i++)
        {
            GameObject spine = new GameObject($"Spine_{i}");
            spine.transform.SetParent(torsoTransform);
            
            SpriteRenderer sr = spine.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            
            float spineWidth = stats.spriteResolution * stats.surfaceDetailSize * 0.25f;
            float spineHeight = stats.spriteResolution * stats.surfaceDetailSize * 0.8f;
            
            float sizeVar = 0.8f + (float)rng.NextDouble() * 0.4f;
            spineWidth *= sizeVar;
            spineHeight *= sizeVar;
            
            Color spineColor = VaryColor(palette.accent, 0.1f, 0.15f, 0.15f);
            sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
                (int)spineWidth, (int)spineHeight, spineColor, 0f, rng);
            
            // Position spine so base touches the top of the body
            Vector3 position = new Vector3(
                -torsoSize.x * 0.5f + spineSpacing * (i + 1),
                torsoSize.y * 0.3f, // Lower so base is at body surface
                0f
            );
            spine.transform.localPosition = position;
            
            // Small random tilt
            float randomRotation = ((float)rng.NextDouble() - 0.5f) * 15f;
            spine.transform.localRotation = Quaternion.Euler(0f, 0f, randomRotation);
        }
    }

    private void CreateArmorPlates(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        float plateSpacing = torsoSize.x / (stats.armorPlateCount + 1);
        
        for (int i = 0; i < stats.armorPlateCount; i++)
        {
            GameObject plate = new GameObject($"ArmorPlate_{i}");
            plate.transform.SetParent(torsoTransform);
            
            SpriteRenderer sr = plate.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            
            float plateSize = stats.spriteResolution * 0.18f * stats.bodySize;
            
            Color plateColor = VaryColor(palette.secondary, 0.05f, 0.1f, 0.1f);
            plateColor = Color.Lerp(plateColor, Color.black, 0.15f);
            
            sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                (int)plateSize, (int)(plateSize * 0.7f), plateColor, 0.25f, 0f, rng);
            
            // Position plates on the body surface with slight random offset
            float yOffset = ((float)rng.NextDouble() - 0.5f) * torsoSize.y * 0.3f;
            Vector3 position = new Vector3(
                -torsoSize.x * 0.5f + plateSpacing * (i + 1),
                yOffset,
                0f
            );
            plate.transform.localPosition = position;
            
            // Slight rotation for natural overlap look
            float randomRotation = ((float)rng.NextDouble() - 0.5f) * 20f;
            plate.transform.localRotation = Quaternion.Euler(0f, 0f, randomRotation);
        }
    }
    
    private void CreateHead(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        GameObject head = new GameObject("Head");
        head.transform.SetParent(torsoTransform);
        
        SpriteRenderer sr = head.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        
        float headSize = stats.spriteResolution * stats.bodySize * stats.headSizeRatio;
        float sizeVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        headSize *= sizeVar;
        
        Color headColor = VaryColor(palette.primary, 0.08f, 0.15f, 0.1f);
        sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
            (int)headSize, (int)(headSize * 1.1f), headColor, stats.shapeVariation, rng);
        
        Vector3 attachPoint = new Vector3(
            torsoSize.x * 0.5f + stats.neckLength,
            torsoSize.y * 0.3f,
            0f
        );
        head.transform.localPosition = attachPoint;
    }
    
    private void CreateLegs(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        float legWidth = stats.spriteResolution * 0.15f * stats.legThickness * stats.bodySize;
        float legHeight = stats.spriteResolution * 0.5f * stats.legLength * stats.bodySize;
        
        Color legColor = VaryColor(palette.secondary, 0.05f, 0.1f, 0.1f);
        
        CreateLeg("FrontLeftLeg", torsoTransform, 
            new Vector3(torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, 1);
            
        CreateLeg("FrontRightLeg", torsoTransform,
            new Vector3(torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, -1);
        
        CreateLeg("BackLeftLeg", torsoTransform,
            new Vector3(-torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, 1);
            
        CreateLeg("BackRightLeg", torsoTransform,
            new Vector3(-torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, -1);
    }
    
    private void CreateLeg(string name, Transform parent, Vector3 attachPoint, 
        float width, float height, Color color, int sortingOrderMod)
    {
        GameObject leg = new GameObject(name);
        leg.transform.SetParent(parent);
        leg.transform.localPosition = attachPoint;
        
        SpriteRenderer sr = leg.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrderMod;
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation * 0.5f;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation * 0.5f;
        
        Color legColorVaried = VaryColor(color, 0.05f, 0.1f, 0.1f);
        sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
            (int)(width * widthVar), (int)(height * heightVar), 
            legColorVaried, 0.5f, stats.shapeVariation * 0.5f, rng);
        
        leg.transform.localPosition += new Vector3(0, -(height * heightVar / 100f) * 0.5f, 0);
    }
    
    private void CreateTail(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        GameObject tail = new GameObject("Tail");
        tail.transform.SetParent(torsoTransform);
        
        SpriteRenderer sr = tail.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -2;
        
        float tailWidth = stats.spriteResolution * 0.4f * stats.tailLength * stats.bodySize;
        float tailHeight = stats.spriteResolution * 0.15f * stats.tailThickness * stats.bodySize;
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        
        Color tailColor = VaryColor(palette.accent, 0.1f, 0.15f, 0.15f);
        sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
            (int)(tailWidth * widthVar), (int)(tailHeight * heightVar),
            tailColor, 0.6f, stats.shapeVariation, rng);
        
        Vector3 attachPoint = new Vector3(
            -torsoSize.x * 0.5f - (tailWidth * widthVar / 100f) * 0.5f,
            0f,
            0f
        );
        tail.transform.localPosition = attachPoint;
    }
    
    private Color VaryColor(Color baseColor, float hueVar, float satVar, float valVar)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        
        h += ((float)rng.NextDouble() - 0.5f) * hueVar * 0.5f;
        s = Mathf.Clamp01(s + ((float)rng.NextDouble() - 0.5f) * satVar * 0.5f);
        v = Mathf.Clamp01(v + ((float)rng.NextDouble() - 0.5f) * valVar * 0.5f);
        
        return Color.HSVToRGB(h % 1f, s, v);
    }
    
    private struct ColorPalette
    {
        public Color primary;
        public Color secondary;
        public Color accent;
        public Color pattern;
    }
}