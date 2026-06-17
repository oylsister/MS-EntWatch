# [Core]MS-EntWatch for ModSharp
Notify players about entity interactions

## Features:
1. JSON Configs
2. Multi-button control
3. Working with game_ui(Logic_case)
4. Block item pick up on E
5. It is possible to transfer a discarded or not yet selected item
6. You can specify the reason for the eban
7. Allows you to use the item in the crowd
8. Information output in HUD, which can be configured by each player separately
9. Allows you to track {number} buttons
10. Async functions
11. SQLite/MySQL/PostgreSQL support
12. Language setting for players
13. Allows you to set up individual access for admins
14. Keeps logs to a file and discord
15. The database is compatible with the old [EntWatch_DZ](https://github.com/darkerz7/EntWatch_DZ) and [EntWatchSharp](https://github.com/darkerz7/EntWatchSharp)
16. Work with math_counter (Mode: 6, 7) and HP (Mode: 8) is available
17. In-game config change
18. Offline ms_eban/ms_eunban
19. Applying filters for the activator
20. Items spawn
21. API for interaction with other plugins
22. Usage GameHUD API
23. Display in clantag
24. Allows you to select the player display format

## Required packages:
1. [ModSharp](https://github.com/Kxnrl/modsharp-public) (min. git132)
2. [ClientPreferences](https://github.com/Kxnrl/modsharp-public/tree/master/Sharp.Modules/ClientPreferences)
3. [LocalizerManager](https://github.com/Kxnrl/modsharp-public/tree/master/Sharp.Modules/LocalizerManager)
4. [AdminManager](https://github.com/Kxnrl/modsharp-public/tree/master/Sharp.Modules/AdminManager)
5. [TargetingManager](https://github.com/Kxnrl/modsharp-public/tree/master/Sharp.Modules/TargetingManager)
6. [AnyBaseLibNext](https://github.com/darkerz7/MS-AnyBaseLibNext-Shared)
7. [GameHUD](https://github.com/darkerz7/MS-GameHUD)

## Installation:
1. Install `ClientPreferences`, `LocalizerManager`, `AdminManager`, `TargetingManager`, `MS-AnyBaseLibNext-Shared` and `MS-GameHUD`
2. Compile or copy MS-EntWatch to `sharp/modules/MS-EntWatch` folger
3. Copy and configure the configuration file `db_config.json` and `log_config.json` to `sharp/modules/MS-EntWatch` folger
4. Copy `EntWatch.json` to `sharp/locales` folger
5. Copy and configure `mapsconfig` and `schemes` to `sharp/modules/MS-EntWatch/maps` and `sharp/modules/MS-EntWatch/scheme` folger
6. Compile or copy MS-EntWatch-Shared to `sharp/shared/MS-EntWatch-Shared` folger
7. Add CVARs to server.cfg
8. Restart server

## Example MapConfig
```
[
	{
		"Name": "",					//String, FullName of Item (Chat)
		"ShortName": "",				//String, ShortName of Item (Hud)
		"Color": "",					//String, One of the colors. (Chat)
		"HammerID": "",					//String, weapon_* HammerID
		"GlowColor": [0,0,0,0],				//Array[4], One of the colors. (Glow)
		"BlockPickup": false,				//Bool, The item cannot be picked up
		"AllowTransfer": false,				//Bool, Allow admins to transfer an item
		"ForceDrop": false,				//Bool, The item will be dropped if player dies or disconnects
		"Chat": false,					//Bool, Display chat items
		"Hud": false,					//Bool, Display Hud items
		"TriggerID": "",				//String, Sets a trigger that an ebanned player cant activate, mostly to prevent picking up weapon_knife items
		"UsePriority": false,				//Bool, Enabled by default. You can disable the forced pressing of the button on a specific item
		"SpawnerID": "",				//String, Allows admins to spawn items. Not recommended to use because it can break the items. Type point_template's HammerID which spawns the item
		"AbilityList": [				//Array of abilities
			{
				"Name": "",			//String, Custom ability name, can be omitted
				"ButtonID": "",			//String, Allows you to sort buttons
				"ButtonClass": "",		//String, Button Class, Can use "game_ui" for anoter activation method
				"Filter": "",			//String, Filter value for activator. |$attribute| for filter_activator_attribute_int (starts with $); |context:value| for filter_activator_context (contains :); other for filter_activator_name
				"Chat_Uses": false,		//Bool, Display chat someone is using an item(if disabled chat)
				"Mode": 0,			//Integer, Mode for item. 0 = Can hold E, 1 = Spam protection only, 2 = Cooldowns, 3 = Limited uses, 4 = Limited uses with cooldowns, 5 = Cooldowns after multiple uses, 6 = Counter - stops when minimum is reached, 7 = Counter - stops when maximum is reached, 8 = Health button
				"MaxUses": 0,			//Integer, Maximum uses for modes 3, 4, 5
				"CoolDown": 0,			//Integer, Cooldown of item for modes 2, 4, 5
				"Ignore": false,		//Bool, Ignore item display
				"LockItem": false,		//Bool, Lock button/door/game_ui_IO
				"MathID": "",			//String, math_counter HammerID for modes 6, 7
				"MathNameFix": false,		//Bool, Fix the name of the math_counter (Work with flag: Preserve entity names (Don't do name fixup) ->point_template/env_entity_maker)
				"MathFindSpawned": false,	//Bool, Search for math_counter on map after weapon spawn(e.x. The math_counter is not included in the point_template and spawns at the beginning of the round, and the weapon spawns much later than 2 seconds)
				"MathDontShowMax": false,	//Bool, Do not show maximum value
				"MathZero": false		//Bool, Allows pressing the button when the math_counter value is zero
			},
			{
				"Name": "",
				"ButtonID": "",
				"ButtonClass": "game_ui::PressedAttack",	//Example for Game_UI
				"Filter": "",
				"Chat_Uses": false,
				"Mode": 0,
				"MaxUses": 0,
				"CoolDown": 0,
				"Ignore": false,
				"LockItem": false,
				"MathID": "",
				"MathNameFix": false,
				"MathFindSpawned": false,
				"MathDontShowMax": false,
				"MathZero": false
			}
		]
	},
	{
		"Name": <Next Item...>
	}
]
```

## Admin privileges
Privilege | Description
--- | ---
`entwatch:reload` | Allows you to reload the plugin settings and view them
`entwatch:chat` | Allows you to view messages about item selection in team mode
`entwatch:hud` | Allows you to display items in command mode
`entwatch:ban` | Allows access to bans (Command)
`entwatch:banperm` | Allows access to permanent bans (Duration 0)
`entwatch:banlong` | Allows access to long bans (Cvar ewc_banlong)
`entwatch:unban` | Allows access to unbans (Command)
`entwatch:unbanperm` | Allows access to permanent unbans (Duration 0)
`entwatch:unbanother` | Allows access to unbans from other admins
`entwatch:transfer` | Allows transfer of items
`entwatch:spawn` | Allows items to spawn

## CVARs
Cvar | Parameters | Description
--- | --- | ---
`ms_ewc_teamonly` | `<false-true>` | Enable/Disable team only mode. (Default true)
`ms_ewc_adminchat` | `<0-2>` | Change Admin Chat Mode (0 - All Messages, 1 - Only Pickup/Drop Items, 2 - Nothing). (Default 0)
`ms_ewc_adminhud` | `<0-2>` | Change Admin Hud Mode (0 - All Items, 1 - Only Item Name, 2 - Nothing). (Default 0)
`ms_ewc_player_format` | `<0-3>` | Changes the way player information is displayed by default (0 - Only Nickname, 1 - Nickname and UserID, 2 - Nickname and SteamID, 3 - Nickname, UserID and SteamID). (Default 3)
`ms_ewc_blockepick` | `<false-true>` | Block players from using E key to grab items. (Default true)
`ms_ewc_delay_use` | `<0.0-60.0>` | Change delay before use. (Default 1.0)
`ms_ewc_globalblock` | `<false-true>` | Blocks the pickup of any items by players. (Default false)
`ms_ewc_display_ability` | `<0-4>` | Count of the abilities to display on the HUD. (Default 4)
`ms_ewc_use_priority` | `<false-true>` | Enable/Disable forced pressing of the button. (Default true)
`ms_ewc_display_mapcommands` | `<false-true>` | Enable/Disable display of item changes. (Default true)
`ms_ewc_scheme_name` | `<string>` | Filename for the scheme. (Default default.json)
`ms_ewc_lower_mapname` | `<false-true>` | Automatically lowercase map name. (Default false)
`ms_ewc_glow_spawn` | `<false-true> `| Enable/Disable the glow after Spawn Items. (Default false)
`ms_ewc_glow_particle` | `<false-true>` | Enable/Disable the glow using a particle. (Default false)
`ms_ewc_glow_prop` | `<false-true>` | Enable/Disable the glow using a prop_dynamic. (Default true)
`ms_ewc_glow_vip` | `<false-true>` | Enable/Disable the glow for privileged users. (Default false)
`ms_ewc_bantime` | `<0-43200>` | Default ban time. 0 - Permanent. (Default 0)
`ms_ewc_banlong` | `<1-1440000>` | Max ban time with once @css/ew_ban privilege. (Default 720)
`ms_ewc_banreason` | `<string>` | Default ban reason. (Default Trolling)
`ms_ewc_unbanreason` | `<string>` | Default unban reason. (Default Giving another chance)
`ms_ewc_keep_expired_ban` | `<false-true>` | Enable/Disable keep expired bans. (Default true)
`ms_ewc_offline_clear_time` | `<1-240>` | Time during which data is stored. (Default 30)
`ms_ewc_clantag` | `<false-true>` | Enable/Disable to display in the ClanTag. (Default true)
`ms_ewc_clantag_info` | `<false-true>` | Enable/Disable to display cooldown and other in the ClanTag. (Default true)
`ms_ewc_endround_remove` | `<false-true>` | Enable/Disable to remove weapons after the end of the round. (Default true)
`ms_ewc_server_lang` | `<string>` | Specify the language into which the server messages should be translated. (Default en-us)

## Commands
Client Command | Description
--- | ---
`ms_hud` | Allows the player to switch the HUD (0 - Disabled, 1 - Center, 2 - Alert, 3 - WorldText)
`ms_hudpos` | Allows the player to change the position of the HUD {X Y Z} (default: -8 2 7; min -200,0; max 200,0)
`ms_hudsize` | Allows the player to change the size of the HUD {size} (default: 54; min 16; max 128)
`ms_hudrefresh` | Allows the player to change the time it takes to scroll through the list {sec} (default: 3; min 1; max 10)
`ms_hudsheet` | Allows the player to change the number of items on the sheet {count} (default: 5; min 1; max 15)
`ms_epf` | Allows the player to change the player display format (0 - Only Nickname, 1 - Nickname and UserID, 2 - Nickname and SteamID, 3 - Nickname, UserID and SteamID)
`ms_eup` | Allows the player to use UsePriority {bool}
`ms_estatus` | Allows the player to view the restrictions {null/target}

## Admin's commands
Admin Command | Privilege | Description
--- | --- | ---
`ms_ereload` | `ew_reload` | Reloads config and Scheme
`ms_eshowitems` | `ew_reload` | Shows a list of spawned items
`ms_eshowscheme` | `ew_reload` | Shows the scheme
`ms_eban` | `ew_ban`+`ew_ban_perm`+`ew_ban_long` | Allows the admin to restrict items for the player `<#userid/name> [<time>] [<reason>]`
`ms_eunban` | `ew_unban`+`ew_unban_perm`+`ew_unban_other` | Allows the admin to remove the item restriction for a player `<#userid/name> [<reason>]`
`ms_ebanlist` | `ew_ban` | Displays a list of restrictions
`ms_etransfer` | `ew_transfer` | Allows the admin to transfer items `<owner>/$<itemname> <receiver>`
`ms_espawn` | `ew_spawn` | Allows the admin to spawn items `<itemname> <receiver> [<strip>]`
`ms_elist` | `ew_ban` | Shows a list of players including those who have disconnected

## Mapper's Commands
Map Command | Variables | Description
--- | --- | ---
`ew_setcooldown` | `<int hammerid> <int buttonid> <int new cooldown> [<bool force apply>]` | Allows you to change the item’s cooldown during the game
`ew_setmaxuses` | `<int hammerid> <int buttonid> <int maxuses> [<bool even if over>]` | Allows you to change the maximum use of the item during the game, depending on whether the item was used to the end
`ew_setuses` | `<int hammerid> <int buttonid> <int value> [<bool even if over>]` | Allows you to change the current use of the item during the game, depending on whether the item was used to the end
`ew_addmaxuses` | `<int hammerid> <int buttonid> [<bool even if over>]` | Allows you to add 1 charge to the item, depending on whether the item was used to the end
`ew_setmode` | `<int hammerid> <int buttonid> <int newmode> <int cooldown> <int maxuses> [<bool even if over>]` | Allows you to completely change the item
`ew_lockbutton` | `<int hammerid> <int buttonid> <bool value>` | Allows to lock item
`ew_setabilityname` | `<int hammerid> <int buttonid> <string newname>` | Allows you to change the ability’s name
`ew_setname` | `<int hammerid> <string newname>` | Allows you to change the item’s name(Chat)
`ew_setshortname` | `<int hammerid> <string newshortname>` | Allows you to change the item’s shortname(HUD)
`ew_block` | `<int hammerid> <bool value>` | Allows you to block an item during the game. Similar to the 'blockpickup' property

PS:<br>
...The values ​​of the int must be greater than or equal to 0<br>
...Mode values ​​must be between 0 and 8<br>
...String values ​​must not be empty/null

## Example of SQL-request for correct use of STEAMID after CS:GO/EntWatchSharp below version 0.DZ.7.beta
```
UPDATE entwatch_current_eban
SET client_steamid = REPLACE(client_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE client_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_current_eban
SET admin_steamid = REPLACE(admin_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_current_eban
SET admin_steamid_unban = REPLACE(admin_steamid_unban, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid_unban LIKE '%STEAM_1:%';

UPDATE entwatch_old_eban
SET client_steamid = REPLACE(client_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE client_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_old_eban
SET admin_steamid = REPLACE(admin_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_old_eban
SET admin_steamid_unban = REPLACE(admin_steamid_unban, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid_unban LIKE '%STEAM_1:%';
```

## Example of changing configs in Notepad++ for correct use after EntWatchSharp above version 1.DZ.4
-Find what: `("HammerID": |"TriggerID": |"SpawnerID": |"ButtonID": |"MathID": )(-\d+|\d+)`<br>
-Replace with: `$1"$2"`<br>
-Search Mode: Regular expression<br>
after<br>
-Find what: `("HammerID": |"TriggerID": |"SpawnerID": |"ButtonID": |"MathID": )("0")`<br>
-Replace with: `$1""`<br>
-Search Mode: Regular expression<br>

## Future plans
1. Fixes Errors
2. GameHUD Annoncer