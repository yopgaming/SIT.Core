
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
Ein Escape From Tarkov BepInEx Modul für SPT-Aki mit dem Ziel einen "Offline" Coop-Modus zu emulieren.
</div>

---

## Über

Das Projekt wurde ins Leben gerufen da Battlestate Games (BSG) bisher keine reine PvE-Erfahrung mit gespeichertem Fortschritt bietet.
Das Ziel ist einfach: Ein PvE Spielmodus in dem du mit Freunden spielen kannst und deine/eure Items und Fortschritt erhalten bleiben.
<br>Sobald BSG allerdings solch einen Spielmodus anbietet wird das Projekt sofort eingestampft!

## Installation

Zu aller erst: Du musst eine aktuelle, legitime Version von Tarkov auf deinem System installiert haben. <br>SIT funktioniert nicht mit alten und/oder gecrackten Versionen!
<br><br>
Der einfachste Weg ist über den bereitgestellten Launcher -> [Download](https://github.com/paulov-t/SIT.Launcher/releases) <- <br><br>
Nach dem starten wird dich der Launcher nach einem Ordnerpfad fragen, wähle hier NICHT deinen richtigen Tarkov-Ordner aus sondern einen neuen, leeren!!!
In dem neuen Ordner wird eine Kopie deiner Tarkov-Installation angelegt und alles nötige installiert, du brauchst also etwa 37-40GB freien Speicher dort!
<br>Der ganze Prozess dauert ein paar Minuten. 

Sobald alles aufgesetzt ist kannst du im Launcher die Serveradresse, deine Accountnamen und Passwort eingeben und dich verbinden.
<br><br>
!!! NICHT deine BSG-Accountdaten !!!
<br><br>
Wähle irgendwas anderes, der Account gilt nur für den emulierten Tarkov-Server und alle Daten werden im Klartext gespeichert!!!
<br>
Beim ersten einloggen wird dir angezeigt, dass der Account nicht existiert und ob du ihn anlegen möchtest, einfach bestätigen. <br>
Im Anschluss sollte dein Tarkov starten und bereit für Coop sein! :)

## Server-Setup

Hier findest du den aktuellen SPT-AKI-Server -> [Download](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)
<br>
Den an deinen Wunschort entpacken.<br><br>
Als nächstes brauchst du den SIT-Servermod -> [Download](https://github.com/paulov-t/SIT.Aki-Server-Mod)<br>
Grüner "Code" Knopf -> Download ZIP<br><br>
Der Servermod kommt ins Server-Verzeichnis unter `.../user/mods/` sodass dort ein neuer Ordner mit den Modinhalten ist.<br>
Wenn der Ordner nicht existiert kannst du ihn entweder selbst erstellen oder einfach den Server kurz starten.<br>
Sobald der Servermod drinnen ist starte den Server noch einmal kurz damit alle Config-Dateien usw. angelegt werden.

## Server-Config

### Ohne Hamachi
Der Server hat einen automatischen IP-Finder, du müsstest theoretisch nur die Kommunikation über deine Firewall erlauben und den Port `6969` in deinem Router freischalten und deine IP mit deinen Freunden teilen. Vergiss aber nicht, dass sich deine Adresse öfter mal ändern kann!

### Mit Hamachi
1. Versichere dich, dass Hamachi läuft.
2. Kopiere deine Hamachi-Adresse. Als Beispiel hier: `5.0.0.1`
3. Öffne die Server-Config im Servermod-Ordner `.../user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.
4. Tausche die Adresse in `externalIP` mit deiner Hamachi-Adresse aus z.B. `http://5.0.0.1:6969`.
5. Setze `useExternalIPFinder` auf `false`.
6. Öffne die Serverconnection-Config im Server-Ordner in `.../Aki_Data/Server/configs/http.json`.
7. Setze bei `ip` deine Hamachi-Adresse ein z.B. `5.0.0.1`.
8. Optional kannst du noch `logRequests` auf `false` setzen um Log-Spam zu verhindern.

### Remote/Root-Server
nachui


## [Guides](./Guides)
## [FAQs](./FAQs)

