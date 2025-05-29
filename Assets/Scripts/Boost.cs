using UnityEngine;

public class Boost : MonoBehaviour
{
    private Player player;
    public float speedMultiplier = 5.0f;

    void Start()
    {
        player = GameObject.Find("PlayerContainer").GetComponent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.speedMultiplier = speedMultiplier;
        }
    }
}
