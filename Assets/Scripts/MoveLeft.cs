using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    public float minSpeedX = 2f;
    public float maxSpeedX = 5f;

    public float minSpeedY = 1f;
    public float maxSpeedY = 3f;

    private float speedX;
    private float speedY;

    void Start()
    {
        // Random speeds for variation
        speedX = Random.Range(minSpeedX, maxSpeedX);
        speedY = Random.Range(minSpeedY, maxSpeedY);
    }

    void Update()
    {
        float currentMultiplier = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;

        // Move left AND down, scaled by global speed
        transform.position += new Vector3(-speedX, -speedY, 0) * currentMultiplier * Time.deltaTime;

        // --- Självförstörelse när objektet lämnat skärmen ---
        float cameraHeight = Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        float leftEdge = Camera.main.transform.position.x - cameraWidth;
        float bottomEdge = Camera.main.transform.position.y - cameraHeight;

        // Vi lägger på en marginal (5 enheter) så att den inte försvinner för tidigt
        if (transform.position.x < leftEdge - 5f || transform.position.y < bottomEdge - 5f)
        {
            Destroy(gameObject);
        }
    }
}
