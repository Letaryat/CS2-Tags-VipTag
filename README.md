# CS2Tags_VipTag
This plugin allows to someone with specific permission to set up their own scoreboard tag, chat tag and colors. This plugin uses MySQL database to store information about tags.<br/>
[![poor-developer discord server](https://i.imgur.com/8L6KsUZ.png)](https://discord.gg/mEmdyqM3Um)
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/H2H8TK0L9)

## [📌] Version for K4-Arenas:
- [Arena-VipTagChange](https://github.com/Letaryat/CS2-Arena-VipTagChange) // Not updated for 1.4 version.

## [📌] Dependiencies:
- [CounterStrikeSharp (tested on v294)](https://github.com/roflmuffin/CounterStrikeSharp)  
- [CS2-Tags (at least v1.4)](https://github.com/schwarper/cs2-tags)
- [CS2MenuManager](https://github.com/schwarper/CS2MenuManager)

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
- [CS2-Tags (at least v1.4)](https://github.com/schwarper/cs2-tags) for api,
- [CS2-Ranks](https://github.com/partiusfabaa/cs2-ranks) how to manage things such as keeping player information from database,
- CounterStrikeSharp discord,
- Probably some other open-source projects that I forgot to mention,
<br><img src="https://i.imgur.com/TQP4lYn.gif" height="200px">

### [🚨] Plugin might be poorly written and have some issues. I have no idea what I am doing. Even so, when tested it worked as intended.