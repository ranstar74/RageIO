# RageIO - A GTA 5 File System Access Library
Provides an API to work with Grand Theft Auto V Directories and Files.

## Requirements

* [Codewalker.Core](https://github.com/dexyfex/CodeWalker/)

## Basic Types:
* RageIO - Provides a description of an entry in a file system.
* DirectoryIO - Provides access to windows directories and RPF archives.
* FileIO - Provides access to windows files and files that are inside RPF archives.

## Examples:

### Reading dlclist.xml
```cs
string gtaPath = @"...\Grand Theft Auto V";
string dlcListPath = @"update\update.rpf\common\data\dlclist.xml";

// Decrypt and read all existing RPF's
CwCore.Init(gtaPath, true);

// Supports both relative and absolute pathes
FileIO dlcList = new FileIO(dlcListPath);

using(StreamReader sr = new StreamReader(dlcList.Open())
{
  string dlcListXml = sr.ReadToEnd();
  
  Console.WriteLine(dlcListXml);
  
  // Or
  
  XDocument dlcListDoc = XDocument.Parse(dlcListXml);
}
```
