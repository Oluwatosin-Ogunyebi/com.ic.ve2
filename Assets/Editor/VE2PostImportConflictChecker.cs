using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public static class VE2PostImportConflictChecker
{
    // List of asset paths in the project that might conflict with your package.
    static readonly string[] conflictPaths = new string[]
    {
        "Assets/Settings", // Entire folder conflict
        "Assets/Settings/SampleSceneProfile.asset",
        "Assets/Readme.asset",
        "Assets/Settings/URP-Balanced-Renderer.asset",
        "Assets/Settings/URP-Balanced.asset",
        "Assets/Settings/URP-HighFidelity-Renderer.asset",
        "Assets/Settings/URP-HighFidelity.asset",
        "Assets/Settings/URP-Performant-Renderer.asset",
        "Assets/Settings/URP-Performant.asset",
        "Assets/UniversalRenderPipelineGlobalSettings.asset"
    };

    static bool hasCheckedConflicts = false;

    static VE2PostImportConflictChecker()
    {
        // Delay the conflict check until the Editor is fully initialized.
        EditorApplication.delayCall += CheckForConflicts;
    }

    static void CheckForConflicts()
    {
        if (hasCheckedConflicts)
            return;
        hasCheckedConflicts = true;

        List<string> foundConflicts = new List<string>();

        // Check each specified path for an existing asset or folder.
        foreach (string path in conflictPaths)
        {
            if (Directory.Exists(path) || AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                foundConflicts.Add(path);
            }
        }

        if (foundConflicts.Count > 0)
        {
            // Build a message listing all conflicting paths.
            string message = "The following assets in your project conflict with package assets and may cause them to be ignored:\n\n";
            foreach (string conflict in foundConflicts)
            {
                message += conflict + "\n";
            }
            message += "\nDo you want to delete these conflicting assets? (This cannot be undone)";

            bool delete = EditorUtility.DisplayDialog(
                "Asset Conflict Detected",
                message,
                "Delete Conflicting Assets",
                "Keep Existing"
            );

            if (delete)
            {
                foreach (string path in foundConflicts)
                {
                    bool success = AssetDatabase.DeleteAsset(path);
                    if (!success)
                    {
                        Debug.LogError("Failed to delete conflicting asset: " + path);
                    }
                }
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog(
                    "Conflict Resolved",
                    "Conflicting assets have been deleted. The package assets will now be used.",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Import Warning",
                    "The conflicting assets will remain. Any package assets with these names will be ignored.",
                    "OK"
                );
            }
        }
    }
}
