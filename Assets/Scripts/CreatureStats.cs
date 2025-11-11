using UnityEngine;

[System.Serializable]
public class CreatureStats
{
    [Header("Generation")]
    public int seed = 0;
    [Tooltip("Generate random seed when Generate from Seed button is pressed")]
    public bool randomizeSeed = true;

    [Header("Body Shape")]
    [Range(0, 4)]
    [Tooltip("0=Oval, 1=Rectangle, 2=Triangle, 3=Blob, 4=Segmented")]
    public int bodyShape = 0;
    
    [Range(0.5f, 3f)]
    public float bodySize = 1f;
    
    [Range(0.5f, 3f)]
    [Tooltip("Width multiplier for more extreme proportions")]
    public float bodyWidthRatio = 1f;
    
    [Range(0.5f, 3f)]
    [Tooltip("Height multiplier for more extreme proportions")]
    public float bodyHeightRatio = 1f;
    
    [Range(0f, 1f)]
    [Tooltip("How irregular/blobby the body shape is")]
    public float bodyIrregularity = 0.2f;

    [Header("Body Features")]
    [Range(0, 3)]
    [Tooltip("Number of humps/ridges on back")]
    public int bodyHumps = 0;
    
    [Range(0f, 0.5f)]
    [Tooltip("Size of humps relative to body")]
    public float humpSize = 0.2f;
    
    [Range(1, 5)]
    [Tooltip("Number of body segments (for segmented type)")]
    public int bodySegments = 3;

    [Header("Body Patterns")]
    [Range(0, 5)]
    [Tooltip("0=None, 1=Spots, 2=Stripes, 3=Gradient, 4=Scales, 5=Patches")]
    public int patternType = 0;
    
    [Range(0f, 1f)]
    [Tooltip("Intensity/visibility of pattern")]
    public float patternIntensity = 0.5f;
    
    [Range(0.5f, 3f)]
    [Tooltip("Scale/size of pattern elements")]
    public float patternScale = 1f;

    [Header("Surface Details")]
    [Range(0, 10)]
    [Tooltip("Number of spines/fins on body")]
    public int surfaceSpines = 0;
    
    [Range(0.1f, 0.5f)]
    [Tooltip("Size of surface details")]
    public float surfaceDetailSize = 0.2f;
    
    [Tooltip("Add armor plates to body")]
    public bool hasArmorPlates = false;
    
    [Range(2, 8)]
    [Tooltip("Number of armor plates")]
    public int armorPlateCount = 4;

    [Header("Head & Limbs")]
    [Range(0.3f, 0.7f)]
    public float headSizeRatio = 0.4f;
    
    [Range(0.5f, 2f)]
    public float legLength = 1f;
    
    [Range(0.3f, 1.5f)]
    public float legThickness = 0.6f;
    
    [Range(0.2f, 2f)]
    public float tailLength = 1f;
    
    [Range(0.2f, 1f)]
    public float tailThickness = 0.4f;
    
    [Range(0f, 0.5f)]
    public float neckLength = 0.2f;

    [Header("Variation")]
    [Range(0f, 0.3f)]
    [Tooltip("How much body parts can vary in proportion")]
    public float proportionVariation = 0.15f;

    [Header("Visual Style")]
    [Range(0f, 0.2f)]
    [Tooltip("Adds organic wobble to shapes")]
    public float shapeVariation = 0.05f;

    [Range(32, 256)]
    public int spriteResolution = 128;

    [Header("Colors (Generated from Seed)")]
    public Color primaryColor = Color.gray;
    public Color secondaryColor = Color.gray;
    public Color accentColor = Color.gray;
    public Color patternColor = Color.gray;

    public void RandomizeFromSeed()
    {
        Random.State oldState = Random.state;
        Random.InitState(seed);

        // Body shape and proportions - more conservative ranges
        bodyShape = Random.Range(0, 5);
        bodySize = Random.Range(0.8f, 2f);
        bodyWidthRatio = Random.Range(0.7f, 1.8f);
        bodyHeightRatio = Random.Range(0.7f, 1.8f);
        bodyIrregularity = Random.Range(0f, 0.4f);

        // Body features
        bodyHumps = Random.Range(0, 4);
        humpSize = Random.Range(0.15f, 0.3f);
        bodySegments = Random.Range(2, 5);

        // Patterns
        patternType = Random.Range(0, 6);
        patternIntensity = Random.Range(0.3f, 0.7f);
        patternScale = Random.Range(0.8f, 1.5f);

        // Surface details - reduced frequency
        float detailChance = Random.value;
        if (detailChance > 0.7f)
        {
            surfaceSpines = Random.Range(2, 6);
            surfaceDetailSize = Random.Range(0.2f, 0.3f);
        }
        else
        {
            surfaceSpines = 0;
        }

        hasArmorPlates = Random.value > 0.75f;
        armorPlateCount = Random.Range(3, 6);

        // Existing limb properties
        headSizeRatio = Random.Range(0.35f, 0.55f);
        legLength = Random.Range(0.7f, 1.5f);
        legThickness = Random.Range(0.4f, 1f);
        tailLength = Random.Range(0.4f, 1.6f);
        tailThickness = Random.Range(0.25f, 0.7f);
        neckLength = Random.Range(0f, 0.4f);
        proportionVariation = Random.Range(0.1f, 0.2f);
        
        // Color generation
        System.Random colorRng = new System.Random(seed);
        float colorScheme = (float)colorRng.NextDouble();
        
        if (colorScheme < 0.4f)
        {
            // Earth tones
            float baseHue = Random.Range(0.05f, 0.15f);
            float saturation = Random.Range(0.2f, 0.4f);
            float brightness = Random.Range(0.3f, 0.6f);
            
            primaryColor = Color.HSVToRGB(baseHue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(baseHue + 0.02f, saturation * 0.7f, brightness * 0.8f);
            accentColor = Color.HSVToRGB(baseHue - 0.02f, saturation * 1.2f, brightness * 1.1f);
            patternColor = Color.HSVToRGB(baseHue + 0.05f, saturation * 0.5f, brightness * 0.6f);
        }
        else if (colorScheme < 0.7f)
        {
            // Grayscale/muted
            float brightness = Random.Range(0.3f, 0.7f);
            float saturation = Random.Range(0.05f, 0.15f);
            float hue = Random.Range(0f, 1f);
            
            primaryColor = Color.HSVToRGB(hue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(hue, saturation * 0.8f, brightness * 0.7f);
            accentColor = Color.HSVToRGB(hue, saturation * 1.2f, brightness * 1.15f);
            patternColor = Color.HSVToRGB(hue, saturation * 0.6f, brightness * 0.5f);
        }
        else if (colorScheme < 0.85f)
        {
            // Warm tones
            float baseHue = Random.Range(0.0f, 0.08f);
            float saturation = Random.Range(0.3f, 0.5f);
            float brightness = Random.Range(0.4f, 0.65f);
            
            primaryColor = Color.HSVToRGB(baseHue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(baseHue + 0.03f, saturation * 0.6f, brightness * 0.5f);
            accentColor = Color.HSVToRGB(baseHue - 0.02f, saturation * 0.4f, brightness * 1.2f);
            patternColor = Color.HSVToRGB(baseHue + 0.08f, saturation * 1.2f, brightness * 0.8f);
        }
        else
        {
            // Vibrant colors
            float baseHue = Random.Range(0.15f, 0.6f);
            float saturation = Random.Range(0.25f, 0.45f);
            float brightness = Random.Range(0.35f, 0.6f);
            
            primaryColor = Color.HSVToRGB(baseHue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(baseHue + 0.05f, saturation * 0.8f, brightness * 0.75f);
            accentColor = Color.HSVToRGB(baseHue - 0.05f, saturation * 0.9f, brightness * 1.1f);
            patternColor = Color.HSVToRGB(baseHue + 0.15f, saturation * 1.1f, brightness * 0.7f);
        }

        Random.state = oldState;
    }
}