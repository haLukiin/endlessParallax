using UnityEngine;
using System.Linq;

public class playerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool useRotation = false; 
    [SerializeField] private float flapAnimationSpeed = 0.5f; // New variable to control animation speed

    private Rigidbody2D rb;
    private Animator anim; // Lägg till Animator
    private bool isDead = false;

    [Header("References")]
    public GameManager gameManager;
    public GameObject explosionPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Hämta Animator vid start
        
        if (anim != null)
        {
            anim.speed = flapAnimationSpeed; // Set the speed once at start
        }
    }

    void Update()
    {
        if (!isDead)
        {
            Flymovement();
            if (useRotation) RotatePlayer();
        }
    }

    void Flymovement()
    {
        if (gameManager == null) return;
        if (gameManager.IsCountingDown) return;

        float currentMultiplier = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;
        float horizontalSpeed = moveSpeed * currentMultiplier;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(horizontalSpeed, jumpForce);
            
            // Trigga ett enstaka vingslag
            if (anim != null)
            {
                anim.SetTrigger("Flap");
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
        }
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
                if (name.Contains("Background") || name.Contains("Scroll") || name.Contains("Parallax"))
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