using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class MeteorFire : MonoBehaviour
{
    void Start()
    {
        SetupFire();
    }

    void SetupFire()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        
        // --- Main Settings ---
        main.startLifetime = 0.7f;
        main.startSpeed = 0f; 
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.simulationSpace = ParticleSystemSimulationSpace.World; 
        main.loop = true;

        // --- Inherit Velocity (Säkrad version) ---
        var inherit = ps.inheritVelocity;
        inherit.enabled = true;
        inherit.mode = ParticleSystemInheritVelocityMode.Initial;
        // Vi använder curve istället för multiplier för att undvika error:
        inherit.curve = new ParticleSystem.MinMaxCurve(1.0f); 

        // --- Emission ---
        var emission = ps.emission;
        emission.rateOverTime = 500; 

        // --- Shape ---
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 15f;

        // --- Velocity Over Lifetime ---
        // Justera dessa värden för att styra svansen snett uppåt vänster:
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = -8f; // Drar svansen åt vänster
        velocity.y = 16f;   // Drar svansen uppåt

        // --- Color over Lifetime ---
        var colorModule = ps.colorOverLifetime;
        colorModule.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0.2f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
                new GradientColorKey(new Color(0.8f, 0.1f, 0f), 0.8f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.7f, 0.6f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorModule.color = grad;

        // --- Size over Lifetime ---
        var sizeModule = ps.sizeOverLifetime;
        sizeModule.enabled = true;
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.2f));

        // --- Renderer (Med materialet tillbaka) ---
        var psRenderer = GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            psRenderer.sortingOrder = 5;
            
            // Försök hitta en snygg glödande shader
            Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            
            if (shader != null)
                psRenderer.material = new Material(shader);
        }
    }
}