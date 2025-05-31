using System.Collections;
using UnityEngine;

public class LavaBucketAttack : MonoBehaviour
{
    private GameObject player;
    public GameObject lavaBeam;
    private Vector3 directionToPlayer;
    private Vector3 startDirectionToPlayer;

    private GameObject lava;

    private Rigidbody2D rb;

    public float shakeTimer;
    [Tooltip("How long lava beam will last for")]
    public float attackTimer;
    [Tooltip("How much enemy/lava will shake")]
    public float shakingIntensity;

    private float rotationSpeed = 30f;

    AttackPhase phase;

    public enum AttackPhase
    {
        None,
        Shake = 1,
        Attack = 2,
        Fade = 3
    }

    private void Start()
    {
        phase = AttackPhase.None;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        directionToPlayer = (player.transform.position - rb.transform.position).normalized;

        if (phase == AttackPhase.Attack)
        {
            if (lava == null) return;

            // Direction to the player
            Vector3 direction = player.transform.position - rb.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;

            // Target rotation as a Quaternion
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            // Smoothly rotate towards the target
            lava.transform.rotation = Quaternion.RotateTowards(
                lava.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public void LavaAttack()
    {
        if (phase == AttackPhase.None)
        {
            phase = AttackPhase.Shake;

            StartCoroutine(ShakeTimer(shakeTimer));
        }
        else if (phase == AttackPhase.Attack)
        {
            //spawn lava here
            Vector2 lavaSpawnPoint = rb.transform.position;
            //lava = Instantiate(lavaBeam, lavaSpawnPoint, Quaternion.identity);

            StartCoroutine(AttackDelay(.05f));
            StartCoroutine(AttackTimer(attackTimer, lava));
        }


    }




    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }

    IEnumerator ShakeTimer(float shake)
    {
        StartCoroutine(Shake(shake, shakingIntensity));
        yield return new WaitForSeconds(shake);
        phase = AttackPhase.Attack;
        LavaAttack();
    }

    IEnumerator AttackTimer(float attackTimer, GameObject lava)
    {
        yield return new WaitForSeconds(attackTimer);
        Destroy(lava);
        phase = AttackPhase.None;
    }

    IEnumerator AttackDelay(float delay)
    {
        startDirectionToPlayer = (player.transform.position - rb.transform.position).normalized;

        yield return new WaitForSeconds(delay);

        Vector2 lavaSpawnPoint = rb.transform.position;

        // Base angle towards player
        float baseAngle = Mathf.Atan2(startDirectionToPlayer.y, startDirectionToPlayer.x) * Mathf.Rad2Deg;


        float offsetAngle = baseAngle + Random.Range(25f, 25f);


        Vector2 shootDirection = new Vector2(Mathf.Cos(offsetAngle * Mathf.Deg2Rad), Mathf.Sin(offsetAngle * Mathf.Deg2Rad));


        lava = Instantiate(lavaBeam, lavaSpawnPoint, Quaternion.Euler(0, 0, offsetAngle + 180));


        Rigidbody2D lavaRb = lava.GetComponent<Rigidbody2D>();
        if (lavaRb != null)
        {
            //lavaRb.linearVelocity = shootDirection * 5f; // Change 5f to your desired speed
        }

        StartCoroutine(AttackTimer(attackTimer, lava));
    }



}
