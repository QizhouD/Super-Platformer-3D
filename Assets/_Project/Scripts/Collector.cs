using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Collector : MonoBehaviour
{
    [Header("Score Settings")]
    public int points = 0;
    public int maxPoints;

    [Header("UI References (Assign in Inspector)")]
    public TextMeshProUGUI scoreText; // TextMeshProUGUI component in the scene

    // Property to automatically update UI when points change
    public int Points
    {
        get => points;
        set
        {
            points = value;
            UpdateScoreUI();
        }
    }

    private void Start()
    {
        // Count all objects tagged as "Waste" at the start
        maxPoints = GameObject.FindGameObjectsWithTag("Waste").Length;
        Debug.Log($"Total targets: {maxPoints}");

        // Auto-find TextMeshProUGUI if not assigned (manual assignment recommended)
        if (scoreText == null)
        {
            scoreText = FindObjectOfType<TextMeshProUGUI>();
            if (scoreText == null)
            {
                Debug.LogWarning("TextMeshProUGUI not found! Please create one and assign it in the Inspector.");
            }
        }

        UpdateScoreUI(); // Initial UI update
    }

    private void Update()
    {
        // Load next scene when all targets are collected
        if (points >= maxPoints)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Refresh score display
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Targets Collected: {points}/{maxPoints}";

            // Optional: change color based on progress (yellow → green)
            float progress = (float)points / maxPoints;
            scoreText.color = Color.Lerp(Color.yellow, Color.green, progress);
        }
    }

    // Public method for other scripts to add points
    public void AddPoint(int amount = 1)
    {
        Points += amount;
    }
}