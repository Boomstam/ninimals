using UnityEngine;

[System.Serializable]
public class CreatureStats
{
    [Header("Generation")]
    public int seed = 0;
    [Tooltip("Generate random seed on start")]
    public bool randomizeSeed = true;

    [Header("Body Proportions")]
    [Range(0.5f, 3f)]
    public float bodySize = 1f;
    
    [Range(0.3f, 0.7f)]
    public float headSizeRatio = 0.4f; // Relative to body
    
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

        Random.state = oldState;
    }
}