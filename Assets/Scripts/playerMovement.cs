using UnityEngine;
using System.Linq;

public class playerMovement : MonoBehaviour
{
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float rotationSpeed = 5f;

    private Rigidbody2D rb;
    private bool isDead = false;

    [Header("References")]
    public GameManager gameManager; // assign in Inspector
    public GameObject explosionPrefab; // Reference to the explosion prefab

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isDead)
        {
            Flymovement();
            RotatePlayer();
        }
    }

    void Flymovement()
    {
        if (gameManager == null) return;
        if (gameManager.IsCountingDown) return;

        if (Input.GetKeyDown(KeyCode.Space))
            rb.linearVelocity = Vector2.up * jumpForce;
    }

    void RotatePlayer()
    {
        float angle = rb.linearVelocity.y * rotationSpeed;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDead)
            Die(collision.transform);
    }

    void Die(Transform hitObject)
    {
        isDead = true;

        // 1. Tell GameManager to stop all movement and hide other obstacles
        if (gameManager != null)
        {
            gameManager.StopAllMovement(hitObject);
        }

        // 2. Instantiate explosion effect if assigned
        if (explosionPrefab != null)
        {
            // Spawn the explosion at the player's position. 
            // We DON'T parent it now because everything else has stopped.
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 3. Make Camera focus on the explosion/death point
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.FocusOn(transform.position);

            // Find all background objects and lock them to the camera
            // so we don't see the "empty void" behind them as we move
            MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var script in allScripts)
            {
                string name = script.GetType().Name;
                if (name.Contains("Background") || name.Contains("Scroll"))
                {
                    cam.ParentToCamera(script.gameObject);
                }
            }
        }

        // Hide the player
        gameObject.SetActive(false);

        // Tell GameManager to show Game Over after the delay
        if (gameManager != null)
            gameManager.GameOver();
    }
}