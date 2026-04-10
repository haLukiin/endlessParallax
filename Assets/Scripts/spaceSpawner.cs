using UnityEngine;

public class SpaceSpawner : MonoBehaviour
{
    public GameObject[] spaceObjects;

    public float minX = 18f;
    public float maxX = 22f;

    public float minY = -4f;
    public float maxY = 4f;

    public float minSpawnDelay = 1.5f;
    public float maxSpawnDelay = 4f;

    public float minScale = 0.7f;
    public float maxScale = 1.5f;

    int currentIndex = 0;
    private float timer = 0f;

    void Start()
    {
        timer = 2f;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsCountingDown) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnObject();
            
            float currentMultiplier = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay) / currentMultiplier;
            timer = delay;
        }
    }

    void SpawnObject()
    {
        if (spaceObjects.Length == 0) return;

        if (currentIndex >= spaceObjects.Length)
            currentIndex = 0;

        GameObject prefab = spaceObjects[currentIndex];

        float x = Random.Range(maxX - 1f, maxX);
        float segmentHeight = (maxY - minY) / spaceObjects.Length;
        float yMin = minY + (currentIndex * segmentHeight);
        float yMax = yMin + segmentHeight;
       
        float y = Random.Range(yMin, yMax);

        Vector3 spawnPos = new Vector3(x, y, 0);
        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);

        float scale = Random.Range(minScale, maxScale);
        obj.transform.localScale = Vector3.one * scale;

        currentIndex = (currentIndex + 1) % spaceObjects.Length;
    }
}
