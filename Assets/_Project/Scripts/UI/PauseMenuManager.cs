using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer;

namespace Platformer.Project.Scripts.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject optionsMenuPanel;
        
        [Header("Input")]
        [SerializeField] private InputReader inputReader;
        
        [Header("Player Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector3 respawnPosition;
        [SerializeField] private bool usePlayerStartPosition = true;
        
        private bool isPaused = false;
        private Vector3 initialPlayerPosition;
        private Rigidbody playerRigidbody;
        
        private void Start()
        {
            gameObject.SetActive(false);
            
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    playerRigidbody = player.GetComponent<Rigidbody>();
                }
            }
            else
            {
                playerRigidbody = playerTransform.GetComponent<Rigidbody>();
            }
            
            if (usePlayerStartPosition && playerTransform != null)
            {
                initialPlayerPosition = playerTransform.position;
            }
            else
            {
                initialPlayerPosition = respawnPosition;
            }
            
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            
            if (optionsMenuPanel != null)
            {
                optionsMenuPanel.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.Pause += TogglePause;
            }
        }
        
        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.Pause -= TogglePause;
            }
        }
        
        private void TogglePause()
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        public void PauseGame()
        {
            gameObject.SetActive(true);
            isPaused = true;
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
            Time.timeScale = 0f;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void ResumeGame()
        {
            isPaused = false;
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            if (optionsMenuPanel != null)
            {
                optionsMenuPanel.SetActive(false);
            }
            Time.timeScale = 1f;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void ResetPlayerPosition()
        {
            if (playerTransform != null)
            {
                playerTransform.position = initialPlayerPosition;
                
                if (playerRigidbody != null)
                {
                    playerRigidbody.velocity = Vector3.zero;
                    playerRigidbody.angularVelocity = Vector3.zero;
                }
                
                ResumeGame();
            }
            else
            {
                Debug.LogWarning("Player Transform is not assigned!");
            }
        }
        
        public void BackToGame()
        {
            ResumeGame();
        }
        
        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Main Menu");
        }
        
        public void ShowOptions()
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            if (optionsMenuPanel != null)
            {
                optionsMenuPanel.SetActive(true);
            }
        }
        
        public void HideOptions()
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
            if (optionsMenuPanel != null)
            {
                optionsMenuPanel.SetActive(false);
            }
        }
        
        public void QuitGame()
        {
            Time.timeScale = 1f;
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
