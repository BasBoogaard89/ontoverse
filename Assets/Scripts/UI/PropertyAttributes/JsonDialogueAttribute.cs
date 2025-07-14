using UnityEngine;

public class JsonDialogueAttribute : PropertyAttribute
{
    public string FolderPath;

    public JsonDialogueAttribute()
    {
    }

    public JsonDialogueAttribute(string folderPath)
    {
        FolderPath = folderPath;
    }
}
