using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

[InitializeOnLoad]
public static class ScopedRegistryAdder
{
    static ScopedRegistryAdder()
    {
        // Delay the registry update until the Editor is ready.
        EditorApplication.delayCall += AddScopedRegistries;
    }

    private static void AddScopedRegistries()
    {
        // Path to the manifest.json file in the project root.
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        if (!File.Exists(manifestPath))
        {
            Debug.LogError("manifest.json not found at: " + manifestPath);
            return;
        }

        string json = File.ReadAllText(manifestPath, Encoding.UTF8);
        // Parse the JSON using MiniJSON (or your preferred JSON library)
        var manifestDict = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
        if (manifestDict == null)
        {
            Debug.LogError("Failed to parse manifest.json");
            return;
        }

        // Get or create the scopedRegistries list.
        List<object> scopedRegistries;
        if (manifestDict.ContainsKey("scopedRegistries"))
        {
            scopedRegistries = manifestDict["scopedRegistries"] as List<object>;
        }
        else
        {
            scopedRegistries = new List<object>();
            manifestDict["scopedRegistries"] = scopedRegistries;
        }

        // Define the OpenUPM registry entry.
        Dictionary<string, object> openUpmRegistry = new Dictionary<string, object>
        {
            { "name", "OpenUPM" },
            { "url", "https://package.openupm.com" },
            { "scopes", new List<object>
                {
                    "com.browar.editor-toolbox",
                    "com.github-glitchenzo.nugetforunity",
                    "net.tnrd.nsubstitute"
                }
            }
        };

        // Check if the registry already exists.
        bool registryExists = false;
        foreach (var entry in scopedRegistries)
        {
            Dictionary<string, object> registry = entry as Dictionary<string, object>;
            if (registry != null && registry.ContainsKey("name") && registry["name"].ToString() == "OpenUPM")
            {
                registryExists = true;
                break;
            }
        }

        if (!registryExists)
        {
            scopedRegistries.Add(openUpmRegistry);
            // Write back the updated manifest.
            string updatedJson = MiniJSON.Json.Serialize(manifestDict);
            File.WriteAllText(manifestPath, updatedJson, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("Added OpenUPM scoped registry to manifest.json.");
        }
        else
        {
            Debug.Log("OpenUPM scoped registry already exists in manifest.json.");
        }
    }
}
