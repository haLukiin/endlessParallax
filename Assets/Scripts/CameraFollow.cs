using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(-3f, 0, 0); // Negative = Player on Right, Positive = Player on Left
    public float smoothSpeed = 5f;

    private bool isFollowingDeathPoint = false;
    private Vector3 deathTargetPosition;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (isFollowingDeathPoint)
        {
            // Focus on death point (keep smoothing here)
            Vector3 desiredPosition = new Vector3(deathTargetPosition.x, deathTargetPosition.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else if (target != null)
        {
            // Follow player EXACTLY on X axis to prevent parallax jitter
            // But we can still lerp the Y axis if you want it smooth vertically
            float targetX = target.position.x + offset.x;
            
            // We set X directly for perfect sync
            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        }
    }

    public void FocusOn(Vector3 position)
    {
        deathTargetPosition = position;
        isFollowingDeathPoint = true;
    }

    public void ParentToCamera(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.SetParent(transform);
        }
    }
}
