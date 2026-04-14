using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;
    public float heightRange = 2.5f;

    public float spawnOffset = 15f; // Distance from camera center
    private float timer = 0f;

    void Start()
    {
        timer = 1f;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsCountingDown) return;

        timer -= Time.deltaTime;

        float currentMultiplier = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;
        float currentInterval = spawnInterval / currentMultiplier;

        if (timer <= 0)
        {
            SpawnObstacle();
            timer = currentInterval;
        }
    }

    void SpawnObstacle()
    {
        float randomY = Random.Range(-heightRange, heightRange);
        float cameraX = Camera.main != null ? Camera.main.transform.position.x : transform.position.x;
        Vector3 spawnPos = new Vector3(cameraX + spawnOffset, randomY, 0);
        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }
}
