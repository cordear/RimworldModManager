# RimeworldModManager

RimworldModManger is a cli tool for checking, upgrading and installing rimworld mods.

## Installation

1. Install latest [.NET runtime](https://dotnet.microsoft.com/download).
2. Download .zip file from release page.
3. Unzip the file to your Rimworld install directory (recommend) or anywhere you like.
4. (Optinal) Add the directory to your system PATH.
5. Generate `RimworldModManagerSetting.json`.
   1. Open your system terminal.
   2. Run `RimworldModManager` or `./RimworldModManager.exe` if you don't add the directory to your system PATH. The program will gererate the json file automatically.
   3. Edit the json file.`GameModDirPath` is your Rimworld game `Mods` directory. `ModConfigXmlPath` is the directory that contains Rimworld `ModsConfig.xml` file. `ModConfigXmlPath` usually generate automatically. If the auto-generate path is wrong, you need to modify it.
6. Generate `modConfig.xml`.
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
