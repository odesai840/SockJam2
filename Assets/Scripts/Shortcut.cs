using UnityEngine;

public class Shortcut : MonoBehaviour
{
    public BoxCollider2D blockage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            blockage.enabled = false;
            blockage.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            blockage.transform.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
    }
}
