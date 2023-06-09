# Step by Step Stay In Tarkov Installation Guide

# Prerequisites 

Before we begin, please make sure the latest version of Escape From Tarkov has been downloaded and installed using the Battlestate Games Launcher. Stay In Tarkov will not work with an outdated or illegitimate copy of the game.

Throughout the guide, we will refer to `SIT_DIR` as the root directory for installing Stay In Tarkov. In this directory, we’ll create three separate folders to keep things organized:

- A `server` folder for the SPT-AKI server.
- A `launcher` folder for the SIT Launcher.
- A `game` folder for the Escape From Tarkov game files.

*Consider using a tool like [7zip](https://7-zip.org/) or WinRAR to unzip compressed files.*

# Installation

## 1. [SIT Launcher](https://github.com/paulov-t/SIT.Launcher/releases) (using auto install)

1. Download the latest release of the `SIT Launcher` from the [Releases](https://github.com/paulov-t/SIT.Launcher/releases) page. 
2. Unzip file and extract contents to `SIT_DIR/launcher`.
3. Run `SIT.Launcher.exe`.
4. The first time you run the launcher, it will prompt you for an installation: 
    *“No OFFLINE install found. Would you like to install now?”* 
    Click “Yes”.
5. Select `SIT_DIR/game` as the installation directory.
6. Let the launcher copy your game files, this can take a few minutes.

## 2. [SPT-AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)

1. Download the latest release of the `SPT-AKI Server` from the [Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) page.
2. Unzip file and extract contents to `SIT_DIR/server`.

## 3. [SIT Server Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)
1. Download the server mod’s zip file from [GitHub](https://github.com/paulov-t/SIT.Aki-Server-Mod) (look for it under the big green button: *Code > Download Zip*).
2. Unzip file and extract contents to `SIT_DIR/server/user/mods`.
    *The `user/mods` directory is automatically created when the server is run the first time. Run `Aki.Server.exe` to create the folder. Stop and close the server once the directory has been created so we can continue the installation process.*

# Configuring the server

## Hosted on localhost (for testing)

### Server
1. Open the coop server configuration file in `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`. 
    *The `coopConfig.json` file is automatically created when the server mod is run the first time. Run `Aki.Server.exe` to create the file. Stop and close the server once the file has been created so we can continue the installation process.*
   *Note: Make edits to the file using Notepad or a text editor that won't introduce formatting. Do not use Microsoft Word.*
2. Set `externalIP` to `http://127.0.0.1:6969`.
3. Set `useExternalIPFinder` to `false`.
4. Optionally, set `logRequests` to `false` in `SIT_DIR/server/Aki_Data/Server/configs/http.json` to prevent log spam.

### Launcher
Connect using `http://127.0.0.1:6969` as the server. 

*You won't be able to invite others to join your game using localhost, but it can be useful when debugging connection issues. Use this to confirm the game and mods are installed correctly.*

## Hosted with port forwarding

### Server
Your external IP address should be automatically detected, no further configuration is required.
Check the server logs for `COOP: Auto-External-IP-Finder` with your IP address.

Optionally, set `logRequests` to `false` in `SIT_DIR/server/Aki_Data/Server/configs/http.json` to prevent log spam.

### Launcher
Use the IP shown in the server’s `COOP: Auto-External-IP-Finder` log, or use the IP found on https://www.whatismyip.com to connect (they should match).

## Hosted with Hamachi VPN

### Server
1. Run Hamachi.
2. Find the IPv4 address shown in the LogMeIn Hamachi widget and copy it. We will use `100.10.1.10` as an example IP for this guide.
3. Open the coop server configuration file in `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`. 
    *The `coopConfig.json` file is automatically created when the server mod is run the first time. Run `Aki.Server.exe` to create the file. Stop and close the server once the file has been created so we can continue the installation process.*
   *Note: Make edits to the file using Notepad or a text editor that won't introduce formatting. Do not use Microsoft Word.*
4. Set `externalIP` to the IP we copied from LogMeIn Hamachi: `http://100.10.1.10:6969`.
5. Set `useExternalIPFinder` to `false`.
6. Open SPT-AKI's server connection configuration file in `SIT_DIR/server/Aki_Data/Server/configs/http.json`.
   *Note: Make edits to the file using Notepad or a text editor that won't introduce formatting. Do not use Microsoft Word.*
7. Set `ip` to `100.10.1.10`.
8. Optionally, set `logRequests` to `false` to prevent log spam.

### Launcher
Connect using the IPv4 address shown in the LogMeIn Hamachi widget. Our example would use `http://100.10.1.10:6969` as the server.

# Starting a game

## 1. Start the server

Run `Aki.Server.exe`


## 2. Start the game

Launch the game via the `SIT Launcher`.
*The first time you try to connect with new credentials, you will be prompted to create the account, click “Yes” (passwords are stored in plain text, do not reuse passwords). You may also be prompted to Alt+F4 after the game launches, if so, close the game and relaunch through SIT Launcher.*

## 3. Create a Lobby

See [How to join each other's match](https://github.com/paulov-t/SIT.Core/blob/master/HOSTING.md#how-to-join-each-others-match) for in-game instructions.
