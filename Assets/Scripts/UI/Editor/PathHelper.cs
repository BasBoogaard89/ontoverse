using UnityEditor;
using UnityEngine;

public static class AssetHelper
{
    public static string EditorUIPath = "Assets/Scripts/UI/Editor";

    public static T LoadEditorAsset<T>(string relativePath) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>($"{EditorUIPath}/{relativePath}");
    }
}