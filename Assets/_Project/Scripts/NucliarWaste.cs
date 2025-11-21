using UnityEngine;

public class NuclearWaste : MonoBehaviour  
{
    [Header("Collector Reference (Drag your Collector GameObject here - Recommended)")]
    public Collector collector;  

    private void OnTriggerEnter(Collider other)
    {
      
        if (other.CompareTag("Player"))
        {
         
            if (collector != null)
            {
                collector.AddPoint();  
                Destroy(gameObject);
                return;
            }

        
            Collector c = FindObjectOfType<Collector>();
            if (c != null)
            {
                c.AddPoint();  // 必须用 AddPoint()，不能 points++（否则 UI 不更新！）
            }
            else
            {
                Debug.LogError("Collector not found in scene! Add one.");
            }

            Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}