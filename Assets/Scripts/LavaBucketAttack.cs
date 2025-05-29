using System.Collections;
using UnityEngine;

public class LavaBucketAttack : MonoBehaviour
{
    private GameObject player;
    public GameObject lavaBeam;

    private Rigidbody2D rb;

    public float shakeTimer;
    [Tooltip("How long lava beam will last for")]
    public float attackTimer;
    [Tooltip("How much enemy/lava will shake")]
    public float shakingIntensity;

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
        if (phase == AttackPhase.Attack)
        {

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
            GameObject lava = Instantiate(lavaBeam, lavaSpawnPoint, Quaternion.identity);
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



}
