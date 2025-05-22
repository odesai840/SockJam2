using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private GameObject mainCamera;
    private Transform player;

    // movement
    [Header("Movement")]
    public Rigidbody2D rb;
    public float moveSpeed = 5.0f;
    private Vector2 inputVector;

    // camera
    [Header("Camera")]
    public float camZAxis = -100.0f;
    public float camSpeed = 4.0f;


    void Start()
    {
        mainCamera = GameObject.Find("PlayerCamera");
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        // movement
        inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        inputVector = Vector2.ClampMagnitude(inputVector, 1f);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.position.x, player.position.y, camZAxis), camSpeed * Time.deltaTime);
    
        // kick
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.LogWarning("KICK");
        }
    }

    void FixedUpdate()
    {
        inputVector = inputVector * moveSpeed * Time.deltaTime * 100.0f;
        rb.linearVelocity = new Vector2(inputVector.x, inputVector.y);
    }
}
