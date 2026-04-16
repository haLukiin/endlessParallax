using UnityEngine;

public class ObstacleGap : MonoBehaviour
{
    [Header("Child References")]
    [Tooltip("The top part of the obstacle. If left empty, I will try to find a child with 'top' in its name.")]
    public Transform topObstacle;
    [Tooltip("The bottom part of the obstacle. If left empty, I will try to find a child with 'bottom' in its name.")]
    public Transform bottomObstacle;

    [Header("Debug")]
    public bool showDebugLogs = true;

    void Start()
    {
        InitializeChildren();
    }

    // Using Update instead of just Start allows you to see changes 
    // in the Inspector in real-time while the game is running.
    void Update()
    {
        ApplyGap();
    }

    void InitializeChildren()
    {
        // If not assigned, try to find children automatically
        if (topObstacle == null || bottomObstacle == null)
        {
            FindChildren();
        }

        if (topObstacle == null || bottomObstacle == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[ObstacleGap] Could not find both Top and Bottom obstacles on {gameObject.name}. Please assign them manually in the Inspector!", gameObject);
        }
    }

    void FindChildren()
    {
        foreach (Transform child in transform)
        {
            string name = child.name.ToLower();
            if (name.Contains("top") || name.Contains("övre") || name.Contains("upp"))
                topObstacle = child;
            else if (name.Contains("bottom") || name.Contains("undre") || name.Contains("ner"))
                bottomObstacle = child;
        }

        // Fallback: If still not found, take the first two children
        if (topObstacle == null && transform.childCount > 0)
            topObstacle = transform.GetChild(0);
        if (bottomObstacle == null && transform.childCount > 1)
            bottomObstacle = transform.GetChild(1);
    }

    void ApplyGap()
    {
        if (GameManager.Instance == null) return;
        if (topObstacle == null || bottomObstacle == null) return;

        float gap = GameManager.Instance.currentVerticalGap;

        // Position the children relative to the center of this parent object
        // Top goes up (gap / 2), Bottom goes down (-gap / 2)
        topObstacle.localPosition = new Vector3(topObstacle.localPosition.x, gap / 2f, topObstacle.localPosition.z);
        bottomObstacle.localPosition = new Vector3(bottomObstacle.localPosition.x, -gap / 2f, bottomObstacle.localPosition.z);
    }
}
