# Building!

## Prerequisites

Before building, ensure you have the following installed:

-   Windows 10 or later (Windows 11 recommended) for the WinForms build.
-   Linux (or another GTK-capable OS) for the GTK# build.
-   .NET 10 SDK.
-   Git (required only if cloning the repository).

For Visual Studio users:

-   Visual Studio 2026 (Community or later) with the **.NET Desktop Development** workload.

## Project layout

This repository contains multiple projects:

- `UnReader.NET` — original Windows WinForms UI.
- `UnReader.NET.Gtk` — Linux GTK# UI.
- `UnReader.NET.Linux` — console runner for Linux setups.
- `UnReader.NET.Core` — shared backend library.

## Even more notes!

To let the app use your server, edit these lines first:

.NET Forms version (UnReader.NET/Form1.vb)
```vb.net
' Edit!
Public api As New ServerReader(SERVER_URL, JWT_SECRET)
'End of edit.
```
GTK# version (for Linux, UnReader.NET.Gtk/Program.vb), as well as (barebones) console version (for Linux, UnReader.NET.Linux/Program.vb)
```vb.net
'Edit!
Dim serverUrl As String = If(Environment.GetEnvironmentVariable("UNREADER_SERVER_URL"), String.Empty)
Dim jwtSecret As String = If(Environment.GetEnvironmentVariable("UNREADER_JWT_SECRET"), String.Empty)
'End of edit.
```

----------

## Building with the .NET CLI

Clone the repository:

```bash
git clone https://github.com/SuprUsr123/UnReader.NET
cd UnReader.NET
```

Restore NuGet packages:

```bash
dotnet restore
```

### Build each project separately

Windows WinForms UI:

```bash
dotnet build ./UnReader.NET/UnReader.NET.vbproj -c Release
```

Linux GTK# UI:

```bash
dotnet build ./UnReader.NET.Gtk/UnReader.NET.Gtk.vbproj -c Release
```

Linux console runner:

```bash
dotnet build ./UnReader.NET.Linux/UnReader.NET.Linux.vbproj -c Release
```

### Publish separate release artifacts

Windows WinForms release:

```bash
dotnet publish ./UnReader.NET/UnReader.NET.vbproj -c Release -o ./release/windows
```

Linux GTK# release:

```bash
dotnet publish ./UnReader.NET.Gtk/UnReader.NET.Gtk.vbproj -c Release -o ./release/linux
```

Linux runner release:

```bash
dotnet publish ./UnReader.NET.Linux/UnReader.NET.Linux.vbproj -c Release -o ./release/linux-runner
```

Build artifacts are in the `bin` directories and published output is in each project's `publish` folder, or in the dedicated `release` directories above.

----------

## Building with Visual Studio

1.  Clone or download the repository.
2.  Open the solution (`.slnx`) in Visual Studio.
3.  Allow Visual Studio to restore any required NuGet packages.
4.  Select the desired configuration (typically **Release**) and target platform.
5.  Choose **Build → Build Solution** (or press **Ctrl+Shift+B**).

The compiled binaries will be available in the project's `bin` directory.

----------


## Troubleshooting

### Missing .NET SDK

Verify that the .NET 10 SDK is installed:

```bash
dotnet --version
```

If the command is not recognized or reports an older SDK version, install the .NET 10 SDK before building.

### NuGet restore failures

Run:

```bash
dotnet restore
```

If issues persist, clear the NuGet cache:

```bash
dotnet nuget locals all --clear
```

and restore again.

### Build errors after pulling updates

Clean the project before rebuilding:

```bash
dotnet clean
dotnet build -c Release
```
