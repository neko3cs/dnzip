# DnZip

DnZip is a .NET command-line tool for creating ZIP archives from one or more files and directories, with optional recursive directory inclusion and password-based encryption.

It is intended as a simple alternative to the default Windows ZIP workflow, especially when you need encrypted archives from the command line.

Package: [DnZip on NuGet](https://www.nuget.org/packages/DnZip)

## Features

- Create a ZIP archive from one or more files and directories
- Include subdirectories recursively
- Encrypt the archive with a password prompt
- Omit directory entries like `zip -D`
- Use Shift_JIS entry encoding for better compatibility with common Windows archive tools

## Installation

You can install DnZip as a .NET tool:

```sh
dotnet tool install --global DnZip
```

If you want to update an existing installation:

```sh
dotnet tool update --global DnZip
```

## Usage

### Syntax

```sh
dnzip [options] <archiveFilePath> <sourcePath> [sourcePath...]
```

### Arguments

| Argument | Description |
| -- | -- |
| `archiveFilePath` | Output path of the ZIP archive to create |
| `sourcePath` | One or more files or directories to archive |

### Options

| Option | Description |
| -- | -- |
| `-r`, `--recurse` | Include subdirectories recursively |
| `-e`, `--encrypt` | Prompt for a password and create an encrypted ZIP archive |
| `-D`, `--no-dir-entries` | Do not create directory entries |

### Examples

Create a ZIP file from a directory:

```sh
dnzip output.zip ./data
```

Create a ZIP file including subdirectories:

```sh
dnzip output.zip ./data --recurse
```

Create an encrypted ZIP file:

```sh
dnzip output.zip ./data --encrypt
```

Create an encrypted ZIP file including subdirectories:

```sh
dnzip output.zip ./data --recurse --encrypt
```

Create a ZIP file from multiple inputs:

```sh
dnzip output.zip ./docs ./src/appsettings.json ./assets --recurse
```

Create a ZIP file without directory entries, like `zip -D`:

```sh
dnzip output.zip ./data --recurse --no-dir-entries
```

## Behavior

- Returns exit code `0` on success
- Returns exit code `1` on failure
- Prints an error message if any source path does not exist
- Prompts twice for password confirmation when `--encrypt` is used
