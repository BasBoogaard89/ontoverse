using Ontoverse.Utils;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ontoverse.DialogueSystem.Editor
{
    public static class DialogueFileService
    {
        private static readonly string folderPath = Application.dataPath + "/Resources/Dialogue";

        public static string[] GetAllFilenames()
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return Directory.GetFiles(folderPath, "*.json")
                .Select(Path.GetFileName)
                .ToArray();
        }

        public static DialogueGraph LoadDialogue(string filename, bool decode = false)
        {
            string path = Path.Combine(folderPath, filename);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"File not found: {path}");
                return null;
            }

            string json = decode
                ? JsonCompressor.DecompressBytes(File.ReadAllBytes(path))
                : File.ReadAllText(path);

            return JsonCompressor.Deserialize<DialogueGraph>(json);
        }

        public static void SaveDialogue(DialogueGraph graph, string defaultFilename = "dialogue", bool encode = false)
        {
            string path = EditorUtility.SaveFilePanel(
                "Save Dialogue Graph",
                folderPath,
                defaultFilename,
                "json"
            );

            if (string.IsNullOrEmpty(path))
                return;

            string json = JsonCompressor.Serialize(graph);

            if (encode)
            {
                var compressed = JsonCompressor.CompressString(json);
                File.WriteAllBytes(path, compressed);
            } else
            {
                File.WriteAllText(path, json);
            }

            Debug.Log("Graph saved to " + path);
            AssetDatabase.Refresh();
        }
    }
}
