using MS_EntWatch.Items;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
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
        int iCurrentNumListH = 0;
        int iCurrentNumListZM = 0;
        double fNextUpdateList = EW.fGameTime - 3;
        public UHud() { }
        public void ConstructString(IPlayerController HudPlayer)
        {
            List<Item> ListShowH = [];
            List<Item> ListShowZM = [];
            bool bAdminPermissions = HudPlayer.GetGameClient() is { } cl && EntWatch.AdminCommands_CheckPermission(cl, EntWatch.PermissionHUD) && Cvar.AdminHud < 2;
            foreach (Item ItemTest in EW.g_ItemList.ToList())
            {
                if (ItemTest.Owner != null)
                {
                    if (ItemTest.Hud && (!Cvar.TeamOnly || HudPlayer.Team < CStrikeTeam.TE || ItemTest.Team == HudPlayer.Team || bAdminPermissions))
                    {
                        if (ItemTest.Team == CStrikeTeam.CT) ListShowH.Add(ItemTest);
                        else if (ItemTest.Team == CStrikeTeam.TE) ListShowZM.Add(ItemTest);
                    }
                }
            }
            if (ListShowH.Count > 0 || ListShowZM.Count > 0)
            {
                string sItems = "";
                bool bNextUpdateSync = true;
                if (ListShowH.Count > 0)
                {
                    int iCountListH = (ListShowH.Count - 1) / iSheetMax + 1;

                    if (fNextUpdateList <= EW.fGameTime)
                    {
                        iCurrentNumListH++;
                        fNextUpdateList = EW.fGameTime + iRefresh;
                        bNextUpdateSync = false;
                    }
                    if (iCurrentNumListH >= iCountListH) iCurrentNumListH = 0;

                    sItems += "EntWatch Humans:";

                    for (int i = iCurrentNumListH * iSheetMax; i < ListShowH.Count && i < (iCurrentNumListH + 1) * iSheetMax; i++)
                    {
                        sItems += $"\n{ListShowH[i].ShortName}";
                        if (!Cvar.TeamOnly || HudPlayer.Team < CStrikeTeam.TE || ListShowH[i].Team == HudPlayer.Team || bAdminPermissions && Cvar.AdminHud == 0)
                        {
                            if (ListShowH[i].CheckDelay())
                            {
                                int iAbilityCount = 0;
                                foreach (Ability AbilityTest in ListShowH[i].AbilityList.ToList())
                                {
                                    if (++iAbilityCount > Cvar.DisplayAbility) break;
                                    if (!AbilityTest.Ignore) sItems += $"[{AbilityTest.GetMessage()}]";
                                }

                            }
                            else sItems += $"[-{Math.Round(ListShowH[i].fDelay - EW.fGameTime, 1)}]";
                        }
                        sItems += $": {ListShowH[i].Owner?.Name}";
                    }
                    if (iCountListH > 1) sItems += $"\nList:[{iCurrentNumListH + 1}/{iCountListH}]";
                }

                if (ListShowZM.Count > 0)
                {
                    int iCountListZM = (ListShowZM.Count - 1) / iSheetMax + 1;

                    if (!bNextUpdateSync || fNextUpdateList <= EW.fGameTime)
                    {
                        iCurrentNumListZM++;
                        if (bNextUpdateSync) fNextUpdateList = EW.fGameTime + iRefresh;
                    }
                    if (iCurrentNumListZM >= iCountListZM) iCurrentNumListZM = 0;

                    if (!string.IsNullOrEmpty(sItems)) sItems += "\n\n";

                    sItems += "EntWatch Zombies:";

                    for (int i = iCurrentNumListZM * iSheetMax; i < ListShowZM.Count && i < (iCurrentNumListZM + 1) * iSheetMax; i++)
                    {
                        sItems += $"\n{ListShowZM[i].ShortName}";
                        if (!Cvar.TeamOnly || HudPlayer.Team < CStrikeTeam.TE || ListShowZM[i].Team == HudPlayer.Team || bAdminPermissions && Cvar.AdminHud == 0)
                        {
                            if (ListShowZM[i].CheckDelay())
                            {
                                int iAbilityCount = 0;
                                foreach (Ability AbilityTest in ListShowZM[i].AbilityList.ToList())
                                {
                                    if (++iAbilityCount > Cvar.DisplayAbility) break;
                                    if (!AbilityTest.Ignore) sItems += $"[{AbilityTest.GetMessage()}]";
                                }

                            }
                            else sItems += $"[-{Math.Round(ListShowZM[i].fDelay - EW.fGameTime, 1)}]";
                        }
                        sItems += $": {ListShowZM[i].Owner?.Name}";
                    }
                    if (iCountListZM > 1) sItems += $"\nList:[{iCurrentNumListZM + 1}/{iCountListZM}]";
                }

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

    class PanoramaHud : UHud
    {
        public ICustomHudLayout? iHudLayout;

        public PanoramaHud() { }

        public override void UpdateText(string sItems, IPlayerController HudPlayer)
        {
            if (iHudLayout == null || !iHudLayout.IsValid()) InitHud(HudPlayer);

            if (string.IsNullOrEmpty(sItems))
            {
                iHudLayout?.SetClassOverrideForPlayer(HudPlayer, "entwatch", "Disabled", HudPanelClassStatus.ForceEnable);
            }

            else if (HudPlayer.IsValid() && !HudPlayer.IsFakeClient && !string.IsNullOrEmpty(sItems))
            {
                //HudPlayer.Print(HudPrintChannel.Chat, "Update HUD");
                iHudLayout?.SetDialogVariableStringForPlayer(HudPlayer, "entwatch", "item-list", sItems);
                iHudLayout?.SetClassOverrideForPlayer(HudPlayer, "entwatch", "Disabled", HudPanelClassStatus.ForceDisable);
            }
        }

        public void InitHud(IPlayerController HudPlayer)
        {
            var path = "panorama/layout/custom_game/entwatch_ed.vxml_c";
            iHudLayout = EntWatch._panorama.CreateLayout(path);

            //HudPlayer.Print(HudPrintChannel.Chat, "Created Hud");

            if (iHudLayout != null && iHudLayout.IsValid())
            {
                iHudLayout.SetClassOverrideForPlayer(HudPlayer, "entwatch", "Disabled", HudPanelClassStatus.ForceDisable);
                //HudPlayer.Print(HudPrintChannel.Chat, "Set entwatch opacity 0 Disabled");
            }
        }

        public void RemoveHud()
        {
            if (iHudLayout != null && iHudLayout.IsValid())
            {
                iHudLayout.Kill();
                iHudLayout = null;
            }
        }
    }
}
