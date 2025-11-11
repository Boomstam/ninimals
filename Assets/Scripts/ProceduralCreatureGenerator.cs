using UnityEngine;

public class ProceduralCreatureGenerator : MonoBehaviour
{
    public CreatureStats stats = new CreatureStats();
    
    [Header("Runtime")]
    [SerializeField] private GameObject creatureRoot;
    
    private System.Random rng;

    private void Start()
    {
        if (stats.randomizeSeed)
        {
            stats.seed = Random.Range(0, 100000);
        }
        GenerateCreature();
    }

    [ContextMenu("Generate Creature")]
    public void GenerateCreature()
    {
        // Clean up existing creature
        if (creatureRoot != null)
        {
            if (Application.isPlaying)
                Destroy(creatureRoot);
            else
                DestroyImmediate(creatureRoot);
        }
        
        // Initialize RNG
        rng = new System.Random(stats.seed);
        
        // Apply seed-based randomization if desired
        stats.RandomizeFromSeed();
        
        // Create root
        creatureRoot = new GameObject("Creature");
        creatureRoot.transform.SetParent(transform);
        creatureRoot.transform.localPosition = Vector3.zero;
        
        // Generate color palette
        ColorPalette palette = GenerateColorPalette();
        
        // Create body parts
        CreateTorso(palette);
    }
    
    private ColorPalette GenerateColorPalette()
    {
        float baseHue = (float)rng.NextDouble();
        
        Color primary = Color.HSVToRGB(baseHue, 0.6f, 0.7f);
        Color secondary = Color.HSVToRGB((baseHue + 0.1f) % 1f, 0.5f, 0.65f);
        Color accent = Color.HSVToRGB((baseHue + 0.3f) % 1f, 0.7f, 0.75f);
        
        return new ColorPalette { primary = primary, secondary = secondary, accent = accent };
    }
    
    private void CreateTorso(ColorPalette palette)
    {
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(creatureRoot.transform);
        torso.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = torso.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 0;
        
        // Torso dimensions
        float torsoWidth = stats.spriteResolution * stats.bodySize;
        float torsoHeight = stats.spriteResolution * stats.bodySize * 0.7f;
        
        // Add slight variation
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        
        torsoWidth *= widthVar;
        torsoHeight *= heightVar;
        
        Color torsoColor = VaryColor(palette.primary, 0.05f, 0.1f, 0.1f);
        sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
            (int)torsoWidth, (int)torsoHeight, torsoColor, 0.3f, stats.shapeVariation, rng);
        
        // Store actual torso size for attachment calculations
        Vector2 torsoSize = new Vector2(torsoWidth / 100f, torsoHeight / 100f);
        
        // Create attached parts
        CreateHead(torso.transform, torsoSize, palette);
        CreateLegs(torso.transform, torsoSize, palette);
        CreateTail(torso.transform, torsoSize, palette);
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
        
        // Position: front-top of torso
        float headLocalSize = headSize / 100f;
        Vector3 attachPoint = new Vector3(
            torsoSize.x * 0.5f + stats.neckLength,
            torsoSize.y * 0.3f,
            0f
        );
        head.transform.localPosition = attachPoint;
    }
    
    private void CreateLegs(Transform torsoTransform, Vector2 torsoSize, ColorPalette palette)
    {
        // Leg dimensions
        float legWidth = stats.spriteResolution * 0.15f * stats.legThickness * stats.bodySize;
        float legHeight = stats.spriteResolution * 0.5f * stats.legLength * stats.bodySize;
        
        Color legColor = VaryColor(palette.secondary, 0.05f, 0.1f, 0.1f);
        
        // Front legs
        CreateLeg("FrontLeftLeg", torsoTransform, 
            new Vector3(torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, 1);
            
        CreateLeg("FrontRightLeg", torsoTransform,
            new Vector3(torsoSize.x * 0.25f, -torsoSize.y * 0.5f, 0f),
            legWidth, legHeight, legColor, -1);
        
        // Back legs
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
        sr.sortingOrder = sortingOrderMod; // 1 for front legs, -1 for back
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation * 0.5f;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation * 0.5f;
        
        Color legColorVaried = VaryColor(color, 0.05f, 0.1f, 0.1f);
        sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
            (int)(width * widthVar), (int)(height * heightVar), 
            legColorVaried, 0.5f, stats.shapeVariation * 0.5f, rng);
        
        // Adjust pivot so leg hangs down from attachment
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
        
        // Position: back-center of torso
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
        
        h += ((float)rng.NextDouble() - 0.5f) * hueVar;
        s = Mathf.Clamp01(s + ((float)rng.NextDouble() - 0.5f) * satVar);
        v = Mathf.Clamp01(v + ((float)rng.NextDouble() - 0.5f) * valVar);
        
        return Color.HSVToRGB(h % 1f, s, v);
    }
    
    private struct ColorPalette
    {
        public Color primary;
        public Color secondary;
        public Color accent;
    }
}