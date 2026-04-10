using UnityEngine;

public class SeamCover : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        float leftEdge = Camera.main.transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect);
        if (transform.position.x < leftEdge - 8f)
            Destroy(gameObject);
    }
}
