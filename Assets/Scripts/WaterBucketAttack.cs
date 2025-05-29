using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaterBucketAttack : MonoBehaviour 
{
    private GameObject player;
    private Rigidbody2D rb;

    AttackPhase phase;

    public float shakeTimer;
    [Tooltip("How long it will take to spill each section of the water")]
    public float spillTimer;
    [Tooltip("How long it will take for fully spilt water to dissapear")]
    public float fadeTimer;
    [Tooltip("How much enemy will shake")]
    public float shakingIntensity;

    public GameObject[] waterPrefabs;

    public enum AttackPhase
    {
        None,
        Shake = 1,
        Inside = 2,
        Middle = 3,
        Outside = 4,
        Fade = 5
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        phase = AttackPhase.None;

    }

    private void Update()
    {
        
        if (phase == AttackPhase.Shake)
        {
            //Run shake Code here
        }
        
    }

    
    public void SpillBucketAttack()
    {
        if (phase == AttackPhase.None)
        {
            phase = AttackPhase.Shake;
            StartCoroutine(ShakeTimer(shakeTimer));
        } 
        else if (phase == AttackPhase.Shake)
        {
            // idk nothing goes here i guess
        }
        else if (phase == AttackPhase.Inside)
        {

            Vector2 waterSpawnPoint = rb.transform.position;
            waterSpawnPoint.y -= .3f;
            GameObject water = Instantiate(waterPrefabs[0], waterSpawnPoint, Quaternion.identity);
            StartCoroutine(SpillPhaseTimer(spillTimer, water));
            
        }
        else if (phase == AttackPhase.Middle)
        {
            // Spawn Middle water
            Vector2 waterSpawnPoint = rb.transform.position;
            waterSpawnPoint.y -= .3f;
            GameObject water = Instantiate(waterPrefabs[1], waterSpawnPoint, Quaternion.identity);
            StartCoroutine(SpillPhaseTimer(spillTimer, water));

        }
        else if (phase == AttackPhase.Outside)
        {
            // Spawn Outside water
            Vector2 waterSpawnPoint = rb.transform.position;
            waterSpawnPoint.y -= .3f;
            GameObject water = Instantiate(waterPrefabs[2], waterSpawnPoint, Quaternion.identity);
            StartCoroutine(FadePhaseTimer(fadeTimer, water));
            
        }
        else if (phase == AttackPhase.Fade)
        {
            // Destroy water here
            // Optional: tell enemy script that attack is over
            phase = AttackPhase.None;
            
        }



    }

    //////////////////////////////////// Shake ///////////////////////////

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

    /////////////////////////////////////////////////////////////////////

    IEnumerator ShakeTimer(float shake)
    {
        StartCoroutine(Shake(shake, shakingIntensity));
        yield return new WaitForSeconds(shake);
        phase = AttackPhase.Inside;
        SpillBucketAttack();
    }
    IEnumerator SpillPhaseTimer(float spill, GameObject water)
    {
        yield return new WaitForSeconds(spill);
        Destroy(water);
        phase++;
        SpillBucketAttack();
    }
    IEnumerator FadePhaseTimer(float fade, GameObject water)
    {
        yield return new WaitForSeconds(fade);
        Destroy(water);
        phase = AttackPhase.Fade;
    }

}
