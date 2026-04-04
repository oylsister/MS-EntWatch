using MS_EntWatch.Items;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Types;

namespace MS_EntWatch.Modules
{
    abstract class UHud
    {
        public Vector vecEntity = new(-6.5f, 2.0f, 7.0f);
        public Color32 colorEntity = new(255, 255, 255, 255);
        public int iSheetMax = 5;
        public int iRefresh = 3;
        public int iSize = 54;
        int iCurrentNumList = 0;
        double fNextUpdateList = EW.fGameTime - 3;
        public UHud() { }
        public void ConstructString(IPlayerController HudPlayer)
        {
            List<Item> ListShow = [];
            bool bAdminPermissions = HudPlayer.GetGameClient() is { } cl && EntWatch.AdminCommands_CheckPermission(cl, "ew_hud") && Cvar.AdminHud < 2;
            foreach (Item ItemTest in EW.g_ItemList.ToList())
            {
                if (ItemTest.Owner != null)
                {
                    if (ItemTest.Hud && (!Cvar.TeamOnly || HudPlayer.Team < CStrikeTeam.TE || ItemTest.Team == HudPlayer.Team || bAdminPermissions))
                    {
                        ListShow.Add(ItemTest);
                    }
                }
            }
            if (ListShow.Count > 0)
            {
                int iCountList = (ListShow.Count - 1) / iSheetMax + 1;

                if (fNextUpdateList <= EW.fGameTime)
                {
                    iCurrentNumList++;
                    fNextUpdateList = EW.fGameTime + iRefresh;
                }
                if (iCurrentNumList >= iCountList) iCurrentNumList = 0;

                string sItems = "EntWatch:";

                for (int i = iCurrentNumList * iSheetMax; i < ListShow.Count && i < (iCurrentNumList + 1) * iSheetMax; i++)
                {
                    sItems += $"\n{ListShow[i].ShortName}";
                    if (!Cvar.TeamOnly || HudPlayer.Team < CStrikeTeam.TE || ListShow[i].Team == HudPlayer.Team || bAdminPermissions && Cvar.AdminHud == 0)
                    {
                        if (ListShow[i].CheckDelay())
                        {
                            int iAbilityCount = 0;
                            foreach (Ability AbilityTest in ListShow[i].AbilityList.ToList())
                            {
                                if (++iAbilityCount > Cvar.DisplayAbility) break;
                                if (!AbilityTest.Ignore) sItems += $"[{AbilityTest.GetMessage()}]";
                            }

                        }
                        else sItems += $"[-{Math.Round(ListShow[i].fDelay - EW.fGameTime, 1)}]";
                    }
                    sItems += $": {ListShow[i].Owner?.Name}";
                }
                if (iCountList > 1) sItems += $"\nList:[{iCurrentNumList + 1}/{iCountList}]";
                UpdateText(sItems, HudPlayer);
            }
            else UpdateText("", HudPlayer);
        }
        public abstract void UpdateText(string sItems, IPlayerController HudPlayer);
    }

    class HudNull : UHud
    {
        public HudNull() { }
        public override void UpdateText(string sItems, IPlayerController HudPlayer) { }
    }

    class HudCenter : UHud
    {
        public HudCenter() { }
        public override void UpdateText(string sItems, IPlayerController HudPlayer)
        {
            if (HudPlayer.IsValid() && !HudPlayer.IsFakeClient && !string.IsNullOrEmpty(sItems)) HudPlayer.Print(HudPrintChannel.Center, sItems);
        }
    }
    class HudAlert : UHud
    {
        public HudAlert() { }
        public override void UpdateText(string sItems, IPlayerController HudPlayer)
        {
            if (HudPlayer.IsValid() && !HudPlayer.IsFakeClient && !string.IsNullOrEmpty(sItems)) HudPlayer.Print(HudPrintChannel.Hint, sItems);
        }
    }

    class HudWorldText : UHud
    {
        public HudWorldText() { }
        public void InitHud(IPlayerController HudPlayer)
        {
            if (EntWatch.GetGameHUD() is { } _api && HudPlayer.IsValid())
            {
                _api.Native_GameHUD_SetParams(HudPlayer, EW.HUDCHANNEL, vecEntity, colorEntity, iSize, "Verdana", iSize / 7000.0f);
            }
        }
        public override void UpdateText(string sItems, IPlayerController HudPlayer)
        {
            if (EntWatch.GetGameHUD() is { } _api && HudPlayer.IsValid())
            {
                _api.Native_GameHUD_ShowPermanent(HudPlayer, EW.HUDCHANNEL, sItems);
            }
        }
    }
}
