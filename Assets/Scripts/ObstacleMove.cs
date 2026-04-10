using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float currentMultiplier = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;
        transform.position += Vector3.left * speed * currentMultiplier * Time.deltaTime;

        float leftEdge = Camera.main.transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect);
        if (transform.position.x < leftEdge - 5f)
        {
            Destroy(gameObject);
        }
    }
}