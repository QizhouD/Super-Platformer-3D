using UnityEngine;
using TMPro;
using System.Collections;

namespace Platformer
{
    public class UINotification : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI notificationText;
        [SerializeField] float displayDuration = 2f;
        [SerializeField] CanvasGroup canvasGroup;
        
        static UINotification instance;
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
        
        public static void ShowNotification(string message)
        {
            if (instance != null)
            {
                instance.Display(message);
            }
        }
        
        void Display(string message)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayCoroutine(message));
        }
        
        IEnumerator DisplayCoroutine(string message)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                yield return new WaitForSeconds(displayDuration);
                canvasGroup.alpha = 0f;
            }
        }
    }
}