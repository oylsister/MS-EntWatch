using MS_EntWatch.Items;
using Sharp.Shared.Enums;
using Sharp.Shared.Objects;
using System.Globalization;

namespace MS_EntWatch.Helpers
{
    static class UI
    {
        public static void EWChatActivity(string sMessage, string sColor, Item ItemTest, IGameClient client, Ability? AbilityTest = null)
        {
            string[] sPlayerInfoFormat = PlayerInfoFormat(client);
            PrintToConsole(ReplaceColorTags(LocalizerFormat(CultureInfo.GetCultureInfo(Cvar.ServerLanguage), sMessage, sPlayerInfoFormat[3], "", "", ItemTest.Name, (AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : ""), false));

            Task.Run(() =>
            {
                LogManager.ItemAction(sMessage, ReplaceColorTags(sPlayerInfoFormat[3]), "", "", ItemTest.Name, (AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "");
            });

            if (!(AbilityTest == null || ItemTest.Chat || AbilityTest.Chat_Uses)) return;


            foreach (var pair in EW.g_EWPlayer)
            {
                if (pair.Key is { IsValid: true, IsFakeClient: false, IsHltv: false } cl && cl.GetPlayerController() is { } player)
                {
                    if (Cvar.TeamOnly && player.Team > CStrikeTeam.Spectator && ItemTest.Team != player.Team && (!EntWatch.AdminCommands_CheckPermission(cl, EntWatch.PermissionChat) || Cvar.AdminChat == 2 || (Cvar.AdminChat == 1 && AbilityTest != null))) continue;

                    ReplyToCommand(cl, sMessage, true, PlayerInfo(cl, sPlayerInfoFormat), sColor, ItemTest.Color, ItemTest.Name, (AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "");
                }
            }
        }
        public static void EWSysInfo(string sMessage, int iColor = 15, params object[] arg)
        {
            PrintToConsole(LocalizerFormat(CultureInfo.GetCultureInfo(Cvar.ServerLanguage), ReplaceColorTags(sMessage, false), arg), iColor);

            Task.Run(() =>
            {
                LogManager.SystemAction(ReplaceColorTags(sMessage, false), true, arg);
            });
        }

        public static void EWSysInfoServerInit(string sMessage, int iColor = 15, params object[] arg)
        {
            PrintToConsole(LocalizerFormat(CultureInfo.GetCultureInfo(Cvar.ServerLanguage), ReplaceColorTags(sMessage, false), arg), iColor);

            Task.Run(() =>
            {
                LogManager.SystemAction(ReplaceColorTags(sMessage, false), false, arg);
            });
        }

        public static void EWAdminInfo(string sMessage, params object[] arg)
        {
            PrintToConsole(LocalizerFormat(CultureInfo.GetCultureInfo(Cvar.ServerLanguage), ReplaceColorTags(sMessage, false), arg), 2);

            Task.Run(() =>
            {
                LogManager.AdminAction(ReplaceColorTags(sMessage, false), arg);
            });
        }

        public static void EWChatAdminBan(string[] sPIF_admin, string[] sPIF_player, string sReason, bool bAction)
        {
            EWAdminInfo(bAction ? "EntWatch.Chat.Admin.Restricted" : "EntWatch.Chat.Admin.Unrestricted", "", ReplaceColorTags(sPIF_admin[3], false), "", ReplaceColorTags(sPIF_player[3], false));
            EWAdminInfo("EntWatch.Chat.Admin.Reason", "", sReason);

            if (EW.g_Scheme == null) return;

            foreach (var pair in EW.g_EWPlayer.ToList())
            {
                ReplyToCommand(pair.Key, bAction ? "EntWatch.Chat.Admin.Restricted" : "EntWatch.Chat.Admin.Unrestricted", true, EW.g_Scheme.Color_warning, PlayerInfo(pair.Key, sPIF_admin), bAction ? EW.g_Scheme.Color_disabled : EW.g_Scheme.Color_enabled, PlayerInfo(pair.Key, sPIF_player));
                ReplyToCommand(pair.Key, "EntWatch.Chat.Admin.Reason", true, EW.g_Scheme.Color_warning, sReason);
            }
        }

        public static void EWChatAdminTransfer(string[] sPIF_admin, string[] sPIF_receiver, string sItem, string[] sPIF_target)
        {
            EWAdminInfo("EntWatch.Reply.Transfer.Notify", ReplaceColorTags(sPIF_admin[3], false), ReplaceColorTags(sItem, false), ReplaceColorTags(sPIF_target[3], false), ReplaceColorTags(sPIF_receiver[3], false));

            foreach (var pair in EW.g_EWPlayer.ToList())
            {
                ReplyToCommand(pair.Key, "EntWatch.Reply.Transfer.Notify", true, PlayerInfo(pair.Key, sPIF_admin), sItem, PlayerInfo(pair.Key, sPIF_target), PlayerInfo(pair.Key, sPIF_receiver));
            }
        }

        public static void EWChatAdminSpawn(string[] sPIF_admin, string[] sPIF_receiver, string sItem)
        {
            EWAdminInfo("EntWatch.Reply.Spawn.Notify", ReplaceColorTags(sPIF_admin[3], false), ReplaceColorTags(sItem, false), ReplaceColorTags(sPIF_receiver[3], false));

            foreach (var pair in EW.g_EWPlayer.ToList())
            {
                ReplyToCommand(pair.Key, "EntWatch.Reply.Spawn.Notify", true, PlayerInfo(pair.Key, sPIF_admin), sItem, PlayerInfo(pair.Key, sPIF_receiver));
            }
        }

        public static void CvarChangeNotify(string sCvarName, string sCvarValue, bool bClientNotify)
        {
            PrintToConsole(LocalizerFormat(CultureInfo.GetCultureInfo(Cvar.ServerLanguage), "EntWatch.Cvar.Notify", [ sCvarName, sCvarValue ]), 3);

            Task.Run(() =>
            {
                LogManager.CvarAction(sCvarName, sCvarValue);
            });

            Task.Run(() =>
            {
                if (bClientNotify && EW.g_Scheme != null)
                {
                    foreach (var pair in EW.g_EWPlayer)
                    {
                        if (pair.Key is { } client) ReplyToCommand(client, "EntWatch.Cvar.Notify.Clients", true, [EW.g_Scheme.Color_warning, EW.g_Scheme.Color_name, sCvarName, sCvarValue]);
                    }
                }
            });
        }

        public static void ReplyToCommand(IGameClient client, string sMessage, bool bChat, params object[] arg)
        {
            if (client is { IsValid: true, IsFakeClient: false, IsHltv: false } && client.GetPlayerController() is { } player && EntWatch.GetLocalizer() is { } lm)
            {
                var localizer = lm.For(client);
                player.Print(bChat ? HudPrintChannel.Chat : HudPrintChannel.Console, ReplaceColorTags($"{Tag()}{localizer.Text(sMessage, arg)}", bChat));
            }
        }

        public static string LocalizerFormat(CultureInfo culture, string key, params ReadOnlySpan<object?> param)
        {
            if (EntWatch.GetLocalizer() is { } lm) return lm.Format(culture, key, param);
            return "";
        }

        public static string PlayerInfo(IGameClient? client, string[] sPlayerInfoFormat)
        {
            if (client != null)
            {
                if (EW.g_EWPlayer[client].PFormatPlayer < 0 || EW.g_EWPlayer[client].PFormatPlayer > 3) return sPlayerInfoFormat[Cvar.PlayerFormat];
                return sPlayerInfoFormat[EW.g_EWPlayer[client].PFormatPlayer];
            }
            return sPlayerInfoFormat[3];
        }
        public static string[] PlayerInfoFormat(IGameClient client)
        {
            if (client != null)
            {
                string[] sResult = new string[4];
                sResult[0] = $"{EW.g_Scheme?.Color_name}{ReplaceSpecial(client.Name)}{EW.g_Scheme?.Color_warning}";
                sResult[1] = $"{sResult[0]}[{EW.g_Scheme?.Color_steamid}#{client.UserId}{EW.g_Scheme?.Color_warning}]";
                sResult[2] = $"{sResult[0]}[{EW.g_Scheme?.Color_steamid}#{EW.ConvertSteamID64ToSteamID(client.SteamId.ToString())}{EW.g_Scheme?.Color_warning}]";
                sResult[3] = $"{sResult[0]}[{EW.g_Scheme?.Color_steamid}#{client.UserId}{EW.g_Scheme?.Color_warning}|{EW.g_Scheme?.Color_steamid}#{EW.ConvertSteamID64ToSteamID(client.SteamId.ToString())}{EW.g_Scheme?.Color_warning}]";
                return sResult;
            }
            return PlayerInfoFormat("Console", "Server");
        }
        public static string[] PlayerInfoFormat(string sName, string sSteamID)
        {
            string[] sResult = new string[4];
            sResult[0] = $"{EW.g_Scheme?.Color_name}{ReplaceSpecial(sName)}{EW.g_Scheme?.Color_warning}";
            sResult[1] = sResult[0];
            sResult[2] = $"{EW.g_Scheme?.Color_name}{ReplaceSpecial(sName)}{EW.g_Scheme?.Color_warning}[{EW.g_Scheme?.Color_steamid}{sSteamID}{EW.g_Scheme?.Color_warning}]";
            sResult[3] = sResult[2];
            return sResult;
        }

        public static string Tag()
        {
            return $" {EW.g_Scheme?.Color_tag} [EntWatch] ";
        }

        public static void PrintToConsole(string sMessage, int iColor = 1)
        {
            Console.ForegroundColor = (ConsoleColor)8;
            Console.Write("[");
            Console.ForegroundColor = (ConsoleColor)6;
            Console.Write("EntWatch");
            Console.ForegroundColor = (ConsoleColor)8;
            Console.Write("] ");
            Console.ForegroundColor = (ConsoleColor)iColor;
            Console.WriteLine(sMessage, false);
            Console.ResetColor();
            /* Colors:
				* 0 - No color		1 - White		2 - Red-Orange		3 - Orange
				* 4 - Yellow		5 - Dark Green	6 - Green			7 - Light Green
				* 8 - Cyan			9 - Sky			10 - Light Blue		11 - Blue
				* 12 - Violet		13 - Pink		14 - Light Red		15 - Red */
        }

        public static string ReplaceSpecial(string input)
        {
            input = input.Replace("{", "[");
            input = input.Replace("}", "]");

            return input;
        }

        public static string ReplaceColorTags(string input, bool bChat = true)
        {
            for (var i = 0; i < colorPatterns.Length; i++)
                input = input.Replace(colorPatterns[i], bChat ? colorReplacements[i] : "");

            return input;
        }

        readonly static string[] colorPatterns =
        [
            "{default}", "{darkred}", "{purple}", "{green}", "{lightgreen}", "{lime}", "{red}", "{grey}", "{team}", "{red2}",
            "{olive}", "{a}", "{lightblue}", "{blue}", "{d}", "{pink}", "{darkorange}", "{orange}", "{darkblue}", "{gold}",
            "{white}", "{yellow}", "{magenta}", "{silver}", "{bluegrey}", "{lightred}", "{cyan}", "{gray}", "{lightyellow}",
        ];
        readonly static string[] colorReplacements =
        [
            "\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08", "\x03", "\x0F",
            "\x06", "\x0A", "\x0B", "\x0C", "\x0D", "\x0E", "\x0F", "\x10", "\x0C", "\x10",
            "\x01", "\x09", "\x0E", "\x0A", "\x0D", "\x0F", "\x03", "\x08", "\x06"
        ];
    }
}
