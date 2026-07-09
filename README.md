# UnReader.NET
A tiny little alternative UI made specifically to interact with (forks of) unreader.
Made in 7 days. (because i got dragged away with studying for the IELTS, playing [SMB1 Remastered](https://github.com/JHDev2006/Super-Mario-Bros.-Remastered-Public), [OneShot](https://store.steampowered.com/app/420530/OneShot/), [DRC5](https://store.steampowered.com/app/1671210/DELTARUNE/), [THSC](https://store.steampowered.com/app/1089980/The_Henry_Stickmin_Collection/), repacking [Garry's Mod](https://store.steampowered.com/app/4000/Garrys_Mod/), etc etc)

WARNING: Shitty code and AI aided code ahead! Read the source code with caution! Or preferrably not read it at all.

## Supported versions

This repository contains two separate UI fronts and a shared backend:

- `UnReader.NET` — original Windows WinForms application. (Most complete version, can run through Wine if you want a full fleshed experience)
- `UnReader.NET.Gtk` — Linux frontend using GTK#. (Still relatively incomplete, due to how I just stitched things together. Man do I love how there's no WYSIWIG editor for GTK#)
- `UnReader.NET.Linux` — Linux console runner and entrypoint for GTK-based deployments. (Barebones! It doesn't send anything, just views.)
- `UnReader.NET.Core` — shared logic used by both UI flavors.

## Release packaging

For GitHub releases, keep Windows and Linux artifacts separate:

- Build and publish `UnReader.NET` for the Windows WinForms release.
- Build and publish `UnReader.NET.Gtk` for the Linux GTK# release.
- Keep output folders distinct, and do not mix `.dll`/`.exe` artifacts from the Windows UI with the Linux GTK# UI.

A typical release layout:

- `release/windows/` — Windows WinForms published binaries.
- `release/linux/` — Linux GTK# published binaries.

## Build guidance

See `BUILD.md` for explicit commands for each project and release output path.
