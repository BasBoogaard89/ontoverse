using System;
using System.Collections.Generic;

[Serializable]
public class VDirectory
{
    public string Name;
    public List<VDirectory> Directories = new();
    public List<VFile> Files = new();
}