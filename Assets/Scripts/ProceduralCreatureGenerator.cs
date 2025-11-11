using UnityEngine;

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
        
        CreateTorso(palette);
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
        
        CreateTorso(palette);
    }
    
    private ColorPalette GenerateColorPalette()
    {
        return new ColorPalette 
        { 
            primary = stats.primaryColor, 
            secondary = stats.secondaryColor, 
            accent = stats.accentColor 
        };
    }
    
    private void CreateTorso(ColorPalette palette)
    {
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(creatureRoot.transform);
        torso.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = torso.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 0;
        
        float torsoWidth = stats.spriteResolution * stats.bodySize;
        float torsoHeight = stats.spriteResolution * stats.bodySize * 0.7f;
        
        float widthVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        float heightVar = 1f + ((float)rng.NextDouble() - 0.5f) * stats.proportionVariation;
        
        torsoWidth *= widthVar;
        torsoHeight *= heightVar;
        
        Color torsoColor = VaryColor(palette.primary, 0.05f, 0.1f, 0.1f);
        sr.sprite = ProceduralSpriteGenerator.CreateRoundedRectangle(
            (int)torsoWidth, (int)torsoHeight, torsoColor, 0.3f, stats.shapeVariation, rng);
        
        Vector2 torsoSize = new Vector2(torsoWidth / 100f, torsoHeight / 100f);
        
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
    }
}