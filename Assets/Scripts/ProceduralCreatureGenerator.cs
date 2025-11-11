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
        
        Debug.Log($"[Body Creation] Initial dimensions: {torsoWidth}x{torsoHeight}");
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        
        torsoWidth *= widthVar;
        torsoHeight *= heightVar;
        
        Debug.Log($"[Body Creation] Final dimensions: {torsoWidth}x{torsoHeight}");
        Debug.Log($"[Body Creation] Body shape: {stats.bodyShape}, Color: {palette.primary}");
        
        Color torsoColor = VaryColor(palette.primary, 0.05f, 0.1f, 0.1f);
        
        // Create base body shape
        Sprite bodySprite = CreateBodyShape((int)torsoWidth, (int)torsoHeight, torsoColor);
        
        if (bodySprite == null)
        {
            Debug.LogError("[Body Creation] FAILED - bodySprite is null!");
        }
        else
        {
            Debug.Log($"[Body Creation] Sprite created successfully: {bodySprite.texture.width}x{bodySprite.texture.height}");
        }
        
        // Apply pattern if enabled
        if (stats.patternType > 0)
        {
            Debug.Log($"[Body Creation] Applying pattern type: {stats.patternType}");
            bodySprite = ProceduralSpriteGenerator.ApplyPattern(
                bodySprite, stats.patternType, palette.pattern, 
                stats.patternIntensity, stats.patternScale, rng);
        }
        
        sr.sprite = bodySprite;
        
        if (sr.sprite == null)
        {
            Debug.LogError("[Body Creation] CRITICAL - SpriteRenderer.sprite is null!");
        }
        else
        {
            Debug.Log($"[Body Creation] Body sprite assigned to renderer successfully");
            Debug.Log($"[Body Creation] Sprite bounds: {sr.sprite.bounds.size}");
            Debug.Log($"[Body Creation] Sprite rect: {sr.sprite.rect}");
            Debug.Log($"[Body Creation] Sprite pivot: {sr.sprite.pivot}");
            Debug.Log($"[Body Creation] SpriteRenderer enabled: {sr.enabled}");
            Debug.Log($"[Body Creation] GameObject active: {torso.activeSelf}");
            Debug.Log($"[Body Creation] Torso position: {torso.transform.position}");
            
            // Check if texture has any visible pixels
            Texture2D tex = bodySprite.texture;
            Color[] pixels = tex.GetPixels();
            int visiblePixels = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0.1f) visiblePixels++;
            }
            Debug.Log($"[Body Creation] Visible pixels in final sprite: {visiblePixels} / {pixels.Length}");
        }
        
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
        Debug.Log($"[CreateBodyShape] Creating shape type {stats.bodyShape} with dimensions {width}x{height}");
        
        Sprite result = null;
        
        switch (stats.bodyShape)
        {
            case 0: // Oval
                result = ProceduralSpriteGenerator.CreateEllipse(
                    width, height, color, stats.shapeVariation, rng);
                break;
            
            case 1: // Rectangle
                result = ProceduralSpriteGenerator.CreateRoundedRectangle(
                    width, height, color, 0.3f, stats.shapeVariation, rng);
                break;
            
            case 2: // Triangle
                result = ProceduralSpriteGenerator.CreateTriangle(
                    width, height, color, stats.shapeVariation, rng);
                break;
            
            case 3: // Blob
                result = ProceduralSpriteGenerator.CreateBlob(
                    width, height, color, stats.bodyIrregularity, rng);
                break;
            
            case 4: // Segmented
                result = CreateSegmentedBody(width, height, color);
                break;
            
            default:
                Debug.LogWarning($"[CreateBodyShape] Unknown body shape {stats.bodyShape}, defaulting to Ellipse");
                result = ProceduralSpriteGenerator.CreateEllipse(
                    width, height, color, stats.shapeVariation, rng);
                break;
        }
        
        if (result == null)
        {
            Debug.LogError($"[CreateBodyShape] Shape generation returned null for type {stats.bodyShape}");
        }
        
        return result;
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
        
        float baseHeadSize = stats.spriteResolution * stats.bodySize * stats.headSizeRatio;
        float sizeVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        baseHeadSize *= sizeVar;
        
        // Apply width and height ratios for more variation
        float headWidth = baseHeadSize * stats.headWidthRatio;
        float headHeight = baseHeadSize * stats.headHeightRatio;
        
        Color headColor = VaryColor(palette.primary, 0.08f, 0.15f, 0.1f);
        
        // Create head based on shape type
        Sprite headSprite = CreateHeadShape((int)headWidth, (int)headHeight, headColor);
        sr.sprite = headSprite;
        
        Vector3 attachPoint = new Vector3(
            torsoSize.x * 0.5f + stats.neckLength,
            torsoSize.y * 0.3f,
            0f
        );
        head.transform.localPosition = attachPoint;
        
        // Add facial features - pass the larger dimension as reference size
        float headSizeForFeatures = Mathf.Max(headWidth, headHeight);
        CreateFacialFeatures(head.transform, headSizeForFeatures, palette);
    }
    
    private Sprite CreateHeadShape(int width, int height, Color color)
    {
        Sprite result = null;
        
        switch (stats.headShape)
        {
            case 0: // Round (circular)
                int avgSize = (width + height) / 2;
                result = ProceduralSpriteGenerator.CreateEllipse(
                    avgSize, avgSize, color, stats.headIrregularity * 0.5f, rng);
                break;
            
            case 1: // Oval (elliptical)
                result = ProceduralSpriteGenerator.CreateEllipse(
                    width, height, color, stats.headIrregularity * 0.5f, rng);
                break;
            
            case 2: // Square (rounded rectangle)
                result = ProceduralSpriteGenerator.CreateRoundedRectangle(
                    width, height, color, 0.25f, stats.headIrregularity * 0.5f, rng);
                break;
            
            case 3: // Triangle (pointing up)
                result = ProceduralSpriteGenerator.CreateTriangle(
                    width, height, color, stats.headIrregularity * 0.5f, rng);
                break;
            
            case 4: // Blob (irregular organic)
                result = ProceduralSpriteGenerator.CreateBlob(
                    width, height, color, stats.headIrregularity, rng);
                break;
            
            case 5: // Elongated (very stretched oval)
                int elongatedWidth = (int)(width * 0.7f);
                int elongatedHeight = (int)(height * 1.3f);
                result = ProceduralSpriteGenerator.CreateEllipse(
                    elongatedWidth, elongatedHeight, color, stats.headIrregularity * 0.5f, rng);
                break;
            
            case 6: // Diamond (angular)
                result = ProceduralSpriteGenerator.CreateDiamond(
                    width, height, color, stats.headIrregularity * 0.5f, rng);
                break;
            
            case 7: // Hammerhead (wide top, narrow bottom)
                result = ProceduralSpriteGenerator.CreateHammerhead(
                    width, height, color, stats.headIrregularity * 0.5f, rng);
                break;
        }
        
        return result;
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
    
    private void CreateFacialFeatures(Transform headTransform, float headSize, ColorPalette palette)
    {
        float headSizeWorld = headSize / 100f;
        
        // Create eyes
        CreateEyes(headTransform, headSizeWorld, palette);
        
        // Create mouth
        if (stats.mouthType > 0)
        {
            CreateMouth(headTransform, headSizeWorld, palette);
        }
        
        // Create antennae
        if (stats.hasAntennae)
        {
            CreateAntennae(headTransform, headSizeWorld, palette);
        }
        
        // Create horns
        if (stats.hasHorns)
        {
            CreateHorns(headTransform, headSizeWorld, palette);
        }
    }
    
    private void CreateEyes(Transform headTransform, float headSize, ColorPalette palette)
    {
        float eyeSize = stats.spriteResolution * stats.eyeSize;
        float eyeSpacing = headSize * stats.eyeSpacing;
        
        // Adjust eye position based on head shape for better placement
        float eyeYOffset = headSize * 0.15f;
        
        switch (stats.headShape)
        {
            case 3: // Triangle - eyes lower and wider
                eyeYOffset = headSize * 0.05f;
                eyeSpacing *= 1.2f;
                break;
            case 4: // Blob - slightly higher
                eyeYOffset = headSize * 0.2f;
                break;
            case 6: // Diamond - centered
                eyeYOffset = 0f;
                break;
            case 7: // Hammerhead - eyes at top wider apart
                eyeYOffset = headSize * 0.3f;
                eyeSpacing *= 1.5f;
                break;
        }
        
        // Left eye
        CreateEye("LeftEye", headTransform, new Vector3(-eyeSpacing * 0.5f, eyeYOffset, 0f), 
            eyeSize, palette);
        
        // Right eye
        CreateEye("RightEye", headTransform, new Vector3(eyeSpacing * 0.5f, eyeYOffset, 0f), 
            eyeSize, palette);
    }
    
    private void CreateEye(string name, Transform parent, Vector3 position, float size, ColorPalette palette)
    {
        GameObject eye = new GameObject(name);
        eye.transform.SetParent(parent);
        eye.transform.localPosition = position;
        
        SpriteRenderer sr = eye.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        
        Color eyeColor = Color.white;
        Sprite eyeSprite = null;
        
        switch (stats.eyeType)
        {
            case 0: // Round
                eyeSprite = ProceduralSpriteGenerator.CreateCircle((int)size, eyeColor);
                break;
            case 1: // Oval
                eyeSprite = ProceduralSpriteGenerator.CreateEllipse(
                    (int)(size * 0.8f), (int)size, eyeColor, 0f, rng);
                break;
            case 2: // Slit (vertical)
                eyeSprite = ProceduralSpriteGenerator.CreateEllipse(
                    (int)(size * 0.3f), (int)size, eyeColor, 0f, rng);
                break;
            case 3: // Compound (hexagonal pattern)
                eyeSprite = ProceduralSpriteGenerator.CreateCircle((int)size, 
                    VaryColor(palette.accent, 0.1f, 0.2f, 0.2f));
                break;
        }
        
        sr.sprite = eyeSprite;
        
        // Create pupil
        GameObject pupil = new GameObject("Pupil");
        pupil.transform.SetParent(eye.transform);
        pupil.transform.localPosition = Vector3.zero;
        
        SpriteRenderer pupilSr = pupil.AddComponent<SpriteRenderer>();
        pupilSr.sortingOrder = 4;
        
        float pupilSize = size * 0.5f;
        Color pupilColor = Color.black;
        
        if (stats.eyeType == 2) // Slit pupil
        {
            pupilSr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                (int)(pupilSize * 0.2f), (int)(pupilSize * 1.2f), pupilColor, 0f, rng);
        }
        else
        {
            pupilSr.sprite = ProceduralSpriteGenerator.CreateCircle((int)pupilSize, pupilColor);
        }
    }
    
    private void CreateMouth(Transform headTransform, float headSize, ColorPalette palette)
    {
        GameObject mouth = new GameObject("Mouth");
        mouth.transform.SetParent(headTransform);
        
        // Adjust mouth position based on head shape
        float mouthYOffset = -headSize * 0.2f;
        
        switch (stats.headShape)
        {
            case 2: // Square - centered
                mouthYOffset = -headSize * 0.15f;
                break;
            case 3: // Triangle - very bottom
                mouthYOffset = -headSize * 0.35f;
                break;
            case 5: // Elongated - lower
                mouthYOffset = -headSize * 0.3f;
                break;
            case 7: // Hammerhead - at neck
                mouthYOffset = -headSize * 0.25f;
                break;
        }
        
        mouth.transform.localPosition = new Vector3(0f, mouthYOffset, 0f);
        
        SpriteRenderer sr = mouth.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        
        float mouthWidth = stats.spriteResolution * stats.mouthSize;
        float mouthHeight = mouthWidth * 0.4f;
        
        Color mouthColor = Color.black;
        
        switch (stats.mouthType)
        {
            case 1: // Simple oval mouth
                sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                    (int)mouthWidth, (int)mouthHeight, mouthColor, 0.1f, rng);
                break;
            case 2: // Mouth with visible teeth
                sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                    (int)mouthWidth, (int)mouthHeight, mouthColor, 0.15f, rng);
                CreateTeeth(mouth.transform, mouthWidth, mouthHeight);
                break;
        }
    }
    
    private void CreateTeeth(Transform mouthTransform, float mouthWidth, float mouthHeight)
    {
        int teethCount = 5;
        float teethSpacing = mouthWidth / (teethCount + 1);
        float toothWidth = mouthWidth * 0.08f;
        float toothHeight = mouthHeight * 0.6f;
        
        for (int i = 0; i < teethCount; i++)
        {
            GameObject tooth = new GameObject($"Tooth_{i}");
            tooth.transform.SetParent(mouthTransform);
            
            SpriteRenderer sr = tooth.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            
            float xPos = -mouthWidth * 0.4f + teethSpacing * (i + 1);
            tooth.transform.localPosition = new Vector3(xPos / 100f, 0f, 0f);
            
            sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
                (int)toothWidth, (int)toothHeight, Color.white, 0f, rng);
        }
    }
    
    private void CreateAntennae(Transform headTransform, float headSize, ColorPalette palette)
    {
        float antennaeLength = stats.spriteResolution * stats.antennaeLength * stats.bodySize * 0.3f;
        float antennaeThickness = stats.spriteResolution * 0.05f;
        
        Color antennaeColor = VaryColor(palette.accent, 0.1f, 0.15f, 0.15f);
        
        // Left antenna
        CreateAntenna("LeftAntenna", headTransform, 
            new Vector3(-headSize * 0.3f, headSize * 0.4f, 0f),
            antennaeLength, antennaeThickness, antennaeColor, -25f);
        
        // Right antenna
        CreateAntenna("RightAntenna", headTransform, 
            new Vector3(headSize * 0.3f, headSize * 0.4f, 0f),
            antennaeLength, antennaeThickness, antennaeColor, 25f);
    }
    
    private void CreateAntenna(string name, Transform parent, Vector3 position, 
        float length, float thickness, Color color, float angle)
    {
        GameObject antenna = new GameObject(name);
        antenna.transform.SetParent(parent);
        antenna.transform.localPosition = position;
        antenna.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        
        SpriteRenderer sr = antenna.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        
        sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
            (int)thickness, (int)length, color, 0.5f, 0.05f, rng);
        
        antenna.transform.localPosition += antenna.transform.up * (length / 100f) * 0.5f;
        
        // Add tip bulb
        GameObject tip = new GameObject("Tip");
        tip.transform.SetParent(antenna.transform);
        tip.transform.localPosition = new Vector3(0f, (length / 100f) * 0.5f, 0f);
        
        SpriteRenderer tipSr = tip.AddComponent<SpriteRenderer>();
        tipSr.sortingOrder = 3;
        
        float tipSize = thickness * 2f;
        tipSr.sprite = ProceduralSpriteGenerator.CreateCircle((int)tipSize, 
            VaryColor(color, 0.05f, 0.1f, 0.15f));
    }
    
    private void CreateHorns(Transform headTransform, float headSize, ColorPalette palette)
    {
        float hornSpacing = headSize * 1.2f / (stats.hornCount + 1);
        float hornSize = stats.spriteResolution * 0.15f * stats.bodySize;
        
        Color hornColor = VaryColor(palette.secondary, 0.1f, 0.15f, -0.2f);
        hornColor = Color.Lerp(hornColor, Color.black, 0.2f);
        
        for (int i = 0; i < stats.hornCount; i++)
        {
            GameObject horn = new GameObject($"Horn_{i}");
            horn.transform.SetParent(headTransform);
            
            SpriteRenderer sr = horn.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            
            float sizeVar = 0.8f + (float)rng.NextDouble() * 0.4f;
            float hornWidth = hornSize * 0.4f * sizeVar;
            float hornHeight = hornSize * sizeVar;
            
            sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
                (int)hornWidth, (int)hornHeight, hornColor, 0.05f, rng);
            
            float xPos = -headSize * 0.5f + hornSpacing * (i + 1);
            Vector3 position = new Vector3(xPos, headSize * 0.45f, 0f);
            horn.transform.localPosition = position;
            
            // Slight random tilt
            float randomRotation = ((float)rng.NextDouble() - 0.5f) * 20f;
            horn.transform.localRotation = Quaternion.Euler(0f, 0f, randomRotation);
        }
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