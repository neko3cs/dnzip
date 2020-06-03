# dnzip

Zip archive cli tool made with .NET Core Tools.

Default windows archiver (it is in right click context menu) can not encrypted zip and very slow.

So, you can use this ```zip``` command like linux ```zip``` command.

## How to use

### Syntax

```
$ dnzip [options] <archiveFilePath> <sourceDirectoryPath>
```

|Argument|Mean|
|--|--|
|```archiveFilePath```|Archive with the directory structure recursively.|
|```sourceDirectoryPath```|Target directory path that include files and directories you wanna archive.|

### Options

|Otion|Function|
|--|--|
|```-r```, ```recursePaths```|Archive with the directory structure recursively.|
|```-e```, ```--encrypt```|Encrypt the archived file.|
