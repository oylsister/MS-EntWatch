using MS_EntWatch.Helpers;
using Sharp.Shared.Objects;

namespace MS_EntWatch.Modules.Eban
{
    public class OfflineBan
    {
        public int UserID;
        public string Name;
        public string SteamID;
        public string LastItem;
        public double TimeStamp;
        public double TimeStamp_Start;
        public bool Online;
        public byte Immutity;
        public IGameClient? Player;

        public OfflineBan()
        {
            UserID = 0;
            Name = "";
            SteamID = "";
            LastItem = "";
            TimeStamp = 0;
            TimeStamp_Start = 0;
            Online = false;
            Immutity = 0;
        }
    }

    public static class OfflineFunc
    {
        static OfflineBan CreateOrFind(IGameClient UserID)
        {
            OfflineBan? offlineplayer = null;
            foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
            {
                if (string.Equals(OfflineTest.SteamID, EW.ConvertSteamID64ToSteamID(UserID.SteamId.ToString())))
                {
                    offlineplayer = OfflineTest;
                    break;
                }
            }
            if (offlineplayer == null)
            {
                offlineplayer = new OfflineBan();
                EW.g_OfflinePlayer.Add(offlineplayer);
            }
            offlineplayer.UserID = UserID.UserId;
            offlineplayer.Name = UI.ReplaceSpecial(UserID.Name);
            string ? sSteamID = EW.ConvertSteamID64ToSteamID(UserID.SteamId.ToString());
            if (!string.IsNullOrEmpty(sSteamID)) offlineplayer.SteamID = sSteamID;
            else offlineplayer.SteamID = "null";
            offlineplayer.Immutity = EntWatch.AdminCommands_GetPlayerImmunity(UserID);
            return offlineplayer;
        }
        public static void PlayerConnectFull(IGameClient UserID)
        {
            if (!UserID.IsValid || UserID.IsFakeClient) return;
            OfflineBan OfflinePlayer = CreateOrFind(UserID);
            OfflinePlayer.Player = UserID;
            OfflinePlayer.Online = true;
        }
        public static void PlayerDisconnect(IGameClient UserID)
        {
            if (!UserID.IsValid || UserID.IsFakeClient) return;
            OfflineBan OfflinePlayer = CreateOrFind(UserID);
            OfflinePlayer.TimeStamp_Start = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            OfflinePlayer.TimeStamp = OfflinePlayer.TimeStamp_Start + Cvar.OfflineClearTime * 60;
            OfflinePlayer.Player = null;
            OfflinePlayer.Online = false;
        }
        public static void TimeToClear()
        {
            double CurrentTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
            {
                if (!OfflineTest.Online && OfflineTest.TimeStamp < CurrentTime) EW.g_OfflinePlayer.Remove(OfflineTest);
            }
        }

        public static OfflineBan? FindTarget(IGameClient admin, string sTarget, bool bChat)
        {
            uint iAdminImmunity = EntWatch.AdminCommands_GetPlayerImmunity(admin);
            OfflineBan? target = null;
            if (sTarget.StartsWith("#steam_", StringComparison.OrdinalIgnoreCase))
            {
                string sTargetSteamID = sTarget[1..].ToLower();
                //steamid
                foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
                {
                    if (!OfflineTest.Online && string.Equals(OfflineTest.SteamID.ToLower(), sTargetSteamID))
                    {
                        target = OfflineTest;
                        break;
                    }
                }
            }
            else if (sTarget[0] == '#')
            {
                //userid
                if (!int.TryParse(sTarget[1..], out int iUID))
                {
                    if (EW.g_Scheme != null) UI.ReplyToCommand(admin, "EntWatch.Reply.Must_be_an_integer", bChat, EW.g_Scheme.Color_warning);
                    return null;
                }
                foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
                {
                    if (!OfflineTest.Online && OfflineTest.UserID == iUID)
                    {
                        target = OfflineTest;
                        break;
                    }
                }
            }
            else
            {
                //name
                int iCount = 0;
                foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
                {
                    if (!OfflineTest.Online && OfflineTest.Name.Contains(sTarget, StringComparison.OrdinalIgnoreCase) && iAdminImmunity > OfflineTest.Immutity)
                    {
                        target = OfflineTest;
                        iCount++;
                    }
                }
                if (iCount > 1)
                {
                    if (EW.g_Scheme != null) UI.ReplyToCommand(admin, "EntWatch.Reply.More_than_one_client_matched", bChat, EW.g_Scheme.Color_warning);
                    return null;
                }
            }
            if (target != null && iAdminImmunity > target.Immutity)
            {
                return target;
            }

            return null;
        }
    }
}
