# Building!

## Prerequisites

Before building, ensure you have the following installed:

-   Windows 10 or later (Windows 11 recommended) for the WinForms build.
-   A GTK 3 runtime on the target system for the GTK# client (Linux, Windows, or macOS).
-   .NET 8 SDK.
-   Git (required only if cloning the repository).

For Visual Studio users:

-   Visual Studio 2022 (Community or later) with the **.NET Desktop Development** workload.

## Project layout

This repository contains multiple projects:

- `UnReader.NET` — original Windows WinForms UI.
- `UnReader.NET.Gtk` — Linux GTK# UI.
- `UnReader.NET.Linux` — console runner for Linux setups.
- `UnReader.NET.Core` — shared backend library.

## Build with a configured server

Run `./build-release.sh` from the repository root. It prompts for the server URL and JWT secret, temporarily applies the URL as the default for all desktop clients, publishes each client, then restores the source files even if a publish fails. `UNREADER_SERVER_URL` and `UNREADER_JWT_SECRET` can provide those values noninteractively. Use `--windows` or `--linux` to build one platform; CI can also set `UNREADER_RUNTIME_IDENTIFIER`, `UNREADER_SELF_CONTAINED`, and `UNREADER_VERSION`. The JWT value is passed to the build process but is not embedded in the desktop app; the server signing secret remains server-side.

After a successful publish, the script can save the server URL in the user's config directory and the JWT secret in the Linux Secret Service keyring (`secret-tool`). On later runs, Enter accepts the saved URL and secret. The JWT secret is never written into the repository. If `secret-tool` is unavailable, the script can still remember the URL but cannot save the secret.

The GTK login screen remembers the server URL and username, and can optionally save the password in the system keyring. User account passwords are not written to application settings. Do not put the server's JWT signing secret in a desktop client. The GTK and Linux console clients also accept `UNREADER_SERVER_URL` at runtime to override their configured default.

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

GTK# UI (build on the target OS, with its GTK 3 runtime available):

```bash
dotnet build ./UnReader.NET.Gtk/UnReader.NET.Gtk.vbproj -c Release
```

The GTK# project targets portable `net8.0`; the GTK 3 native runtime is still an OS-level dependency and must be installed on each target machine. `build-release.sh` currently publishes the Linux GTK artifact; use `dotnet publish` on Windows or macOS to produce a build for those platforms. Password saving uses Windows Credential Manager or Linux Secret Service; on other systems, the app still remembers the server and username but disables password saving.

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

## Automatic GitHub releases

Add the repository Actions secrets `UNREADER_SERVER_URL` and `UNREADER_JWT_SECRET`. Every branch push builds both platforms and uploads the packages as workflow artifacts. Pushes to the repository's default branch also publish a release automatically as `v1.0.N`, where `N` is the workflow run number; this makes each version unique and monotonically increasing. Change the `1.0` prefix in `.github/scripts/prepare-release.py` when starting a new minor or major version. GitHub's generated `GITHUB_TOKEN` publishes releases; no personal access token is needed. The JWT signing secret belongs only on the server and is not embedded in desktop clients. The Groq key follows the original site's built-in-key behavior and remains part of the app build.

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

Verify that the .NET 8 SDK is installed:

```bash
dotnet --version
```

If the command is not recognized or reports an older SDK version, install the .NET 8 SDK before building.

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
