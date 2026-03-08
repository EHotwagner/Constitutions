# Constitutions

A collection of reusable project constitutions and a dotnet tool to apply them.

## Structure

Each constitution lives in its own folder:

```
fsGeneral/constitution.md   — F# general-purpose constitution
```

## upcons

A dotnet tool that downloads a constitution from this repo and replaces any `constitution.md` found in your current directory tree.

### Install

```sh
dotnet pack src/upcons/upcons.fsproj -c Release
dotnet tool install --global upcons --add-source ~/.local/share/nuget-local
```

### Usage

```sh
upcons fsGeneral
```

This will:
1. Download `fsGeneral/constitution.md` from GitHub
2. Find all `constitution.md` files in the current directory and subdirectories
3. Replace them with the downloaded version
