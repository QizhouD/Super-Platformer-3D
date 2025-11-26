using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Platformer._Project.Scripts.UI
{
    public class SimplePauseManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject optionsMenuPanel;
        
        [Header("Player Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector3 respawnPosition;
        [SerializeField] private bool usePlayerStartPosition = true;
        
        private bool isPaused = false;
        private Vector3 initialPlayerPosition;
        private Rigidbody playerRigidbody;
        private PlayerInputActions inputActions;
        
        private void Awake()
        {
            inputActions = new PlayerInputActions();
        }
        
        private void Start()
        {
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
            inputActions.Enable();
            inputActions.Player.Pause.performed += OnPausePerformed;
        }
        
        private void OnDisable()
        {
            inputActions.Player.Pause.performed -= OnPausePerformed;
            inputActions.Disable();
        }
        
        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }
        
        private void TogglePause()
        {
            Debug.Log("TogglePause! Current state: " + isPaused);
            
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
            Debug.Log("Game Paused!");
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
            Debug.Log("Game Resumed!");
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
            Debug.Log("Player position reset!");
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
                Debug.LogWarning("Player Transform not found!");
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
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (optionsMenuPanel != null) optionsMenuPanel.SetActive(true);
        }
        
        public void HideOptions()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        }
    }
}
