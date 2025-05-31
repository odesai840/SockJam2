using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    private Camera mainCamera;
    private Transform player;
    private Animator animator;

    // movement
    [Header("Movement")]
    public Rigidbody2D rb;
    public float moveSpeed = 5.0f;
    private Vector2 inputVector;
    private Vector2 lastInputVector;
    private bool canMove = true;
    public float speedMultiplier = 1.0f;

    // camera
    [Header("Camera")]
    public float camZAxis = -100.0f;
    public float camSpeed = 4.0f;

    // health
    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;
    public RectTransform healthBar;

    // dash 
    [Header("Dash")]
    public float dashDistance = 50.0f;
    public float dashCooldown = 1.0f;
    public float dashDuration = 0.1f;
    public float dashShakeIntensity = 0.2f;
    public float dashShakeDuration = 0.1f;
    private bool canDash = true;
    
    // attacks
    [Header("Attacks")]
    [SerializeField] private GameObject rangedProjectile;

    // checkpoint
    [HideInInspector] public Checkpoint checkpoint;

    void Start()
    {
        canMove = true;
        canDash = true;

        currentHealth = maxHealth;

        mainCamera = Camera.main;
        player = GameObject.Find("Player").transform;
        animator = player.GetComponent<Animator>();

        // testing
        checkpoint = GameObject.Find("Checkpoint0").GetComponent<Checkpoint>();

        player.position = checkpoint.transform.position;
    }

    void Update()
    {
        // testing death and checkpoints
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(1);
            Debug.Log("Current Health: " + currentHealth);
        }

        if (currentHealth <= 0)
        {
            Death();
        }

        // movement
        inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        inputVector = Vector2.ClampMagnitude(inputVector, 1f);

        if (inputVector != Vector2.zero)
        {
            lastInputVector = inputVector;
        }

        // kick
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("KICK");
            animator.SetTrigger("kick");

            
        }
        
        // ranged attack
        if(Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 playerPos2D = new Vector2(player.position.x, player.position.y);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Vector2 direction2D = (mousePos2D - playerPos2D).normalized;
            Vector3 direction = new Vector3(direction2D.x, direction2D.y, 0);
            GameObject shuriken = Instantiate(rangedProjectile, player.position, Quaternion.identity);
            
            Shuriken shurikenScript = shuriken.GetComponent<Shuriken>();
            if (shurikenScript != null)
            {
                shurikenScript.Initialize(direction);
            }
            Debug.DrawLine(shuriken.transform.position, shuriken.transform.position + direction * 5f, Color.red, 2f);
        }

        // dash
        if (canDash && speedMultiplier == 1.0f && Input.GetKeyDown(KeyCode.Space))
        {
            canDash = false;
            Dash();
        }

        // camera
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.position.x, player.position.y, camZAxis), camSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        FlipPlayerSprite();
        if (canMove)
        {
            inputVector *= moveSpeed * speedMultiplier * Time.deltaTime * 100.0f;
            rb.linearVelocity = new Vector2(inputVector.x, inputVector.y);
        }
    }
    
    private void FlipPlayerSprite(){
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if(mousePos.x < player.position.x){
            player.GetComponent<SpriteRenderer>().flipX = true;
        } else{
            player.GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    void Dash()
    {
        rb.AddForce(lastInputVector.normalized * dashDistance, ForceMode2D.Force);
        StartCoroutine(ScreenShake(dashShakeDuration, dashShakeIntensity));
        StartCoroutine(WaitForDashEnd(dashDuration));
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.sizeDelta = new Vector2(currentHealth * 70f, healthBar.sizeDelta.y);
    }

    void Death()
    {
        Debug.Log("DIED");
        player.position = checkpoint.transform.position;
        currentHealth = maxHealth;
        healthBar.sizeDelta = new Vector2(currentHealth * 70f, healthBar.sizeDelta.y);
    }

    IEnumerator WaitForDashEnd(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        rb.linearVelocity = Vector2.zero;
        canMove = true;
        StartCoroutine(WaitForDashCooldown(dashCooldown));
    }

    IEnumerator WaitForDashCooldown(float cooldown)
    {
        yield return new WaitForSecondsRealtime(cooldown);
        canDash = true;
    }

    IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPosition = mainCamera.transform.localPosition;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        mainCamera.transform.localPosition = originalPosition;
    }
}
