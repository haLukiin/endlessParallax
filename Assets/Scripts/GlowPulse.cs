using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlowPulse : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = new Color(0.5f, 0f, 1f, 1f); // Default purple
    public float baseIntensity = 1f;
    public float pulseAmount = 0.5f;
    public float pulseSpeed = 2f;
    public float outerRadius = 3f;

    private Light2D lightComponent;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Try to find an existing Light2D or add one
        lightComponent = GetComponentInChildren<Light2D>();
        
        if (lightComponent == null)
        {
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            lightComponent = lightObj.AddComponent<Light2D>();
        }

        // Initialize light settings
        lightComponent.lightType = Light2D.LightType.Point;
        lightComponent.color = glowColor;
        lightComponent.pointLightOuterRadius = outerRadius;
        lightComponent.intensity = baseIntensity;

        // Ensure the light affects the correct sorting layers
        if (spriteRenderer != null)
        {
            // This makes the light affect the layer the sprite is on.
            // In URP 2D, we often want to affect a range or specific layers.
            // By default, let's make it affect the sprite's layer.
            // If you want it to affect everything, you can set the mask in the inspector if you have a Light2D already.
        }
    }

    void Update()
    {
        if (lightComponent != null)
        {
            // Calculate pulse using sine wave
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            lightComponent.intensity = baseIntensity + pulse;
        }
    }
}
