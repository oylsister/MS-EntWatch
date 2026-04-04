using MS_EntWatch.Helpers;
using MS_EntWatch.Items;
using MS_EntWatch.Modules;
using MS_EntWatch.Modules.Eban;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using static MS_EntWatch.Modules.Eban.EbanDB;

namespace MS_EntWatch
{
    public partial class EntWatch
    {
        private const string PermissionReload = "entwatch:reload";
        private const string PermissionBan = "entwatch:ban";
        private const string PermissionUnban = "entwatch:unban";
        private const string PermissionTransfer = "entwatch:transfer";
        private const string PermissionSpawn = "entwatch:spawn";
        private const string PermissionBanPerm = "entwatch:banperm";
        private const string PermissionBanLong = "entwatch:banlong";
        private const string PermissionUnbanPerm = "entwatch:unbanperm";
        private const string PermissionUnbanOther = "entwatch:unbanother";
        public const string PermissionChat = "entwatch:chat";
        public const string PermissionHUD = "entwatch:hud";

        private void AdminCommands_InitializePermissions()
        {
            if (_adminManager?.Instance is not { } adminManager || _AMInit) return;

            try
            {
                var registry = adminManager.GetCommandRegistry(DisplayName);

                registry.RegisterPermissions([PermissionReload, PermissionBan, PermissionUnban, PermissionTransfer, PermissionSpawn, PermissionBanPerm, PermissionBanLong, PermissionUnbanPerm, PermissionUnbanOther, PermissionChat, PermissionHUD]);
                registry.RegisterAdminCommand("ereload", OnEWReload, [PermissionReload]);
                registry.RegisterAdminCommand("eshowitems", OnEWShow, [PermissionReload]);
                registry.RegisterAdminCommand("eshowscheme", OnEWScheme, [PermissionReload]);
                registry.RegisterAdminCommand("eban", OnEWBan, [PermissionBan]);
                registry.RegisterAdminCommand("eunban", OnEWUnBan, [PermissionUnban]);
                registry.RegisterAdminCommand("ebanlist", OnEWBanList, [PermissionBan]);
                registry.RegisterAdminCommand("elist", OnEWList, [PermissionBan]);
                registry.RegisterAdminCommand("etransfer", OnEWTransfer, [PermissionTransfer]);
                registry.RegisterAdminCommand("espawn", OnEWSpawn, [PermissionSpawn]);

                _AMInit = true;
            }
            catch (InvalidOperationException) { }
            catch (Exception e)
            {
                UI.EWSysInfo("EntWatch.Info.Error", 15, "Failed to initialize admin permissions.");
                UI.EWSysInfo("EntWatch.Info.Error", 15, $"{e.Message}");
            }
        }

        private static Sharp.Modules.AdminManager.Shared.IAdmin? AdminCommands_GetAdmin(IGameClient client)
        {
            if (_adminManager?.Instance is not { } adminManager || !_AMInit) return null;
            return adminManager.GetAdmin(client.SteamId);
        }

        public static bool AdminCommands_CheckPermission(IGameClient client, string permission)
        {
            if (AdminCommands_GetAdmin(client) is not { } admin) return false;
            
            return admin.HasPermission(permission);
        }

        public static byte AdminCommands_GetPlayerImmunity(IGameClient client)
        {
            if (AdminCommands_GetAdmin(client) is not { } admin) return 0;

            return admin.Immunity;
        }

        private void OnEWReload(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid) return;

            EW.CleanData();
            EW.LoadScheme();
            EW.LoadConfig();
            UI.ReplyToCommand(client, "EntWatch.Reply.Reload_configs", command.ChatTrigger, EW.g_Scheme != null ? EW.g_Scheme.Color_warning : "");
        }

        private void OnEWShow(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || !EW.g_CfgLoaded || EW.g_Scheme == null) return;

            foreach (Item ItemTest in EW.g_ItemList.ToList())
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.ShowItem.Main", command.ChatTrigger, EW.g_Scheme.Color_warning, EW.g_Scheme.Color_tag, ItemTest.Name, ItemTest.WeaponHandle.Index, (ItemTest.Owner != null && ItemTest.Owner.GetPlayerController() is { } player) ? player.PlayerName : "Null");
                UI.ReplyToCommand(client, "EntWatch.Reply.ShowItem.Buttons", command.ChatTrigger, EW.g_Scheme.Color_warning);
                foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
                {
                    if (AbilityTest.Entity != null)
                    {
                        string sMathID = "";
                        if (!string.IsNullOrEmpty(AbilityTest.MathID) && !string.Equals(AbilityTest.MathID, "0")) sMathID = $" {EW.g_Scheme.Color_warning}MathID: {EW.g_Scheme.Color_tag}{AbilityTest.MathID}";
                        if (AbilityTest.MathCounter != null) sMathID += $" {EW.g_Scheme.Color_warning}MathCounterID: {EW.g_Scheme.Color_tag}{AbilityTest.MathCounter.Index}";

                        UI.ReplyToCommand(client, "EntWatch.Reply.ShowItem.Ability", command.ChatTrigger, EW.g_Scheme.Color_warning, EW.g_Scheme.Color_tag, AbilityTest.Entity.Index, AbilityTest.Name, AbilityTest.ButtonID, AbilityTest.ButtonClass, AbilityTest.Chat_Uses, AbilityTest.Mode, AbilityTest.MaxUses, AbilityTest.CoolDown, sMathID);
                    }
                }
                UI.ReplyToCommand(client, "EntWatch.Reply.NullString", command.ChatTrigger);
            }
        }

        private void OnEWScheme(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || EW.g_Scheme == null) return;

            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_tag", EW.g_Scheme.Color_tag, UI.ReplaceSpecial(EW.g_Scheme.Color_tag));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_name", EW.g_Scheme.Color_name, UI.ReplaceSpecial(EW.g_Scheme.Color_name));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_steamid", EW.g_Scheme.Color_steamid, UI.ReplaceSpecial(EW.g_Scheme.Color_steamid));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_use", EW.g_Scheme.Color_use, UI.ReplaceSpecial(EW.g_Scheme.Color_use));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_pickup", EW.g_Scheme.Color_pickup, UI.ReplaceSpecial(EW.g_Scheme.Color_pickup));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_drop", EW.g_Scheme.Color_drop, UI.ReplaceSpecial(EW.g_Scheme.Color_drop));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_disconnect", EW.g_Scheme.Color_disconnect, UI.ReplaceSpecial(EW.g_Scheme.Color_disconnect));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_death", EW.g_Scheme.Color_death, UI.ReplaceSpecial(EW.g_Scheme.Color_death));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_warning", EW.g_Scheme.Color_warning, UI.ReplaceSpecial(EW.g_Scheme.Color_warning));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_enabled", EW.g_Scheme.Color_enabled, UI.ReplaceSpecial(EW.g_Scheme.Color_enabled));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Color_disabled", EW.g_Scheme.Color_disabled, UI.ReplaceSpecial(EW.g_Scheme.Color_disabled));
            UI.ReplyToCommand(client, "EntWatch.Reply.ShowScheme", command.ChatTrigger, EW.g_Scheme.Color_warning, "Server_name", EW.g_Scheme.Color_tag, UI.ReplaceSpecial(EW.g_Scheme.Server_name));
        }

        private void OnEWBan(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || EW.g_Scheme == null) return;

            int iArgNeed = 1;
            string sArgHelper = "<#userid|name|#steamid> [<time>] [<reason>]";
            if (command.ArgCount < iArgNeed)
            {
                UI.ReplyToCommand(client, "EntWatch.Info.Error.MinArg", command.ChatTrigger, iArgNeed, command.CommandName, sArgHelper);
                return;
            }

            var players = TargetManager.Find(client, command.GetArg(1));

            OfflineBan? target = null;

            if (players.Count > 0)
            {
                //One Target
                IGameClient clientOnline = players.Single();
                if (AdminCommands_GetPlayerImmunity(client) < AdminCommands_GetPlayerImmunity(clientOnline))
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.You_cannot_target", command.ChatTrigger, EW.g_Scheme.Color_disabled);
                    return;
                }

                if (!EW.CheckDictionary(clientOnline))
                {
                    UI.ReplyToCommand(client, "EntWatch.Info.Error.NotFoundInDictionary", command.ChatTrigger);
                    return;
                }

                if (EW.g_EWPlayer[clientOnline].BannedPlayer.bBanned)
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Has_a_Restrict", command.ChatTrigger, UI.PlayerInfo(client, UI.PlayerInfoFormat(clientOnline)), EW.g_Scheme.Color_disabled);
                    return;
                }
                foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
                {
                    if (OfflineTest.UserID == clientOnline.UserId)
                    {
                        target = OfflineTest;
                        break;
                    }
                }
            }
            else
            {
                target = OfflineFunc.FindTarget(client, command.GetArg(1), command.ChatTrigger);
            }

            if (target == null)
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.No_matching_client", command.ChatTrigger, EW.g_Scheme.Color_warning);
                return;
            }

            int time = Cvar.BanTime;
            if (command.ArgCount >= 2)
            {
                if (!int.TryParse(command.GetArg(2), out int timeparse))
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Must_be_an_integer", command.ChatTrigger, EW.g_Scheme.Color_warning);
                    return;
                }
                time = timeparse;
            }

            if (time == 0 && !AdminCommands_CheckPermission(client, PermissionBanPerm))
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Access.Permanent", command.ChatTrigger, EW.g_Scheme.Color_warning);
                return;
            }

            if (time > Cvar.BanLong && !AdminCommands_CheckPermission(client, PermissionBanLong))
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Access.Long", command.ChatTrigger, EW.g_Scheme.Color_warning, Cvar.BanLong);
                return;
            }

            string reason = command.ArgCount >= 3 ? command.GetArg(3) : "";
            if (string.IsNullOrEmpty(reason)) reason = Cvar.BanReason;

            EbanPlayer ebanPlayer = (target.Online && target.Player != null) ? EW.g_EWPlayer[target.Player].BannedPlayer : new EbanPlayer();

            string? sSteamID = EW.ConvertSteamID64ToSteamID(client.SteamId.ToString());

            if (ebanPlayer.SetBan(UI.ReplaceSpecial(client.Name), !string.IsNullOrEmpty(sSteamID) ? sSteamID : "SERVER", UI.ReplaceSpecial(target.Name), target.SteamID, time, reason))
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Ban.Success", command.ChatTrigger, EW.g_Scheme.Color_warning);
            else
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Ban.Failed", command.ChatTrigger, EW.g_Scheme.Color_warning);
                return;
            }

            UI.EWChatAdminBan(UI.PlayerInfoFormat(client), target.Online && target.Player != null ? UI.PlayerInfoFormat(target.Player) : UI.PlayerInfoFormat(target.Name, target.SteamID), reason, true);
        }

        private void OnEWUnBan(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || EW.g_Scheme == null) return;

            int iArgNeed = 1;
            string sArgHelper = "<#userid|name|#steamid> [<reason>]";
            if (command.ArgCount < iArgNeed)
            {
                UI.ReplyToCommand(client, "EntWatch.Info.Error.MinArg", command.ChatTrigger, iArgNeed, command.CommandName, sArgHelper);
                return;
            }

            var players = TargetManager.Find(client, command.GetArg(1));

            EbanPlayer target = new();
            string sTarget = command.GetArg(1);

            bool bOnline = players.Count > 0;

            if (bOnline)
            {
                IGameClient clientOnline = players.Single();

                if (AdminCommands_GetPlayerImmunity(client) < AdminCommands_GetPlayerImmunity(clientOnline))
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.You_cannot_target", command.ChatTrigger, EW.g_Scheme.Color_disabled);
                    return;
                }

                if (!EW.CheckDictionary(clientOnline))
                {
                    UI.ReplyToCommand(client, "EntWatch.Info.Error.NotFoundInDictionary", command.ChatTrigger);
                    return;
                }

                target.bBanned = EW.g_EWPlayer[clientOnline].BannedPlayer.bBanned;
                target.sAdminName = EW.g_EWPlayer[clientOnline].BannedPlayer.sAdminSteamID;
                target.sAdminSteamID = EW.g_EWPlayer[clientOnline].BannedPlayer.sAdminSteamID;
                target.iDuration = EW.g_EWPlayer[clientOnline].BannedPlayer.iDuration;
                target.iTimeStamp_Issued = EW.g_EWPlayer[clientOnline].BannedPlayer.iTimeStamp_Issued;
                target.sReason = EW.g_EWPlayer[clientOnline].BannedPlayer.sReason;
                target.sClientName = UI.ReplaceSpecial(clientOnline.Name);
                string? sSteamID = EW.ConvertSteamID64ToSteamID(clientOnline.SteamId.ToString());
                if (string.IsNullOrEmpty(sSteamID))
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.InvalidSteamID", command.ChatTrigger, EW.g_Scheme.Color_disabled, EW.g_Scheme.Color_name, UI.ReplaceSpecial(clientOnline.Name));
                    return;
                }
                target.sClientSteamID = sSteamID;
            }

            string reason = command.ArgCount >= 2 ? command.GetArg(2) : "";
            if (string.IsNullOrEmpty(reason)) reason = Cvar.UnBanReason;

            if (bOnline) UnBanComm(client, players.Single(), target, reason, command.ChatTrigger);
            else if (sTarget.StartsWith("#steam_", StringComparison.OrdinalIgnoreCase))
            {
                EbanPlayer.GetBan(sTarget[1..], client, reason, command.ChatTrigger, GetBanComm_Handler);
            }
            else UI.ReplyToCommand(client, "EntWatch.Reply.No_matching_client", command.ChatTrigger, EW.g_Scheme.Color_warning);
        }

        readonly GetBanCommFunc GetBanComm_Handler = (sClientSteamID, client, reason, bChat, DBQuery_Result) =>
        {
            if (DBQuery_Result.Count > 0)
            {
                EbanPlayer target = new()
                {
                    bBanned = true,
                    sAdminName = DBQuery_Result[0][0],
                    sAdminSteamID = DBQuery_Result[0][1],
                    iDuration = Convert.ToInt32(DBQuery_Result[0][2]),
                    iTimeStamp_Issued = Convert.ToInt32(DBQuery_Result[0][3]),
                    sReason = DBQuery_Result[0][4],
                    sClientName = DBQuery_Result[0][5],
                    sClientSteamID = sClientSteamID
                };
                UnBanComm(client, null, target, reason, bChat);
                return;
            }
            UnBanComm(client, null, null, reason, bChat);
        };

        static void UnBanComm(IGameClient client, IGameClient? player, EbanPlayer? target, string reason, bool bChat)
        {
            if (EW.g_Scheme == null) return;
            if (target == null)
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.No_matching_client", bChat, EW.g_Scheme.Color_warning);
                return;
            }

            if (!target.bBanned)
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Can_pickup", bChat, UI.PlayerInfo(client, UI.PlayerInfoFormat(target.sClientName, target.sClientSteamID)), EW.g_Scheme.Color_enabled);
                return;
            }
            
            if (target.iDuration == 0 && !AdminCommands_CheckPermission(client, PermissionUnbanPerm))
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Access.UnPermanent", bChat, EW.g_Scheme.Color_warning);
                return;
            }

            string? sSteamID = EW.ConvertSteamID64ToSteamID(client.SteamId.ToString());
            if (!string.Equals(target.sAdminSteamID, !string.IsNullOrEmpty(sSteamID) ? sSteamID : "SERVER") && AdminCommands_CheckPermission(client, PermissionUnbanOther))
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Access.Other", bChat, EW.g_Scheme.Color_warning);
                return;
            }

            if (target.UnBan(UI.ReplaceSpecial(client.Name), !string.IsNullOrEmpty(sSteamID) ? sSteamID : "SERVER", target.sClientSteamID, reason))
            {
                if (player != null) EW.g_EWPlayer[player].BannedPlayer.bBanned = false;
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.UnBan.Success", bChat, EW.g_Scheme.Color_warning);
            }
            else
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Eban.UnBan.Failed", bChat, EW.g_Scheme.Color_warning);
                return;
            }

            UI.EWChatAdminBan(UI.PlayerInfoFormat(client), UI.PlayerInfoFormat(target.sClientName, target.sClientSteamID), reason, false);
        }

        private void OnEWBanList(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || EW.g_Scheme == null) return;

            UI.ReplyToCommand(client, "EntWatch.Reply.Eban.List", command.ChatTrigger, EW.g_Scheme.Color_warning);

            int iCount = 0;
            foreach (var pair in EW.g_EWPlayer.ToList())
            {
                if (pair.Value.BannedPlayer.bBanned)
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Player", command.ChatTrigger, EW.g_Scheme.Color_warning, UI.PlayerInfo(client, UI.PlayerInfoFormat(pair.Key)));
                    UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Admin", command.ChatTrigger, EW.g_Scheme.Color_warning, UI.PlayerInfo(client, UI.PlayerInfoFormat(pair.Value.BannedPlayer.sAdminName, pair.Value.BannedPlayer.sAdminSteamID)));
                    switch (pair.Value.BannedPlayer.iDuration)
                    {
                        case -1: UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Duration.Temporary", command.ChatTrigger, EW.g_Scheme.Color_warning, EW.g_Scheme.Color_enabled); break;
                        case 0: UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Duration.Permanently", command.ChatTrigger, EW.g_Scheme.Color_warning, EW.g_Scheme.Color_disabled); break;
                        default: UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Duration.Minutes", command.ChatTrigger, EW.g_Scheme.Color_warning, EW.g_Scheme.Color_disabled, pair.Value.BannedPlayer.iDuration); break;
                    }
                    UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Expires", command.ChatTrigger, EW.g_Scheme.Color_warning, EW.g_Scheme.Color_disabled, DateTimeOffset.FromUnixTimeSeconds(pair.Value.BannedPlayer.iTimeStamp_Issued));
                    UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Reason", command.ChatTrigger, EW.g_Scheme.Color_warning, EW.g_Scheme.Color_disabled, pair.Value.BannedPlayer.sReason);
                    UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Separator", command.ChatTrigger, EW.g_Scheme.Color_warning);
                    iCount++;
                }
            }
            if (iCount == 0) UI.ReplyToCommand(client, "EntWatch.Reply.Eban.NoPlayers", command.ChatTrigger, EW.g_Scheme.Color_warning);
        }

        private void OnEWList(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || EW.g_Scheme == null) return;

            UI.ReplyToCommand(client, "EntWatch.Reply.Offline.Info", command.ChatTrigger, EW.g_Scheme.Color_warning);

            int iCount = 0;
            double CurrentTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
            {
                iCount++;
                if (OfflineTest.Online)
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Offline.OnServer", command.ChatTrigger, EW.g_Scheme.Color_warning, iCount, OfflineTest.Name, OfflineTest.UserID, OfflineTest.SteamID, !string.IsNullOrEmpty(OfflineTest.LastItem) ? OfflineTest.LastItem : "-");
                }
                else
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Offline.Leave", command.ChatTrigger, EW.g_Scheme.Color_warning, iCount, OfflineTest.Name, OfflineTest.UserID, OfflineTest.SteamID, !string.IsNullOrEmpty(OfflineTest.LastItem) ? OfflineTest.LastItem : "-", (int)((CurrentTime - OfflineTest.TimeStamp_Start) / 60));
                }
            }
        }

        private void OnEWTransfer(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || !EW.g_CfgLoaded || EW.g_Scheme == null) return;

            int iArgNeed = 2;
            string sArgHelper = "<owner>/$<itemname> <receiver>";
            if (command.ArgCount < iArgNeed)
            {
                UI.ReplyToCommand(client, "EntWatch.Info.Error.MinArg", command.ChatTrigger, iArgNeed, command.CommandName, sArgHelper);
                return;
            }

            string sItemName = command.GetArg(1);
            IGameClient? target = null;
            Item? item = null;
            if (sItemName[0] == '$')
            {
                sItemName = sItemName[1..];
                foreach (Item ItemTest in EW.g_ItemList.ToList())
                {
                    if (ItemTest.Name.Contains(sItemName, StringComparison.OrdinalIgnoreCase) || ItemTest.ShortName.Contains(sItemName, StringComparison.OrdinalIgnoreCase))
                    {
                        target = ItemTest.Owner;
                        item = ItemTest;
                        break;
                    }
                }
                if (item == null)
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Transfer.InvalidItemName", command.ChatTrigger, EW.g_Scheme.Color_warning);
                    return;
                }
            } else
            {
                var targets = TargetManager.Find(client, command.GetArg(1));

                if (targets.Count > 0)
                {
                    target = targets.Single();
                    if (AdminCommands_GetPlayerImmunity(client) < AdminCommands_GetPlayerImmunity(target))
                    {
                        UI.ReplyToCommand(client, "EntWatch.Reply.You_cannot_target", command.ChatTrigger, EW.g_Scheme.Color_disabled);
                        return;
                    }
                }
            }

            var receivers = TargetManager.Find(client, command.GetArg(2));
            IGameClient? receiver;
            if (receivers.Count > 0)
            {
                receiver = receivers.Single();

                if (!EW.CheckDictionary(receiver))
                {
                    UI.ReplyToCommand(client, "EntWatch.Info.Error.NotFoundInDictionary", command.ChatTrigger);
                    return;
                }

                if (EW.g_EWPlayer[receiver].BannedPlayer.bBanned)
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Has_a_Restrict", command.ChatTrigger, UI.PlayerInfo(client, UI.PlayerInfoFormat(receiver)), EW.g_Scheme.Color_disabled);
                    return;
                }
            } else
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.No_matching_client", command.ChatTrigger, EW.g_Scheme.Color_warning);
                return;
            }

            if (target != null)
            {
                if (target == receiver)
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Transfer.AlreadyOwns", command.ChatTrigger, EW.g_Scheme.Color_warning);
                    return;
                }

                if (target.GetPlayerController() is { } targetplayer && receiver.GetPlayerController() is { } receiverplayer && targetplayer.Team != receiverplayer.Team)
                {
                    UI.ReplyToCommand(client, "EntWatch.Reply.Transfer.Differsteam", command.ChatTrigger, EW.g_Scheme.Color_warning);
                    return;
                }
            }

            if (item == null)
            {
                Transfer.Target(client, target, receiver, command.ChatTrigger);
            }
            else
            {
                Transfer.ItemName(client, item, receiver, command.ChatTrigger);
            }
        }

        private void OnEWSpawn(IGameClient? client, StringCommand command)
        {
            if (client == null || !client.IsValid || !EW.g_CfgLoaded || EW.g_Scheme == null) return;

            int iArgNeed = 2;
            string sArgHelper = "<itemname> <receiver> [<strip>]";
            if (command.ArgCount < iArgNeed)
            {
                UI.ReplyToCommand(client, "EntWatch.Info.Error.MinArg", command.ChatTrigger, iArgNeed, command.CommandName, sArgHelper);
                return;
            }

            string sItemName = command.GetArg(1);

            if (string.IsNullOrEmpty(sItemName))
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Spawn.BadItemName", command.ChatTrigger, EW.g_Scheme.Color_warning);
                return;
            }

            bool bStrip = false;
            string sStrip = command.ArgCount >= 3 ? command.GetArg(3) : "";
            if (!string.IsNullOrEmpty(sStrip)) bStrip = sStrip.Contains("true", StringComparison.OrdinalIgnoreCase) || string.Equals(sStrip, "1");

            var receivers = TargetManager.Find(client, command.GetArg(2));
            if (receivers.Count > 0)
            {
                if (receivers.Count == 1)
                {
                    IGameClient receiver = receivers.Single();
                    if (!EW.CheckDictionary(receiver))
                    {
                        UI.ReplyToCommand(client, "EntWatch.Info.Error.NotFoundInDictionary", command.ChatTrigger);
                        return;
                    }

                    if (EW.g_EWPlayer[receiver].BannedPlayer.bBanned)
                    {
                        UI.ReplyToCommand(client, "EntWatch.Reply.Eban.Has_a_Restrict", command.ChatTrigger, UI.PlayerInfo(client, UI.PlayerInfoFormat(receiver)), EW.g_Scheme.Color_disabled);
                        return;
                    }

                    SpawnItem.Spawn(client, receiver, sItemName, bStrip, command.ChatTrigger, true);
                }
                else
                {
                    ItemConfig? Item = null;
                    foreach (var receiver in receivers)
                    {
                        if (EW.CheckDictionary(receiver) && !EW.g_EWPlayer[receiver].BannedPlayer.bBanned)
                        {
                            (ItemConfig?, int) objReturn = SpawnItem.Spawn(client, receiver, sItemName, bStrip, command.ChatTrigger, false);
                            if (objReturn.Item2 == 0) return;
                            if (Item == null && objReturn.Item1 != null) Item = objReturn.Item1;
                        }
                    }
                    if (Item != null)
                    {
                        string[] sReceivers = new string[4];
                        sReceivers[0] = $"{EW.g_Scheme.Color_name}{UI.ReplaceSpecial(command.GetArg(2))}{EW.g_Scheme.Color_warning}";
                        sReceivers[1] = sReceivers[0];
                        sReceivers[2] = sReceivers[0];
                        sReceivers[3] = sReceivers[0];
                        UI.EWChatAdminSpawn(UI.PlayerInfoFormat(client), sReceivers, $"{Item.Color}{Item.Name}({Item.ShortName}){EW.g_Scheme.Color_warning}");
                    }
                }
            }
            else
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.No_matching_client", command.ChatTrigger, EW.g_Scheme.Color_warning);
            }
        }
    }
}
