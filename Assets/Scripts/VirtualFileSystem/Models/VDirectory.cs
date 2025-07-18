using System;
using System.Collections.Generic;

public class VDirectory
{
    public string Name;
    public List<VDirectory> Directories = new();
    public List<VFile> Files = new();
}