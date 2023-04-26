# HOSTING

## How to allow others to join your SPT-Aki Server?
1) Install Aki-Server
2) Open Aki_Data\Server\configs\http.json with your favourite text editor
3) Change the `ip` setting to your internal network IP of your Computer Primary Network (Ethernet or Wi-Fi)
4) Change the `logRequests` setting to `false` to prevent log spam
5) Forward ports of your router to your Computer Primary Network on port 6969
6) Install [SPT-Aki mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) in user\mods of your Server
7) Get your IPv4 from https://www.whatismyip.com/
8) Start the Server and provide the others your IP and port to connect to

## How to join each other's match
THIS IS EXTREMELY EXPERIMENTAL AT THIS TIME

### HOST
1) The HOST must select a Map, Time and Offline Settings - It is HIGHLY RECOMMENDED that you test on FACTORY first
2) The HOST should see "START MATCH" instead of "READY" on the last screen
3) The HOST clicks "START MATCH"
4) The HOST waits in Raid for other members to join, do NOT engage with bots or open any doors!

## CLIENT
1) The CLIENT must wait until the HOST has loaded the match and is in Raid
2) The CLIENT must select the same Map & Time
3) The CLIENT must ensure Bot Amount is None in the OFFLINE SETTINGS
4) The CLIENT clicks "JOIN MATCH"

## NOTES / ISSUES
- Both HOST and CLIENT should spawn on each others game after a few seconds
- CLIENT may not see BOTS until they are spawned in their RAID and may be targeted by BOTS on the HOST before CLIENT spawns them
- CLIENT may not see some doors opened
- Only ONE match can run on a Server at ONE time, this means if the HOST or CLIENT dies, they must wait for the game to FINISH before playing again
- If the HOST dies, the SERVER is DEAD. CLIENTS can freely escape or disconnect as they wish
- Loot is NOT the same between HOST & CLIENT, there may be a desync between them - THIS IS PRIORITY 1 TO FIX
