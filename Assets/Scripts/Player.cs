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

    // movement
    [Header("Movement")]
    public Rigidbody2D rb;
    public float moveSpeed = 5.0f;
    private Vector2 inputVector;
    private Vector2 lastInputVector;
    private bool canMove = true;

    // camera
    [Header("Camera")]
    public float camZAxis = -100.0f;
    public float camSpeed = 4.0f;

    // dash 
    [Header("Dash")]
    public float dashDistance = 50.0f;
    public float dashCooldown = 1.0f;
    public float dashDuration = 0.1f;
    public float dashShakeIntensity = 0.2f;
    public float dashShakeDuration = 0.1f;
    private bool canDash = true;

    void Start()
    {
        canMove = true;
        canDash = true;

        mainCamera = GameObject.Find("PlayerCamera");
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
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
            Debug.LogWarning("KICK");
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
            inputVector *= moveSpeed * Time.deltaTime * 100.0f;
            rb.linearVelocity = new Vector2(inputVector.x, inputVector.y);  
        }
    }

    void Dash()
    {
        rb.AddForce(lastInputVector.normalized * dashDistance, ForceMode2D.Force);
        StartCoroutine(ScreenShake(dashShakeDuration, dashShakeIntensity));
        StartCoroutine(WaitForDashEnd(dashDuration));
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
