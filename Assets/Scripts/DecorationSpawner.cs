using UnityEngine;

public class DecorationSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] decorationPrefabs;

    [Header("Spawn Position")]
    public float minSpawnOffset = 18f;
    public float maxSpawnOffset = 25f;
    public float minY = -4f;
    public float maxY = 6f;

    [Header("Spawn Timing")]
    public float minSpawnDelay = 2f;
    public float maxSpawnDelay = 5f;

    [Header("Visuals")]
    public float minScale = 0.5f;
    public float maxScale = 1.2f;
    public int sortingOrder = -5; // Default behind most things

    private float timer = 0f;

    void Start()
    {
        timer = Random.Range(1f, 3f);
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsCountingDown)) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnDecoration();
            
            float speedMult = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;
            float densityMult = GameManager.Instance != null ? GameManager.Instance.spawnDensityMultiplier : 1f;
            
            // Scaled delay based on game speed
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay) / (speedMult * densityMult);
            timer = delay;
        }
    }

    void SpawnDecoration()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0) return;

        // Pick a random decoration
        GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];

        // Calculate spawn position relative to camera
        float cameraX = Camera.main != null ? Camera.main.transform.position.x : transform.position.x;
        float x = cameraX + Random.Range(minSpawnOffset, maxSpawnOffset);
        float y = Random.Range(minY, maxY);

        Vector3 spawnPos = new Vector3(x, y, 0);
        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);

        // Apply random scale
        float scale = Random.Range(minScale, maxScale);
        obj.transform.localScale = Vector3.one * scale;

        // Ensure it's in the background
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = sortingOrder;
        }

        // Optional: If you want children to have the same sorting order
        foreach (SpriteRenderer childSr in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            childSr.sortingOrder = sortingOrder;
        }
    }
}
