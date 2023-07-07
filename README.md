
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
An Escape From Tarkov BepInEx module designed to be used with SPT-Aki Server with the ultimate goal of "Offline" Coop 
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [Deutsch](README_DE.md) **|** [简体中文](README_CN.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md)
</div>

---

## About

The Stay in Tarkov project was born due to Battlestate Games' (BSG) reluctance to create the pure PvE version of Escape from Tarkov. 
The project's aim is simple, create a Cooperation PvE experience that retains progression. If BSG decide to create the ability to do this on live, this project will be shut down immediately.

## Disclaimer

* You must buy the game to use this. You can obtain it here. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com). 
* This is by no means designed for cheats (this project was made because cheats have destroyed the Live experience)
* This is by no means designed for illegally downloading the game (and has blocks for people that do!)
* This is purely for educational purposes (I am using this to learn Unity, Reverse Engineering & Networking)
* I am in no way affiliated with BSG or others (on Reddit or Discord) claiming to be working on a project

## Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* Please be aware. The Ko-Fi link is buying me a coffee, nothing else!
* I do not have some special subset of code that makes it work beyond what is here on GitHub 
* Please do not hand over money expecting help or a solution
* This is a hobby, for fun, project. Please don't treat it seriously.
* I do not BS the community. I know this is a semi-broken attempt but will attempt to fix as best I can.
* Pull Requests are encouraged!

## SPT-AKI Requirement
* Stay in Tarkov works requires the [latest AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Server) to run. You can learn about SPT-Aki [here](https://www.sp-tarkov.com/).
* DO NOT INSTALL THIS ON TO SPT-Aki CLIENT! ONLY INSTALL THE SERVER!

## [Wiki](https://github.com/paulov-t/SIT.Core/wiki)
**The Wiki is under construction by various contributors. It may be broken! All instructions are also kept within the source in the wiki directory.**
  - ### [Setup Manuals](https://github.com/paulov-t/SIT.Core/wiki/Guides-English)
  - ### [FAQs](https://github.com/paulov-t/SIT.Core/wiki/FAQs-English)

## Coop

### Coop Summary
**BE AWARE**
* Coop is in early stages of development. 
* Most features work (ish) and it is "playable (ish) with likely bugs". "Playable" and perfect are two very different things. Expect lag (desync), issues and bugs.
* My tests have included all maps. The maps that work best are Factory and Labs. Performance is very dependant on the CPU / Internet on the Server and Clients and AI count on the Server
* More Information on HOSTING & COOP is in the [HOSTING.md Document](https://github.com/paulov-t/SIT.Core/wiki/en/Guides/HOSTING-English.md)

### PREREQUISITE
You must have the [SPT-Aki mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) installed in your Server for this module to work. If you do not wish to use the Coop module, you must disable it in the BepInEx config file.

### Can Coop use BSG code?
No. BSG server code is hidden from the client for obvious reasons. So BSG's implementation of Coop use the same online servers as PvPvE. We don't see this, so we cannot use this.

### Coding explanation
- The project uses multiple methods of BepInEx Harmony patches coupled with Unity Components to achieve its aims.
- Features/Methods that require constant polling between Client->Server->Client (Move, Rotate, Look, etc) use Components to send data (AI code runs the Update/LateUpdate command and the function every tick, therefore causing network flood).
- Features/Methods that can easily be "replicated" use ModuleReplicationPatch abstract class to easily round trip the call.
- All server communication is via JSON TCP Http and Web Socket calls to the ["Web Server" developed by SPT-Aki](https://dev.sp-tarkov.com/SPT-AKI/Server) using a [typescript mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) to handle the "backend" work.
- CoopGameComponent is attached to the GameWorld object when a Coop ready game is started (any game that isn't Hideout). CoopGameComponent polls the Server for information and passes the data to the PlayerReplicatedComponent.

## SPT-Aki

### Are Aki Modules supported?
The following Aki Modules are supported.
- aki-core
- Aki.Common
- Aki.Reflection
- 50/50 on SPT-AKI Client mods. This is dependant on how well written the patches are. If they directly target GCLASSXXX or PUBLIC/PRIVATE then they will likely fail.

### Why don't you use Aki Module DLLs?
SPT-Aki DLLs are written specifically for their own Deobfuscation technique and my own technique is not working well with Aki Modules at this moment in time.
So I ported many of SPT-Aki features into this module. My end-goal would be to rely on SPT-Aki and for this to be solely focused on SIT only features.

## How to compile? 
[Compiling Document](COMPILE.md)

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
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=pmc062158106353313252 -config={"BackendUrl":"http://127.0.0.1:6969","Version":"live"}
```
- If BepInEx is working a console should open and display the module "plugin" as started


## Thanks List
- SPT-Aki team
- MTGA team
- SPT-Aki Modding Community
- DrakiaXYZ ([Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints), [BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain))

## License

- DrakiaXYZ projects contain the MIT License
- 95% of the original core and single-player functionality completed by SPT-Aki teams. There may be licenses pertaining to them within this source.
- None of my own work is Licensed. This is solely a just for fun project. I don't care what you do with it.
