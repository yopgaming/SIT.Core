## Infinite "Loading profile data..." screen

Caused by: 
- A broken installation.
- An issue with the server connection, related to your IP settings.

Make sure port forwarding is setup correctly so that you can connect to your external IP.

If you are playing Single Player, do not use any of these options and leave it as 127.0.0.1 and turn the External IP Finder off.

See: 
- [Discussion#139](https://github.com/paulov-t/SIT.Core/discussions/139)
- [Discussion#24](https://github.com/paulov-t/SIT.Core/discussions/24)
- [Issue#115](https://github.com/paulov-t/SIT.Core/issues/115)
- [Issue#60](https://github.com/paulov-t/SIT.Core/issues/60#issuecomment-1560461446)

---

## Where do I install mods?

### Client mods
Install client mods in `<game folder>/BepInEx/plugins/`.

### Server mods
Install server mods in `<server folder>/user/mods/`.

See:
- [Discussion#111](https://github.com/paulov-t/SIT.Core/discussions/111)
- [Discussion#134](https://github.com/paulov-t/SIT.Core/discussions/134)

---

## Port Forwarding by using Ddns technology?

Type 0.0.0.0 for http.json and server.json.

Type your domain address for CoopConfig.json.

---
