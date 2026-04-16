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

        float speedMult = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;
        float densityMult = GameManager.Instance != null ? GameManager.Instance.spawnDensityMultiplier : 1f;
        
        // Interval decreases as speed increases (to keep distance constant) 
        // AND as density increases (to actually shrink the distance)
        float currentInterval = (spawnInterval / speedMult) / densityMult;

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
        GameObject newObstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);

        // Automatically set the vertical gap
        if (GameManager.Instance != null)
        {
            float gap = GameManager.Instance.currentVerticalGap;
            SetObstacleGap(newObstacle.transform, gap);
        }
    }

    void SetObstacleGap(Transform parent, float gap)
    {
        Transform top = null;
        Transform bottom = null;

        // Try to find children by name
        foreach (Transform child in parent)
        {
            string name = child.name.ToLower();
            if (name.Contains("top") || name.Contains("övre") || name.Contains("upp")) top = child;
            else if (name.Contains("bottom") || name.Contains("undre") || name.Contains("ner")) bottom = child;
        }

        // Fallback: Use the first two children if names don't match
        if (top == null && parent.childCount > 0) top = parent.GetChild(0);
        if (bottom == null && parent.childCount > 1) bottom = parent.GetChild(1);

        if (top != null && bottom != null)
        {
            float baseOffset = GameManager.Instance.mountainBaseOffset;
            
            // We use the gap to push the objects further apart from their base offset.
            // If baseOffset is -12, and gap is 8, topY becomes -12 + 4 = -8.
            // As gap shrinks to 2, topY becomes -12 + 1 = -11. 
            // This moves the mountain further down (closing the gap) as intended.
            float topY = baseOffset + (gap / 2f);
            float bottomY = -baseOffset - (gap / 2f);

            top.localPosition = new Vector3(top.localPosition.x, topY, top.localPosition.z);
            bottom.localPosition = new Vector3(bottom.localPosition.x, bottomY, bottom.localPosition.z);
        }
    }
}
