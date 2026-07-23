using UnityEngine;
using UnityEditor;
using Platformer;

public static class CreatePlayer2Prefab
{
    const string MaleCharacterPath = "Assets/RPG Tiny Hero Duo/Prefab/MaleCharacterPBR.prefab";
    const string Player1Path = "Assets/_Project/Prefabs/Player 1.prefab";
    const string OutputPath = "Assets/_Project/Prefabs/Player 2.prefab";

    [MenuItem("Tools/Create Player 2 Prefab")]
    public static void Create()
    {
        GameObject player1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player1Path);
        if (player1Prefab == null)
        {
            Debug.LogError($"Player 1 not found at: {Player1Path}");
            return;
        }

        GameObject malePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MaleCharacterPath);
        if (malePrefab == null)
        {
            Debug.LogError($"MaleCharacter not found at: {MaleCharacterPath}");
            return;
        }

        // Read values from Player 1
        var p1Animator = player1Prefab.GetComponent<Animator>();
        var p1Capsule = player1Prefab.GetComponent<CapsuleCollider>();
        var p1Controller = player1Prefab.GetComponent<PlayerController>();
        var p1Health = player1Prefab.GetComponent<Health>();
        var p1GroundChecker = player1Prefab.GetComponent<GroundChecker>();

        // Create fresh GameObject from MaleCharacter (preserves materials/meshes)
        GameObject instance = Object.Instantiate(malePrefab);
        instance.name = "Player 2";
        instance.tag = "Player";

        // --- Animator: swap to Player 1's Controller ---
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.runtimeAnimatorController = p1Animator.runtimeAnimatorController;
        animator.applyRootMotion = p1Animator.applyRootMotion;
        animator.cullingMode = p1Animator.cullingMode;
        // Keep MaleCharacter's own Avatar (matches its skeleton)

        // --- CapsuleCollider ---
        CapsuleCollider capsule = instance.AddComponent<CapsuleCollider>();
        capsule.radius = p1Capsule.radius;
        capsule.height = p1Capsule.height;
        capsule.center = p1Capsule.center;
        capsule.direction = p1Capsule.direction;

        // --- Rigidbody ---
        Rigidbody rb = instance.AddComponent<Rigidbody>();
        // rb uses default values, which match Player 1's Rigidbody defaults

        // --- GroundChecker ---
        GroundChecker gc = instance.AddComponent<GroundChecker>();
        var gcSrc = new SerializedObject(p1GroundChecker);
        var gcDst = new SerializedObject(gc);
        gcDst.FindProperty("groundDistance").floatValue =
            gcSrc.FindProperty("groundDistance").floatValue;
        gcDst.FindProperty("groundLayers").intValue =
            gcSrc.FindProperty("groundLayers").intValue;
        gcDst.ApplyModifiedPropertiesWithoutUndo();

        // --- Health ---
        Health health = instance.AddComponent<Health>();
        var hSrc = new SerializedObject(p1Health);
        var hDst = new SerializedObject(health);
        hDst.FindProperty("maxHealth").intValue =
            hSrc.FindProperty("maxHealth").intValue;
        hDst.FindProperty("playerHealthChannel").objectReferenceValue =
            hSrc.FindProperty("playerHealthChannel").objectReferenceValue;
        hDst.ApplyModifiedPropertiesWithoutUndo();

        // --- PlayerController ---
        PlayerController pc = instance.AddComponent<PlayerController>();
        var pcSrc = new SerializedObject(p1Controller);
        var pcDst = new SerializedObject(pc);
        // Copy all movement/combat fields
        string[] pcFields = {
            "moveSpeed", "rotationSpeed", "smoothTime",
            "jumpForce", "jumpDuration", "jumpCooldown", "gravityMultiplier",
            "allowDoubleJump",
            "dashForce", "dashDuration", "dashCooldown", "allowDash",
            "attackCooldown", "attackDistance", "attackDamage"
        };
        foreach (var f in pcFields)
        {
            var sp = pcSrc.FindProperty(f);
            if (sp != null) pcDst.CopyFromSerializedProperty(sp);
        }
        pcDst.ApplyModifiedPropertiesWithoutUndo();

        // --- PlatformCollisionHandler ---
        instance.AddComponent<PlatformCollisionHandler>();

        // --- ResetPlayer ---
        instance.AddComponent<ResetPlayer>();

        // --- Save ---
        // Ensure directory
        System.IO.Directory.CreateDirectory(
            System.IO.Path.GetDirectoryName(OutputPath));

        // Remove old one if exists
        AssetDatabase.DeleteAsset(OutputPath);
        AssetDatabase.Refresh();

        PrefabUtility.SaveAsPrefabAsset(instance, OutputPath);
        Object.DestroyImmediate(instance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var result = AssetDatabase.LoadAssetAtPath<GameObject>(OutputPath);
        if (result != null)
        {
            Debug.Log($"<color=green>Player 2 created: {OutputPath}</color>");
            Selection.activeObject = result;
            EditorGUIUtility.PingObject(result);
        }
        else
        {
            Debug.LogError("Player 2 creation FAILED");
        }
    }
}
