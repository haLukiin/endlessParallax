using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 5f;
    private bool isFollowing = false;
    private Vector3 targetPosition;
    private float originalZ;

    void Start()
    {
        originalZ = transform.position.x; // We use Z for 2D, but camera is at -10 usually
    }

    void LateUpdate()
    {
        if (isFollowing)
        {
            // Keep original Z so we don't move into the 2D plane
            Vector3 desiredPosition = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }

    public void FocusOn(Vector3 position)
    {
        targetPosition = position;
        isFollowing = true;
    }

    public void ParentToCamera(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.SetParent(transform);
        }
    }
}
