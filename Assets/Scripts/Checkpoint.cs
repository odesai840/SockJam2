using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointNumber = 0;
    private Player player;

    void Start()
    {
        player = GameObject.Find("PlayerContainer").GetComponent<Player>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (this.checkpointNumber >= player.checkpoint.checkpointNumber)
                player.checkpoint = this;
        }
    }
}
