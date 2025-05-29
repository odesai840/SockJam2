using UnityEngine;

public class Shuriken : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float projectileRange = 5f;
    
    private Vector3 startPosition;
    private Vector3 moveDirection;
    private bool isInitialized = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized) return;
        
        transform.position += moveDirection * projectileSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= projectileRange)
        {
            Destroy(gameObject);
        }
    }
    
    public void Initialize(Vector3 direction)
    {
        moveDirection = direction.normalized;
        isInitialized = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: enemy damage calculations
    }
}
