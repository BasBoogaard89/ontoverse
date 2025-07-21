using UnityEditor;
using UnityEngine;

namespace Ontoverse.DialogueSystem.Editor
{
    public static class AssetHelper
    {
        public static string EditorUIPath = "Assets/Scripts/DialogueSystem/Editor";

        public static T LoadEditorAsset<T>(string relativePath) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>($"{EditorUIPath}/{relativePath}");
        }
    }
}