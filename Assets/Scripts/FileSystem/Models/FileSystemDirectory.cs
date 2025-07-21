using System.Collections.Generic;

namespace Ontoverse.FileSystem
{
    public class FileSystemDirectory : BaseFileSystemEntry
    {
        public List<BaseFileSystemEntry> Children = new();
    }
}