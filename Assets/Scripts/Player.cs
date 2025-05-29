using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    private GameObject mainCamera;
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

    // dash 
    [Header("Dash")]
    public float dashDistance = 50.0f;
    public float dashCooldown = 1.0f;
    public float dashDuration = 0.1f;
    public float dashShakeIntensity = 0.2f;
    public float dashShakeDuration = 0.1f;
    private bool canDash = true;

    // checkpoint
    [HideInInspector] public Checkpoint checkpoint;

    void Start()
    {
        canMove = true;
        canDash = true;

        currentHealth = maxHealth;

        mainCamera = GameObject.Find("PlayerCamera");
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("KICK");
            animator.SetTrigger("kick");
        }

        // dash
        if (canDash && Input.GetKeyDown(KeyCode.Space))
        {
            canDash = false;
            Dash();
        }

        // camera
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.position.x, player.position.y, camZAxis), camSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            inputVector *= moveSpeed * speedMultiplier * Time.deltaTime * 100.0f;
            rb.linearVelocity = new Vector2(inputVector.x, inputVector.y);
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
    }

    void Death()
    {
        Debug.Log("DIED");
        player.position = checkpoint.transform.position;
        currentHealth = maxHealth;
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
