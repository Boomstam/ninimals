using UnityEngine;

[System.Serializable]
public class CreatureStats
{
    [Header("Generation")]
    public int seed = 0;
    [Tooltip("Generate random seed when Generate from Seed button is pressed")]
    public bool randomizeSeed = true;

    [Header("Body Proportions")]
    [Range(0.5f, 3f)]
    public float bodySize = 1f;
    
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

    public void RandomizeFromSeed()
    {
        Random.State oldState = Random.state;
        Random.InitState(seed);

        bodySize = Random.Range(0.7f, 2f);
        headSizeRatio = Random.Range(0.35f, 0.55f);
        legLength = Random.Range(0.7f, 1.5f);
        legThickness = Random.Range(0.4f, 1f);
        tailLength = Random.Range(0.4f, 1.6f);
        tailThickness = Random.Range(0.25f, 0.7f);
        neckLength = Random.Range(0f, 0.4f);
        proportionVariation = Random.Range(0.1f, 0.2f);
        
        System.Random colorRng = new System.Random(seed);
        float colorScheme = (float)colorRng.NextDouble();
        
        if (colorScheme < 0.4f)
        {
            float baseHue = Random.Range(0.05f, 0.15f);
            float saturation = Random.Range(0.2f, 0.4f);
            float brightness = Random.Range(0.3f, 0.6f);
            
            primaryColor = Color.HSVToRGB(baseHue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(baseHue + 0.02f, saturation * 0.7f, brightness * 0.8f);
            accentColor = Color.HSVToRGB(baseHue - 0.02f, saturation * 1.2f, brightness * 1.1f);
        }
        else if (colorScheme < 0.7f)
        {
            float brightness = Random.Range(0.3f, 0.7f);
            float saturation = Random.Range(0.05f, 0.15f);
            float hue = Random.Range(0f, 1f);
            
            primaryColor = Color.HSVToRGB(hue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(hue, saturation * 0.8f, brightness * 0.7f);
            accentColor = Color.HSVToRGB(hue, saturation * 1.2f, brightness * 1.15f);
        }
        else if (colorScheme < 0.85f)
        {
            float baseHue = Random.Range(0.0f, 0.08f);
            float saturation = Random.Range(0.3f, 0.5f);
            float brightness = Random.Range(0.4f, 0.65f);
            
            primaryColor = Color.HSVToRGB(baseHue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(baseHue + 0.03f, saturation * 0.6f, brightness * 0.5f);
            accentColor = Color.HSVToRGB(baseHue - 0.02f, saturation * 0.4f, brightness * 1.2f);
        }
        else
        {
            float baseHue = Random.Range(0.15f, 0.6f);
            float saturation = Random.Range(0.25f, 0.45f);
            float brightness = Random.Range(0.35f, 0.6f);
            
            primaryColor = Color.HSVToRGB(baseHue, saturation, brightness);
            secondaryColor = Color.HSVToRGB(baseHue + 0.05f, saturation * 0.8f, brightness * 0.75f);
            accentColor = Color.HSVToRGB(baseHue - 0.05f, saturation * 0.9f, brightness * 1.1f);
        }

        Random.state = oldState;
    }
}