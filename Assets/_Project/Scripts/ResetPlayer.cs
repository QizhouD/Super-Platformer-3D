using UnityEngine;
using Unity.UI;
public class ResetPlayer : MonoBehaviour
{
    [Header("ResetPlayer")]
    [SerializeField] float fallThreshold = -20f;
    
    Vector3 originalPos;
    Rigidbody rb;

    void Start()
    {
        originalPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y <= fallThreshold)
        {
            ResetPlayerPosition();
        }
    }

    void ResetPlayerPosition()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        transform.position = originalPos;
    }
}