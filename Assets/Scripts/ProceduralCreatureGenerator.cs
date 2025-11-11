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
                result = ProceduralSpriteGenerator.CreateEllipse(
                    width, height, color, stats.shapeVariation, rng);
                break;
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
        
        // Create 4 legs with proper z-ordering
        CreateLeg("FrontLeftLeg", torsoTransform, 
            new Vector3(torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, 1, palette);
            
        CreateLeg("FrontRightLeg", torsoTransform,
            new Vector3(torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, -1, palette);
        
        CreateLeg("BackLeftLeg", torsoTransform,
            new Vector3(-torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, 1, palette);
            
        CreateLeg("BackRightLeg", torsoTransform,
            new Vector3(-torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, -1, palette);
    }
    
    private void CreateLeg(string name, Transform parent, Vector3 attachPoint, 
        float width, float height, Color color, int sortingOrderMod, ColorPalette palette)
    {
        GameObject leg = new GameObject(name);
        leg.transform.SetParent(parent);
        leg.transform.localPosition = attachPoint;
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation * 0.5f;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation * 0.5f;
        
        width *= widthVar;
        height *= heightVar;
        
        // Create multi-segment leg based on type
        float segmentHeight = height / stats.legSegments;
        Vector3 currentPos = Vector3.zero;
        
        for (int i = 0; i < stats.legSegments; i++)
        {
            GameObject segment = new GameObject($"Segment_{i}");
            segment.transform.SetParent(leg.transform);
            segment.transform.localPosition = currentPos;
            
            SpriteRenderer sr = segment.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrderMod;
            
            // Calculate tapering - thicker at top, thinner at bottom
            float taperProgress = i / (float)stats.legSegments;
            float segmentWidth = width * (1f - stats.legTaper * taperProgress);
            
            Color segmentColor = VaryColor(color, 0.03f, 0.08f, 0.08f);
            
            // Create segment based on leg type
            switch (stats.legType)
            {
                case 0: // Simple - rounded rectangles
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)segmentWidth, (int)segmentHeight, 
                        segmentColor, 0.5f, stats.shapeVariation * 0.5f, rng);
                    break;
                
                case 1: // Segmented - distinct segments with joints
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)segmentWidth, (int)(segmentHeight * 0.9f), 
                        segmentColor, 0.4f, stats.shapeVariation * 0.3f, rng);
                    // Add visible joint
                    if (i < stats.legSegments - 1)
                    {
                        CreateLegJoint(segment.transform, segmentWidth * 1.2f, segmentColor, segmentHeight);
                    }
                    break;
                
                case 2: // Digitigrade - angled segments like dog/cat legs
                    float angleOffset = (i == 1) ? -15f : 0f; // Bend at middle segment
                    segment.transform.localRotation = Quaternion.Euler(0f, 0f, angleOffset);
                    sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                        (int)(segmentWidth * 0.9f), (int)segmentHeight, 
                        segmentColor, stats.shapeVariation * 0.4f, rng);
                    break;
                
                case 3: // Insectoid - thin, segmented with chitinous look
                    Color darkened = Color.Lerp(segmentColor, Color.black, 0.15f);
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)(segmentWidth * 0.8f), (int)(segmentHeight * 0.95f), 
                        darkened, 0.3f, 0f, rng);
                    // Add chitin lines
                    if (i < stats.legSegments - 1)
                    {
                        CreateChitinRidge(segment.transform, segmentWidth * 0.9f, darkened, segmentHeight);
                    }
                    break;
                
                case 4: // Tentacle - smooth, organic, tapered
                    float tentacleWidth = segmentWidth * Mathf.Lerp(1f, 0.4f, taperProgress);
                    sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                        (int)tentacleWidth, (int)segmentHeight, 
                        segmentColor, stats.shapeVariation * 0.8f, rng);
                    // Slight curve
                    float curve = Mathf.Sin(taperProgress * Mathf.PI) * 5f;
                    segment.transform.localRotation = Quaternion.Euler(0f, 0f, curve);
                    break;
                
                case 5: // Clawed - muscular with defined segments
                    float muscleBulge = 1f + Mathf.Sin(taperProgress * Mathf.PI) * 0.2f;
                    sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                        (int)(segmentWidth * muscleBulge), (int)segmentHeight, 
                        segmentColor, stats.shapeVariation * 0.6f, rng);
                    break;
                
                case 6: // Hoofed - sturdy, thick segments
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)(segmentWidth * 1.1f), (int)segmentHeight, 
                        segmentColor, 0.35f, stats.shapeVariation * 0.4f, rng);
                    break;
            }
            
            // Move down for next segment
            currentPos += new Vector3(0, -(segmentHeight / 100f), 0);
            
            // Add spikes to leg if enabled
            if (stats.hasLegSpikes && i < stats.legSegments - 1 && i < stats.legSpikeCount)
            {
                CreateLegSpike(segment.transform, segmentWidth, segmentHeight, palette);
            }
        }
        
        // Add foot/claw at the end
        if (stats.hasClaws || stats.legType == 5)
        {
            CreateLegClaw(leg.transform, currentPos, width, height, palette);
        }
    }
    
    private void CreateLegJoint(Transform segmentTransform, float size, Color color, float segmentHeight)
    {
        GameObject joint = new GameObject("Joint");
        joint.transform.SetParent(segmentTransform);
        joint.transform.localPosition = new Vector3(0f, -(segmentHeight / 100f) * 0.5f, 0f);
        
        SpriteRenderer sr = joint.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        
        Color jointColor = Color.Lerp(color, Color.black, 0.2f);
        sr.sprite = ProceduralSpriteGenerator.CreateCircle((int)size, jointColor);
    }
    
    private void CreateChitinRidge(Transform segmentTransform, float width, Color color, float segmentHeight)
    {
        GameObject ridge = new GameObject("ChitinRidge");
        ridge.transform.SetParent(segmentTransform);
        ridge.transform.localPosition = new Vector3(0f, -(segmentHeight / 100f) * 0.45f, 0f);
        
        SpriteRenderer sr = ridge.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        
        Color ridgeColor = Color.Lerp(color, Color.black, 0.3f);
        sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
            (int)(width * 0.95f), (int)(segmentHeight * 0.15f), ridgeColor, 0f, rng);
    }
    
    private void CreateLegSpike(Transform segmentTransform, float width, float height, ColorPalette palette)
    {
        GameObject spike = new GameObject("Spike");
        spike.transform.SetParent(segmentTransform);
        
        SpriteRenderer sr = spike.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        
        float spikeSize = stats.spriteResolution * stats.clawSize * 0.5f;
        float randomAngle = ((float)rng.NextDouble() - 0.5f) * 40f + 90f;
        
        spike.transform.localRotation = Quaternion.Euler(0f, 0f, randomAngle);
        spike.transform.localPosition = new Vector3(
            -(width / 100f) * 0.5f, 
            ((float)rng.NextDouble() - 0.5f) * (height / 100f), 
            0f
        );
        
        Color spikeColor = VaryColor(palette.accent, 0.1f, 0.15f, -0.2f);
        sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
            (int)(spikeSize * 0.4f), (int)spikeSize, spikeColor, 0f, rng);
    }
    
    private void CreateLegClaw(Transform legTransform, Vector3 position, float legWidth, float legHeight, ColorPalette palette)
    {
        GameObject claw = new GameObject("Claw");
        claw.transform.SetParent(legTransform);
        claw.transform.localPosition = position;
        
        SpriteRenderer sr = claw.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        
        float clawWidth = stats.spriteResolution * stats.clawSize * stats.bodySize;
        float clawHeight = clawWidth * 0.8f;
        
        Color clawColor = VaryColor(palette.accent, 0.08f, 0.12f, -0.25f);
        clawColor = Color.Lerp(clawColor, Color.black, 0.3f);
        
        // Different claw shapes based on leg type
        switch (stats.legType)
        {
            case 3: // Insectoid - pointed claw
                sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
                    (int)(clawWidth * 0.6f), (int)clawHeight, clawColor, 0.05f, rng);
                break;
            case 4: // Tentacle - sucker pad
                sr.sprite = ProceduralSpriteGenerator.CreateCircle((int)(clawWidth * 0.8f), clawColor);
                break;
            case 5: // Clawed - three-pronged claw
                CreateProngedClaw(claw.transform, clawWidth, clawHeight, clawColor);
                return;
            case 6: // Hoofed - flat hoof
                sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                    (int)(clawWidth * 1.2f), (int)(clawHeight * 0.6f), clawColor, 0.3f, 0f, rng);
                break;
            default: // Simple claw
                sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                    (int)(clawWidth * 0.8f), (int)(clawHeight * 0.7f), clawColor, 0.1f, rng);
                break;
        }
    }
    
    private void CreateProngedClaw(Transform clawTransform, float width, float height, Color color)
    {
        // Create 3 claw prongs
        for (int i = 0; i < 3; i++)
        {
            GameObject prong = new GameObject($"Prong_{i}");
            prong.transform.SetParent(clawTransform);
            
            SpriteRenderer sr = prong.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            
            float angle = -30f + (i * 30f);
            float xOffset = (i - 1) * (width / 100f) * 0.3f;
            
            prong.transform.localPosition = new Vector3(xOffset, 0f, 0f);
            prong.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            
            sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
                (int)(width * 0.35f), (int)(height * 0.9f), color, 0f, rng);
        }
    }
    
    private void CreateTail(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        GameObject tail = new GameObject("Tail");
        tail.transform.SetParent(torsoTransform);
        
        float baseWidth = stats.spriteResolution * 0.15f * stats.tailThickness * stats.bodySize;
        float totalLength = stats.spriteResolution * 0.5f * stats.tailLength * stats.bodySize;
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        float lengthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        
        baseWidth *= widthVar;
        totalLength *= lengthVar;
        
        Color tailColor = VaryColor(palette.accent, 0.1f, 0.15f, 0.15f);
        
        float segmentLength = totalLength / stats.tailSegments;
        Vector3 currentPos = Vector3.zero;
        Vector3 attachPoint = new Vector3(-torsoSize.x * 0.5f, 0f, 0f);
        
        tail.transform.localPosition = attachPoint;
        
        // Track for forked tail
        bool isFork = (stats.tailType == 5 && stats.tailSegments >= 3);
        int forkPoint = isFork ? stats.tailSegments - 2 : -1;
        
        for (int i = 0; i < stats.tailSegments; i++)
        {
            // Handle forked tail
            if (isFork && i == forkPoint)
            {
                CreateTailFork(tail.transform, currentPos, segmentLength, baseWidth, 
                    tailColor, palette, stats.tailSegments - forkPoint);
                break;
            }
            
            GameObject segment = new GameObject($"Segment_{i}");
            segment.transform.SetParent(tail.transform);
            segment.transform.localPosition = currentPos;
            
            SpriteRenderer sr = segment.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -2;
            
            // Calculate tapering and curving
            float taperProgress = i / (float)stats.tailSegments;
            float segmentWidth = baseWidth * (1f - stats.tailTaper * taperProgress);
            
            Color segmentColor = VaryColor(tailColor, 0.05f, 0.08f, 0.08f);
            
            // Create segment based on tail type
            switch (stats.tailType)
            {
                case 0: // Simple - smooth taper
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)segmentLength, (int)segmentWidth,
                        segmentColor, 0.6f, stats.shapeVariation, rng);
                    break;
                
                case 1: // Segmented - distinct visible segments
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)(segmentLength * 0.95f), (int)segmentWidth,
                        segmentColor, 0.5f, stats.shapeVariation * 0.5f, rng);
                    // Add segment line
                    if (i < stats.tailSegments - 1)
                    {
                        CreateTailSegmentLine(segment.transform, segmentWidth, segmentColor, segmentLength);
                    }
                    break;
                
                case 2: // Finned - fins along sides
                    sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                        (int)(segmentLength * 0.9f), (int)segmentWidth,
                        segmentColor, stats.shapeVariation * 0.6f, rng);
                    if (i > 0 && i % 2 == 0)
                    {
                        CreateTailSideFin(segment.transform, segmentWidth, segmentLength, palette);
                    }
                    break;
                
                case 3: // Spiked - spikes along top
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)(segmentLength * 0.95f), (int)segmentWidth,
                        segmentColor, 0.5f, stats.shapeVariation * 0.4f, rng);
                    break;
                
                case 4: // Club - thick segments ending in bulb
                    float clubMultiplier = (i >= stats.tailSegments - 2) ? 1.5f : 1f;
                    sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                        (int)(segmentLength * 0.95f), (int)(segmentWidth * clubMultiplier),
                        segmentColor, stats.shapeVariation * 0.5f, rng);
                    if (i == stats.tailSegments - 1)
                    {
                        CreateTailClub(segment.transform, segmentWidth * 2f, palette);
                    }
                    break;
                
                case 5: // Forked - handled above
                    sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                        (int)(segmentLength * 0.95f), (int)segmentWidth,
                        segmentColor, 0.6f, stats.shapeVariation, rng);
                    break;
                
                case 6: // Whip - very thin, flexible looking
                    float whipWidth = segmentWidth * Mathf.Lerp(1f, 0.2f, taperProgress);
                    sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
                        (int)(segmentLength * 0.85f), (int)whipWidth,
                        segmentColor, stats.shapeVariation * 0.9f, rng);
                    break;
            }
            
            // Add spikes if enabled
            if (stats.hasTailSpikes && i < stats.tailSpikeCount)
            {
                CreateTailSpike(segment.transform, segmentWidth, segmentLength, palette);
            }
            
            // Calculate curvature based on tail type and progress
            float curvatureAngle = 0f;
            
            switch (stats.tailType)
            {
                case 0: // Simple - gentle natural curve upward
                    curvatureAngle = -taperProgress * taperProgress * 15f; // Quadratic curve
                    break;
                
                case 1: // Segmented - slight upward curve
                    curvatureAngle = -taperProgress * 8f;
                    break;
                
                case 2: // Finned - swimming motion curve
                    curvatureAngle = Mathf.Sin(taperProgress * Mathf.PI) * -12f;
                    break;
                
                case 3: // Spiked - dramatic aggressive curve
                    curvatureAngle = -taperProgress * taperProgress * 20f;
                    break;
                
                case 4: // Club - curves down then up for weight
                    if (i < stats.tailSegments - 2)
                        curvatureAngle = taperProgress * 10f; // Down
                    else
                        curvatureAngle = -5f; // Up at end
                    break;
                
                case 5: // Forked - gentle S-curve
                    curvatureAngle = -taperProgress * 10f;
                    break;
                
                case 6: // Whip - dramatic sinusoidal wave
                    curvatureAngle = Mathf.Sin(taperProgress * Mathf.PI * 2f) * 15f * taperProgress;
                    break;
            }
            
            // Apply rotation for curvature
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, curvatureAngle);
            
            // Move along the curve direction for next segment
            float angleRad = (180f + curvatureAngle) * Mathf.Deg2Rad;
            Vector3 moveDirection = new Vector3(
                Mathf.Cos(angleRad) * (segmentLength / 100f),
                Mathf.Sin(angleRad) * (segmentLength / 100f),
                0f
            );
            currentPos += moveDirection;
        }
        
        // Add tail fin at end if enabled
        if (stats.hasTailFin && stats.tailType != 4 && stats.tailType != 5)
        {
            CreateTailEndFin(tail.transform, currentPos, baseWidth, palette);
        }
    }
    
    private void CreateTailSegmentLine(Transform segmentTransform, float width, Color color, float segmentLength)
    {
        GameObject line = new GameObject("SegmentLine");
        line.transform.SetParent(segmentTransform);
        line.transform.localPosition = new Vector3(-(segmentLength / 100f) * 0.5f, 0f, 0f);
        
        SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -1;
        
        Color lineColor = Color.Lerp(color, Color.black, 0.25f);
        sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
            (int)(segmentLength * 0.08f), (int)(width * 1.05f), lineColor, 0f, rng);
    }
    
    private void CreateTailSideFin(Transform segmentTransform, float width, float segmentLength, ColorPalette palette)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject fin = new GameObject($"SideFin_{(side > 0 ? "Right" : "Left")}");
            fin.transform.SetParent(segmentTransform);
            fin.transform.localPosition = new Vector3(0f, (width / 100f) * 0.6f * side, 0f);
            
            SpriteRenderer sr = fin.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -3;
            
            float finSize = width * stats.tailFinSize * 0.8f;
            Color finColor = VaryColor(palette.accent, 0.08f, 0.12f, 0.15f);
            finColor.a = 0.85f;
            
            sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
                (int)(finSize * 1.2f), (int)finSize, finColor, 0.1f, rng);
            fin.transform.localRotation = Quaternion.Euler(0f, 0f, side * 90f);
        }
    }
    
    private void CreateTailSpike(Transform segmentTransform, float width, float segmentLength, ColorPalette palette)
    {
        GameObject spike = new GameObject("Spike");
        spike.transform.SetParent(segmentTransform);
        
        SpriteRenderer sr = spike.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -1;
        
        float spikeSize = width * 0.8f;
        float xPos = ((float)rng.NextDouble() - 0.5f) * (segmentLength / 100f);
        
        spike.transform.localPosition = new Vector3(xPos, (width / 100f) * 0.6f, 0f);
        
        Color spikeColor = VaryColor(palette.accent, 0.1f, 0.15f, -0.2f);
        sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
            (int)(spikeSize * 0.5f), (int)spikeSize, spikeColor, 0.05f, rng);
    }
    
    private void CreateTailClub(Transform segmentTransform, float size, ColorPalette palette)
    {
        GameObject club = new GameObject("Club");
        club.transform.SetParent(segmentTransform);
        club.transform.localPosition = new Vector3(-(size / 100f) * 0.3f, 0f, 0f);
        
        SpriteRenderer sr = club.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -1;
        
        Color clubColor = VaryColor(palette.secondary, 0.08f, 0.12f, -0.15f);
        clubColor = Color.Lerp(clubColor, Color.black, 0.2f);
        
        sr.sprite = ProceduralSpriteGenerator.CreateEllipse(
            (int)(size * 1.2f), (int)size, clubColor, 0.15f, rng);
        
        // Add some studs/spikes on club
        int studCount = 4;
        for (int i = 0; i < studCount; i++)
        {
            GameObject stud = new GameObject($"Stud_{i}");
            stud.transform.SetParent(club.transform);
            
            SpriteRenderer studSr = stud.AddComponent<SpriteRenderer>();
            studSr.sortingOrder = 0;
            
            float angle = (i / (float)studCount) * 360f;
            float distance = (size / 100f) * 0.3f;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;
            
            stud.transform.localPosition = new Vector3(x, y, 0f);
            
            Color studColor = Color.Lerp(clubColor, Color.black, 0.3f);
            studSr.sprite = ProceduralSpriteGenerator.CreateCircle((int)(size * 0.2f), studColor);
        }
    }
    
    private void CreateTailEndFin(Transform tailTransform, Vector3 position, float tailWidth, ColorPalette palette)
    {
        GameObject fin = new GameObject("EndFin");
        fin.transform.SetParent(tailTransform);
        fin.transform.localPosition = position;
        
        SpriteRenderer sr = fin.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -3;
        
        float finWidth = tailWidth * 2f * stats.tailFinSize;
        float finHeight = tailWidth * 1.5f * stats.tailFinSize;
        
        Color finColor = VaryColor(palette.accent, 0.1f, 0.15f, 0.2f);
        finColor.a = 0.9f;
        
        sr.sprite = ProceduralSpriteGenerator.CreateTriangle(
            (int)finWidth, (int)finHeight, finColor, 0.15f, rng);
        fin.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
    }
    
    private void CreateTailFork(Transform tailTransform, Vector3 startPos, float segmentLength, 
        float baseWidth, Color tailColor, ColorPalette palette, int forkedSegments)
    {
        // Create two forked branches
        for (int fork = 0; fork < 2; fork++)
        {
            GameObject forkBranch = new GameObject($"Fork_{fork}");
            forkBranch.transform.SetParent(tailTransform);
            forkBranch.transform.localPosition = startPos;
            
            float forkAngle = (fork == 0) ? 20f : -20f;
            Vector3 currentPos = Vector3.zero;
            float accumulatedAngle = 0f;
            
            for (int i = 0; i < forkedSegments; i++)
            {
                GameObject segment = new GameObject($"Segment_{i}");
                segment.transform.SetParent(forkBranch.transform);
                segment.transform.localPosition = currentPos;
                
                SpriteRenderer sr = segment.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -2;
                
                float taperProgress = i / (float)forkedSegments;
                float segmentWidth = baseWidth * 0.7f * (1f - stats.tailTaper * taperProgress);
                
                Color segmentColor = VaryColor(tailColor, 0.05f, 0.08f, 0.08f);
                
                sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
                    (int)(segmentLength * 0.9f), (int)segmentWidth,
                    segmentColor, 0.6f, stats.shapeVariation * 0.7f, rng);
                
                // Add progressive curve to fork - curves outward more as it extends
                float segmentCurve = forkAngle * 0.4f * taperProgress;
                accumulatedAngle += segmentCurve;
                
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, segmentCurve);
                
                // Move along the curved direction
                float totalAngle = (180f + forkAngle + accumulatedAngle) * Mathf.Deg2Rad;
                currentPos += new Vector3(
                    Mathf.Cos(totalAngle) * (segmentLength / 100f),
                    Mathf.Sin(totalAngle) * (segmentLength / 100f),
                    0f
                );
            }
            
            // Add small fin at fork tips
            if (stats.hasTailFin)
            {
                GameObject tip = new GameObject("ForkTip");
                tip.transform.SetParent(forkBranch.transform);
                tip.transform.localPosition = currentPos;
                
                SpriteRenderer tipSr = tip.AddComponent<SpriteRenderer>();
                tipSr.sortingOrder = -2;
                
                Color tipColor = VaryColor(palette.accent, 0.1f, 0.15f, 0.15f);
                tipSr.sprite = ProceduralSpriteGenerator.CreateCircle((int)(baseWidth * 0.5f), tipColor);
            }
        }
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