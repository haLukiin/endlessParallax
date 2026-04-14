using UnityEngine;

[DefaultExecutionOrder(20)]
public class Parallax : MonoBehaviour
{
    public float parallaxEffect;
    [Tooltip("Increase this value if you see small gaps between background pieces.")]
    public float overlap = 0.1f; 
    
    private Transform cam;
    private float length;
    private GameObject[] layers;

    void Start()
    {
        if (Camera.main == null) return;
        cam = Camera.main.transform;
        
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return;

        // Calculate width using pixel data to ignore transparency trimming
        Sprite s = renderers[0].sprite;
        length = (s.rect.width / s.pixelsPerUnit) * renderers[0].transform.localScale.x;

        layers = new GameObject[renderers.Length];
        System.Array.Sort(renderers, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        for (int i = 0; i < renderers.Length; i++)
        {
            layers[i] = renderers[i].gameObject;
            // Initial positioning with overlap
            float xPos = (i - 1) * (length - overlap); 
            layers[i].transform.localPosition = new Vector3(xPos, 0, 0);
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Move the parent based on parallax factor
        float dist = cam.position.x * parallaxEffect;
        transform.position = new Vector3(dist, transform.position.y, transform.position.z);

        float cameraLeftEdge = cam.position.x - (Camera.main.orthographicSize * Camera.main.aspect);

        foreach (GameObject layer in layers)
        {
            // Use the calculated length for edge detection
            float layerRightEdge = layer.transform.position.x + (length / 2f);

            if (layerRightEdge < cameraLeftEdge - 5f)
            {
                Vector3 newLocalPos = layer.transform.localPosition;
                // Move the piece to the front of the queue
                newLocalPos.x += (length - overlap) * layers.Length;
                layer.transform.localPosition = newLocalPos;
            }
            
            // Handle moving backwards
            float cameraRightEdge = cam.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
            float layerLeftEdge = layer.transform.position.x - (length / 2f);
            
            if (layerLeftEdge > cameraRightEdge + 5f)
            {
                Vector3 newLocalPos = layer.transform.localPosition;
                newLocalPos.x -= (length - overlap) * layers.Length;
                layer.transform.localPosition = newLocalPos;
            }
        }
    }
}
