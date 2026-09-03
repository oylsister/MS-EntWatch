using MS_EntWatch.Modules;
using MS_EntWatch.Modules.Eban;
using Sharp.Shared.GameEntities;

namespace MS_EntWatch
{
    internal class EWPlayer
    {
        public EbanPlayer BannedPlayer;
        public UHud HudPlayer;
        public UsePriority UsePriorityPlayer;
        public Privilege PrivilegePlayer;
        public int PFormatPlayer;

        public EWPlayer()
        {
            BannedPlayer = new EbanPlayer();
            HudPlayer = new HudNull();
            UsePriorityPlayer = new UsePriority();
            PrivilegePlayer = new Privilege();
            PFormatPlayer = Cvar.PlayerFormat;
        }

        public void RemoveEntityHud(IPlayerController player)
        {
            if (EntWatch.GetGameHUD() is { } _api && player.IsValid())
            {
                _api.Native_GameHUD_Remove(player, EW.HUDCHANNEL);
            }

            PanoramaService.KillHud(player);
        }

        public void SwitchHud(IPlayerController player, int number)
        {
            RemoveEntityHud(player);

            var LastCfg = HudPlayer;

            HudPlayer = number switch
            {
                0 => new HudNull(),
                1 => new HudCenter(),
                2 => new HudAlert(),
                3 => new HudWorldText(),
                4 => new PanoramaHud(),
                _ => new HudNull(),
            };
            HudPlayer.vecEntity = LastCfg.vecEntity;
            HudPlayer.colorEntity = LastCfg.colorEntity;
            HudPlayer.iSheetMax = LastCfg.iSheetMax;
            HudPlayer.iRefresh = LastCfg.iRefresh;
            HudPlayer.iSize = LastCfg.iSize;
            if (HudPlayer is HudWorldText hud) hud.InitHud(player);
        }
    }
}
