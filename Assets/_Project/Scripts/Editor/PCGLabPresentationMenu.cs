using Platformer.PCG;
using UnityEditor;
using UnityEngine;

public static class PCGLabPresentationMenu {
    [MenuItem("Platformer/PCG/Apply Lab Presentation")]
    public static void ApplyLabPresentation() {
        var panel = Object.FindObjectOfType<PCGDebugPanel>();
        if (panel == null) {
            Debug.LogWarning("Open PCG_Lab and exit Play Mode before applying presentation.");
            return;
        }

        PCGLabExperience.EnsureInstalled(panel);
        EditorUtility.SetDirty(panel.gameObject);
        Debug.Log("PCG Lab presentation is installed. Enter Play Mode to see the upgraded HUD, lighting, and audio.");
    }
}
