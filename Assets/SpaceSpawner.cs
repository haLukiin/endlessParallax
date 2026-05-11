using UnityEngine;

public class SpaceSpawner : MonoBehaviour
{
    public GameObject[] obstacles;

    public float spawnTime = 2f;

    public float minY = -3f;
    public float maxY = 3f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnTime);
    }

    void SpawnObstacle()
    {
        int randomIndex = Random.Range(0, obstacles.Length);

        Vector3 spawnPos = new Vector3(
            transform.position.x,
            Random.Range(minY, maxY),
            0f
        );

        Instantiate(obstacles[randomIndex], spawnPos, Quaternion.identity);
    }
}