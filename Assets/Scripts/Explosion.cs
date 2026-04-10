using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float destroyDelay = 1.5f;

    void Start()
    {
        // Destroy the explosion object after its animation/effect is done
        Destroy(gameObject, destroyDelay);
    }
}
