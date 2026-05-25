using UnityEngine;
using System.Linq;

public class playerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f; // Snappier jump
    [SerializeField] private float sinkForce = 12f; // Fast descent
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool useRotation = false; 
    [SerializeField] private float flapAnimationSpeed = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    [Header("References")]
    public GameManager gameManager;
    public GameObject explosionPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        if (anim != null)
        {
            anim.speed = flapAnimationSpeed;
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
            
            if (anim != null)
            {
                anim.SetTrigger("Flap");
            }
        }
        else if (Input.GetKey(KeyCode.C))
        {
            rb.linearVelocity = new Vector2(horizontalSpeed, -sinkForce);
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

        if (gameManager != null)
        {
            gameManager.StopAllMovement(hitObject);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.FocusOn(transform.position);

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

        gameObject.SetActive(false);

        if (gameManager != null)
            gameManager.GameOver();
    }
}
