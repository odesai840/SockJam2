using System;
using System.Collections;
using UnityEngine;

public class WaterSpill : MonoBehaviour
{
    
    public int damage;

    public int damageDelay;

    private bool canDamage = true;

    Collider2D myCollider;

    private GameObject player;
    private Rigidbody2D rb;

    Player playerScript;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();

        //myCollider = GetComponent<Collider2D>();

        //playerScript = player.GetComponent<Player>();
    }
    private void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canDamage)
        {
            //Debug.Log("Trigger hit: " + other.name + ", Tag: " + other.tag);
            Player playerScript = other.GetComponentInParent<Player>();
            //if (playerScript != null)
            
                playerScript.TakeDamage(damage);
                Debug.Log("Doing Damage");
                canDamage = false;
                StartCoroutine(DamageTimer(damageDelay));
            
        }
    }

    IEnumerator DamageTimer(int damageDelay)
    {
        yield return new WaitForSeconds(damageDelay);
        canDamage = true;
    }
}
