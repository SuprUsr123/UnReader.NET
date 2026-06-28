# Building!

## Prerequisites

Before building, ensure you have the following installed:

-   Windows 10 or later (Windows 11 recommended)
    
-   .NET 10 SDK
    
-   Git (required only if cloning the repository)
    

For Visual Studio users:

-   Visual Studio 2026 (Community or later) with the **.NET Desktop Development** workload.
    
 ## Even more notes!
 To let the app use your server, edit these lines first:
 ```vb.net
 ' Edit!
Public api As New ServerReader(SERVER_URL, JWT_SECRET)
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

Build the project:

```bash
dotnet build -c Release

```

To create a publishable build:

```bash
dotnet publish -c Release

```

Build artifacts can be found in the `bin` directory of each project. Published output is located under the project's `publish` folder.

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
