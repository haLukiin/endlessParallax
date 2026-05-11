using UnityEngine;

public class DecorationSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] decorationPrefabs;
    [SerializeField] private GameObject grimmyPrefab;
    [SerializeField] private bool grimmyOnGround = true;
    [SerializeField] private float groundY = -4.5f;
    [SerializeField] private float grimmySpeed = 2f;

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
        GameObject prefab = null;
        bool isGrimmy = false;

        // 10% chance to spawn Grimmy if assigned
        if (grimmyPrefab != null && Random.value < 0.1f)
        {
            prefab = grimmyPrefab;
            isGrimmy = true;
        }
        else if (decorationPrefabs != null && decorationPrefabs.Length > 0)
        {
            prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
        }

        if (prefab == null) return;

        // Calculate spawn position relative to camera
        float cameraX = Camera.main != null ? Camera.main.transform.position.x : transform.position.x;
        float x = cameraX + Random.Range(minSpawnOffset, maxSpawnOffset);
        float y = isGrimmy && grimmyOnGround ? groundY : Random.Range(minY, maxY);

        Vector3 spawnPos = new Vector3(x, y, 0);
        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);

        // Ensure it has movement if it doesn't already
        if (obj.GetComponent<MoveLeft>() == null && obj.GetComponent<SeamCover>() == null)
        {
            MoveLeft move = obj.AddComponent<MoveLeft>();
            // Set speed: use grimmySpeed for Grimmy, 2f for others
            float speed = isGrimmy ? grimmySpeed : 2f;
            move.minSpeedX = speed; 
            move.maxSpeedX = speed;
        }

        // Apply random scale (wider range for Grimmy to make him look creepier at different distances)
        float currentMinScale = isGrimmy ? minScale * 0.8f : minScale;
        float currentMaxScale = isGrimmy ? maxScale * 1.5f : maxScale;
        float scale = Random.Range(currentMinScale, currentMaxScale);
        obj.transform.localScale = Vector3.one * scale;

        // Ensure it's in the background
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = sortingOrder;
            
            // If on ground, pivot usually needs to be at the bottom. 
            // Since we can't easily change pivot of a sprite at runtime without complex logic,
            // we'll just offset the Y slightly based on scale to keep feet on ground.
            if (isGrimmy && grimmyOnGround)
            {
                // Simple heuristic: adjust Y up by half the scale increase
                float yOffset = (scale - 1f) * 0.5f; 
                obj.transform.position += Vector3.up * yOffset;
            }
        }

        // Optional: If you want children to have the same sorting order
        foreach (SpriteRenderer childSr in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            childSr.sortingOrder = sortingOrder;
        }
    }
}
