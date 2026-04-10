using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    public GameObject[] backgrounds;
    public float speed = 2f;

    [Header("Seam Covers")]
    public GameObject monsterPrefab;
    public GameObject deathStarPrefab;

    private GameObject[] coverPrefabs;
    private int nextCoverIndex = 0;

    void Start()
    {
        coverPrefabs = new GameObject[] { monsterPrefab, deathStarPrefab };

        // Sortera bakgrunderna så vi vet ordningen (Vänster till Höger)
        System.Array.Sort(backgrounds, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // Tvinga ihop dem direkt vid start för att ta bort glapp från editorn
        for (int i = 1; i < backgrounds.Length; i++)
        {
            SnapToTarget(backgrounds[i-1], backgrounds[i]);
        }

        nextCoverIndex = 1;
    }

    void Update()
    {
        float currentMultiplier = GameManager.Instance != null ? GameManager.Instance.speedMultiplier : 1f;
        float dt = Time.deltaTime;
        
        // Flytta alla bakgrunder
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].transform.position += Vector3.left * speed * currentMultiplier * dt;
        }

        float cameraLeftEdge = Camera.main.transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            SpriteRenderer sr = backgrounds[i].GetComponent<SpriteRenderer>();
            float spriteWidth = sr.bounds.size.x;
            float rightEdge = backgrounds[i].transform.position.x + (spriteWidth / 2f);

            // Om biten åkt utanför skärmen till vänster
            if (rightEdge < cameraLeftEdge)
            {
                // Hitta den bit som INTE är den vi just nu flyttar (för 2-bilders setup)
                // Eller hitta den som är längst till höger (för 3+ bilders setup)
                GameObject rightmost = backgrounds[i];
                float maxX = -float.MaxValue;

                foreach (GameObject bg in backgrounds)
                {
                    if (bg != backgrounds[i]) // Vi vill inte jämföra med oss själva
                    {
                        if (bg.transform.position.x > maxX)
                        {
                            maxX = bg.transform.position.x;
                            rightmost = bg;
                        }
                    }
                }

                // Utför "snapping" till den högra bilden
                SnapToTarget(rightmost, backgrounds[i]);

                // Placera en cover (monster/death star) vid skarven
                float seamX = rightmost.transform.position.x + (rightmost.GetComponent<SpriteRenderer>().bounds.size.x / 2f);
                PositionCoverAtSeam(seamX);
            }
        }
    }

    // Hjälpmetod för att sätta en bild exakt kant-i-kant med en annan
    void SnapToTarget(GameObject target, GameObject pieceToMove)
    {
        float widthA = target.GetComponent<SpriteRenderer>().bounds.size.x;
        float widthB = pieceToMove.GetComponent<SpriteRenderer>().bounds.size.x;
        
        // Beräkna nytt läge: Mitten på A + halva A:s bredd + halva B:s bredd - liten överlappning
        float newX = target.transform.position.x + (widthA / 2f) + (widthB / 2f) - 0.1f;
        pieceToMove.transform.position = new Vector3(newX, pieceToMove.transform.position.y, pieceToMove.transform.position.z);
    }

    void PositionCoverAtSeam(float xPos)
    {
        if (coverPrefabs == null || coverPrefabs.Length == 0) return;
        GameObject prefab = coverPrefabs[nextCoverIndex];
        if (prefab == null) return;

        float randomY = Random.Range(-1f, 1f);
        GameObject cover = Instantiate(prefab, new Vector3(xPos, randomY, -1f), Quaternion.identity);

        SpriteRenderer coverSR = cover.GetComponent<SpriteRenderer>();
        if (coverSR != null) coverSR.sortingOrder = 10;

        SeamCover sc = cover.GetComponent<SeamCover>();
        if (sc == null) sc = cover.AddComponent<SeamCover>();
        sc.speed = speed;

        nextCoverIndex = (nextCoverIndex + 1) % coverPrefabs.Length;
    }
}
