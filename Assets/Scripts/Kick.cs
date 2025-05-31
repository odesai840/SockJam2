using UnityEngine;

public class Kick : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.GetComponent<Enemy>() != null)
                other.GetComponent<Enemy>().TakeDamage(1);
            else if (other.GetComponent<LavaEnemy>() != null)
                other.GetComponent<LavaEnemy>().TakeDamage(1);
        }
    }
}
