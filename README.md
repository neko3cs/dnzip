# dnzip

Zip archive cli tool made with .NET Core Tools.

Default windows archiver (it is in right click context menu) can not create encrypted zip and very slow.

So, you can use this ```dnzip``` command like linux ```zip``` command.

Download here 👉 [DnZip - nuget.org](https://www.nuget.org/packages/DnZip)

## How to use

### Syntax

```
$ dnzip [options] <archiveFilePath> <sourceDirectoryPath>
```

|Argument|Mean|
|--|--|
|```archiveFilePath```|Archived file path that outputed by this command.|
|```sourceDirectoryPath```|Target directory path that include files and directories you wanna archive.|

### Options

|Otion|Function|
|--|--|
|```-r```, ```--recursePaths```|Archive with the directory structure recursively.|
|```-e```, ```--encrypt```|Encrypt the archived file.|
