# CS2Tags_VipTag
This plugin allows to someone with specific permission to set up their own scoreboard tag, chat tag and colors. This plugin uses MySQL database to store information about tags.

## [📌] Version for K4-Arenas:
- [Arena-VipTagChange](https://github.com/Letaryat/CS2-Arena-VipTagChange).

## [📌] Dependiencies:
- [CounterStrikeSharp (tested on v294)](https://github.com/roflmuffin/CounterStrikeSharp)  
- [CS2-Tags (at least v1.1)](https://github.com/schwarper/cs2-tags)
- [PlayerSettings (needed for MenuManager)](https://github.com/NickFox007/PlayerSettingsCS2)
- [AnyBaseLibCS2 (needed for MenuManager)](https://github.com/NickFox007/AnyBaseLibCS2)
- [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2)

## [📋] Commands:
- !settag  | Sets up tag. Usage: !settag ExampleTag,
- !tagmenu | Displays menu,

## [📋] Functions:
- Changing scoreboard and chat tag,
- Changing color of tag, chat text color, name color,
- Turning on / off the tag,
- Storing all data in MySQL database,

## [📌] Setup
- Install all dependiencies listed upwards,
- Download latest release,
- Drag files to /plugins/
- Restart your server,
- Config file should be created in configs/plugins/CS2Tags_VipTag,
- Edit to your liking,

```
{
  "VipFlag": "@vip-plugin", //flag example
  "DBHost": "", //MySQL Host
  "DBPort": 3306, //MySQL Port
  "DBUsername": "", //MySQL Username
  "DBName": "", //MySQL database name
  "DBPassword": "", //MySQL database password
  "ConfigVersion": 1
}
```

### [🩷] Thanks to:
- [CS2-Tags (at least v1.1)](https://github.com/schwarper/cs2-tags) for api,
- [CS2-Ranks](https://github.com/partiusfabaa/cs2-ranks) how to manage things such as keeping player information from database,
- CounterStrikeSharp discord,
- Probably some other open-source projects that I forgot to mention,
<br><img src="https://i.imgur.com/TQP4lYn.gif" height="200px">

### [🚨] Plugin might be poorly written and have some issues. I have no idea what I am doing. Even so, when tested it worked as intended.