using UnityEngine;

namespace Ontoverse.FileSystem
{
    public class FileSystemController
    {
        public TextAsset FileSystemJson;

        public FileSystemDirectory RootDirectory { get; private set; }

        void Awake()
        {
            LoadFromJson();
        }

        void LoadFromJson()
        {
            if (FileSystemJson == null)
            {
                Debug.LogError("FileSystem JSON niet toegewezen.");
                return;
            }

            RootDirectory = JsonUtility.FromJson<FileSystemDirectory>(FileSystemJson.text);

            if (RootDirectory == null)
                Debug.LogError("Kon bestandssysteem niet inladen.");
        }

        public void PrintFileTree()
        {
            FileSystemUtils.PrintTree(RootDirectory);
        }
    }
}
