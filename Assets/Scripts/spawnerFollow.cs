using UnityEngine;

public class spawnerFollow : MonoBehaviour
{
    public Transform player;

    public float offsetX = 15f;

    void Update()
    {
        transform.position = new Vector3(
            player.position.x + offsetX,
            transform.position.y,
            transform.position.z
        );
    }
}