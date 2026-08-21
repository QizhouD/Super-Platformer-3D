using UnityEditor;
using UnityEngine;

public static class PlatformPlaceTool {
    const string GroundLayerName = "Ground";

    [MenuItem("Platformer/Level Tools/Snap Selection To Grid %#g")]
    public static void SnapSelection() {
        var step = 1f;
        foreach (var transform in Selection.transforms) {
            Undo.RecordObject(transform, "Snap To Grid");
            var p = transform.position;
            transform.position = new Vector3(
                Mathf.Round(p.x / step) * step,
                Mathf.Round(p.y / step) * step,
                Mathf.Round(p.z / step) * step);
        }
    }

    [MenuItem("Platformer/Level Tools/Create Ground Cube")]
    public static void CreateGroundCube() {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Platform";
        var ground = LayerMask.NameToLayer(GroundLayerName);
        if (ground >= 0) cube.layer = ground;
        cube.transform.localScale = new Vector3(4f, 1f, 4f);
        if (SceneView.lastActiveSceneView != null)
            cube.transform.position = SceneView.lastActiveSceneView.pivot;
        Undo.RegisterCreatedObjectUndo(cube, "Create Ground Cube");
        Selection.activeGameObject = cube;
    }

    [MenuItem("Platformer/Level Tools/Add Path Moving Platform")]
    public static void AddPathMoving() {
        var selected = Selection.activeGameObject;
        if (selected == null) {
            Debug.LogWarning("Select a platform mesh first.");
            return;
        }

        Undo.AddComponent<Platformer.PathMovingPlatform>(selected);
        selected.tag = "MovingPlatform";
        var a = new GameObject("Waypoint_A");
        var b = new GameObject("Waypoint_B");
        a.transform.position = selected.transform.position;
        b.transform.position = selected.transform.position + selected.transform.right * 4f;
        a.transform.SetParent(selected.transform.parent, true);
        b.transform.SetParent(selected.transform.parent, true);
        Undo.RegisterCreatedObjectUndo(a, "Waypoint A");
        Undo.RegisterCreatedObjectUndo(b, "Waypoint B");
    }
}
