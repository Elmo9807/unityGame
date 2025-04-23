#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

// First approach: Add a build preprocessing step
public class SaveFileNuker
{
    [InitializeOnLoadMethod]
    static void RegisterBuildPreprocessor()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(DeleteSaveAndBuild);
    }

    static void DeleteSaveAndBuild(BuildPlayerOptions options)
    {
        // Delete save file
        string exactSavePath = @"C:/Users/dylan/AppData/LocalLow/DefaultCompany/OneOfMany/player.save";
        try
        {
            if (File.Exists(exactSavePath))
            {
                File.Delete(exactSavePath);
                Debug.Log("Successfully deleted save file before build at: " + exactSavePath);
            }
            else
            {
                Debug.Log("No save file found at: " + exactSavePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error trying to delete save file: " + e.Message);
        }

        // Continue with build
        BuildPipeline.BuildPlayer(options);
    }
}
#endif