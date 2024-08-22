#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FileFinderTool : EditorWindow
{
    private string fileID;
    private string guid;
    private Object foundAsset;

    [MenuItem("3MG/Tools/Asset Finder Tool")]
    public static void ShowWindow()
    {
        GetWindow<FileFinderTool>("Asset Finder Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("Find Asset by File ID or GUID", EditorStyles.boldLabel);

        fileID = EditorGUILayout.TextField("File ID", fileID);
        guid = EditorGUILayout.TextField("GUID", guid);

        if (GUILayout.Button("Find by File ID"))
        {
            foundAsset = FindAssetByFileID(fileID);
        }

        if (GUILayout.Button("Find by GUID"))
        {
            foundAsset = FindAssetByGUID(guid);
        }

        if (foundAsset != null)
        {
            EditorGUILayout.ObjectField("Found Asset", foundAsset, typeof(Object), false);
        }
    }

    private Object FindAssetByFileID(string fileID)
    {
        long parsedFileID;
        if (!long.TryParse(fileID, out parsedFileID))
        {
            Debug.LogError("Invalid File ID format.");
            return null;
        }

        string[] allGuids = AssetDatabase.FindAssets("");
        foreach (string guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
            {
                long localFileID;
                if (TryGetLocalFileID(path, out localFileID) && localFileID == parsedFileID)
                {
                    return AssetDatabase.LoadAssetAtPath<Object>(path);
                }
            }
        }

        Debug.LogError("Asset not found.");
        return null;
    }

    private Object FindAssetByGUID(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (!string.IsNullOrEmpty(path))
        {
            return AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        Debug.LogError("Asset not found.");
        return null;
    }

    private bool TryGetLocalFileID(string assetPath, out long localFileID)
    {
        localFileID = 0;
        string assetMetaPath = assetPath + ".meta";
        if (!System.IO.File.Exists(assetMetaPath))
        {
            return false;
        }

        string[] metaLines = System.IO.File.ReadAllLines(assetMetaPath);
        foreach (string line in metaLines)
        {
            if (line.Trim().StartsWith("fileID:"))
            {
                string fileIDStr = line.Split(':')[1].Trim();
                if (long.TryParse(fileIDStr, out localFileID))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
#endif