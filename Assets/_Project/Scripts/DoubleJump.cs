using UnityEngine;

namespace Platformer
{
    public class DoubleJump : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [SerializeField] float rotationSpeed = 50f;
        [SerializeField] float bobSpeed = 2f;
        [SerializeField] float bobHeight = 0.5f;
        
        Vector3 startPosition;
        
        void Start()
        {
            startPosition = transform.position;
            
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
        
        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.EnableDoubleJump();
                    Destroy(gameObject);
                }
            }
        }
    }
}