# SIT.Core

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
- Please be aware. The Ko-Fi link is literally being me a coffee
- I do not have some special subset of code that magically makes it work beyond what is here on GitHub 
- Please do not hand over money expecting help or a magic solution
- I will update this README / GitHub Wiki with instructions when things are at a point where you can join and play a game together.

## Disclaimer

- You must buy the game to use this. You can obtain it here. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com). 
- This is by no means designed for cheats (this project was made because cheats have destroyed the Live experience)
- This is by no means designed for illegally downloading the game (and has blocks for people that do!)
- This is purely for educational purposes (I am using this to learn Unity, Reverse Engineering & Networking)
- I am in no way affiliated with BattleStateGames or others (on Reddit or Discord) claiming to be working on a project

## SPT-AKI Requirement
Stay in Tarkov requires the [latest AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Server) to run. You can learn about SPT-Aki [here](https://www.sp-tarkov.com/).

## Summary

The Stay in Tarkov project was born due to Battlestate Games' reluctance to create the pure PvE version of Escape from Tarkov that was outlined in its early cycle. 
The project's aim is simple, create a Cooperation PvE experience that retains progression without the issues of Desync, Cheaters and dealing with other issues surrounding the live game right now.

## Coop

### Highlight - BE AWARE
Coop is in very early stages of redevelopment. Nothing works.

### PREREQUISITE
You must have the [SPT-Aki mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) installed in your Server for this module to work. If you do not wish to use the Coop module, you must disable it in the BepInEx config file.

### Can Coop use BSG code?
No. BSG server code is hidden from the client for obvious reasons. So BSG's implementation of Coop use the same online servers as PvPvE. We don't see this, so we cannot use this.

### How it will work and reason
1. After rigourous testing in [SIT.Tarkov.Coop](https://github.com/paulov-t/SIT.Tarkov.Coop), I discovered that my UDP Web Socket implementation was much to unreliable and laggy.
2. With point 1 in mind, I have reverted back to basic TCP JSON web calls back and forth to the SPT-Aki Server with a mod handling the data. Initial movement tests work extremely well!

### Coding explanation
- The project uses multiple methods of BepInEx Harmony patches coupled with Unity Components to achieve its aims.
- Features/Methods that require constant polling between Client->Server->Client (Move, Rotate, Look, etc) use Components to send data (Harmony patches are too expensive).
- Features/Methods that can easily be "replicated" use ModuleReplicationPatch abstract class to easily round trip the call.
- All server communication is via JSON TCP calls to the ["Web Server" developed by SPT-Aki](https://dev.sp-tarkov.com/SPT-AKI/Server) using a [typescript mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) to handle the "backend" work.
- CoopGameComponent is attached to the GameWorld object when a Coop ready game is started (any game that isn't Hideout). CoopGameComponent polls the Server for information and passes the data to the PlayerReplicatedComponent.

### Progress
- See the Coop folder to see what features have been implemented
- Spawning on clients is broken
- BSG's code design is exceptionally difficult to create Harmony modules for and to work out what does what, so some features will 100% be missing!

## SPT-Aki

### Are Aki Modules supported?
The following Aki Modules are supported.
- aki-core
- Aki.Common
- Aki.Reflection
- 50/50 on Client mods (depends on how well written their reflection patterns are)

### Why don't you use Aki Module DLLs?
SPT-Aki DLLs are written specifically for their own Deobfuscation technique and my own technique is not working well with Aki Modules at this moment in time.
So I ported many of SPT-Aki features into this module. My end-goal would be to rely on SPT-Aki and for this to be solely focused on SIT only features.

## How to compile? 
1. Create Working Directory for all Tarkov Modding {EFT_WORK}
2. Clone this {SIT_CORE} to a {SIT_CORE} directory inside {EFT_WORK}
3. Copy your Live Tarkov Directory somewhere else {EFT_OFFLINE}
4. Deobfuscate latest Assembly-CSharp in {EFT_OFFLINE} via [SIT.Launcher](https://github.com/paulov-t/SIT.Tarkov.Launcher). Ensure to close and restart Launcher after Deobfuscation.
5. Copy all of {EFT_OFFLINE}\EscapeFromTarkov_Data\Managed assemblies to Tarkov.References {TARKOV.REF} in the parent folder of this project {EFT_WORK}
6. You will need BepInEx Nuget Feed installed on your PC by running the following command in a terminal. 
```
dotnet new -i BepInEx.Templates --nuget-source https://nuget.bepinex.dev/v3/index.json
```
7. Open the .sln with Visual Studio 2022
8. Rebuild Solution (This should download and install all nuget packages on compilation)

## Which version of BepInEx is this built for?
Version 5

# How to install BepInEx
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## Install to Tarkov
BepInEx 5 must be installed and configured first (see How to install BepInEx)
Place the built .dll in the BepInEx plugins folder

## Test in Tarkov
- Browse to where BepInEx is installed within your Tarkov folder
- Open config
- Open BepInEx.cfg
- Change the following setting [Logging.Console] Enabled to True
- Save the config file
- Run Tarkov through a launcher or bat file like this one (replacing the token with your ID)
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=AID062158106353313252ruc -config={"BackendUrl":"http://localhost:6969","Version":"live"}
```
- If BepInEx is working a console should open and display the module "plugin" as started


## Thanks List
- SPT-Aki team
- MTGA team

## License

- 95% of the original core and single-player functionality completed by SPT-Aki teams. There may be licenses pertaining to them within this source.
- None of my own work is Licensed. This is solely a just for fun project. I don't care what you do with it.