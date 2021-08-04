# RimeworldModManager

RimworldModManger is a cli tool for checking, upgrading and installing rimworld mods.

## Installation

### Stand-alone version

1. Choose the version you need. Stand-alone version doesn't require .NET rutime. If you don't want to install runtime, you must choose stand-alone version. Framework dependency verion need the latest version [.NET runtime](https://dotnet.microsoft.com/download).
2. Unzip the file to your Rimworld root directory (recommend) or anywhere you like.
3. (Optinal) Add the directory to your system PATH.
4. Generate `RimworldModManagerSetting.json`.
   1. Open your system terminal.
   2. Run `RimworldModManager` or `./RimworldModManager.exe` if you don't add the directory to your system PATH. The program will gererate the json file automatically.
   3. Edit the json file.`GameModDirPath` is your Rimworld game `Mods` directory. `ModConfigXmlPath` is the directory that contains Rimworld `ModsConfig.xml` file. `ModConfigXmlPath` usually generate automatically. If the auto-generate path is wrong, you need to modify it.
5. Generate `modConfig.xml`.
   1. Run `RimworldModManager -Qy` in your terminal. The `modConfig,xml` will generate automatically.

## Usage

### Show mods information

```
RimworldModManager -Q
```

### Check mod upgrade

```
RimworldModManager -Qu
```

### Install mods

```
RimworldModManager -S [steamworkshop id]
```

The `steamworkshop id` can be found in the url:

For example

https://steamcommunity.com/sharedfiles/filedetails/?id=2023507013

is the steam workshop page for _Vanilla Expanded Framework_. Then `2023507013` is the `steamworkshop id`.

### Upgrade mods

```
RimworldModManager -Su
```

### Re-generate `modConfig.xml`

```
RimworldModManager -Qy
```
