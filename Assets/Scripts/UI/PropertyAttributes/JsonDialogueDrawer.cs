using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(JsonDialogueAttribute))]
public class JsonDialogueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        JsonDialogueAttribute folderAttr = (JsonDialogueAttribute)attribute;

        EditorGUI.BeginProperty(position, label, property);

        var assets = AssetDatabase.FindAssets("t:TextAsset", new[] { $"Assets/Resources/Dialogue{folderAttr.FolderPath}" });
        string[] paths = new string[assets.Length];
        string[] names = new string[assets.Length];
        int currentIndex = -1;

        for (int i = 0; i < assets.Length; i++)
        {
            paths[i] = AssetDatabase.GUIDToAssetPath(assets[i]);
            names[i] = System.IO.Path.GetFileNameWithoutExtension(paths[i]);

            if (property.objectReferenceValue != null &&
                AssetDatabase.GetAssetPath(property.objectReferenceValue) == paths[i])
            {
                currentIndex = i;
            }
        }

        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, names);

        if (newIndex != currentIndex && newIndex >= 0)
        {
            property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(paths[newIndex]);
        }

        EditorGUI.EndProperty();
    }
}
