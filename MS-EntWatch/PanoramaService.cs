using MS_EntWatch.Items;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;

namespace MS_EntWatch;

public class PanoramaService
{
    private static IPanoramaManager _manager;
    public static Dictionary<IPlayerController, ICustomHudLayout?> _hudList = [];

    public static void GetPanoramaManager(IPanoramaManager manager)
    {
        _manager = manager;
    }

    private static ICustomHudLayout? GetPlayerHud(IPlayerController client)
    {
        if(!_hudList.TryGetValue(client, out var hud))
        {
            _hudList.Add(client, null);
            return _hudList[client];
        }

        return hud;
    }

    public static void UpdateText(IPlayerController client, string text)
    {
        var hud = GetPlayerHud(client);

        if (hud == null || !hud.IsValid())
        {
            hud = InitHud(client);
            _hudList[client] = hud;
        }

        hud?.SetDialogVariableStringForPlayer(client, "entwatch", "item-list", text);

        if(string.IsNullOrEmpty(text))
            hud?.SetClassOverrideForPlayer(client, "entwatch", "Disabled", HudPanelClassStatus.ForceEnable);

        else
            hud?.SetClassOverrideForPlayer(client, "entwatch", "Disabled", HudPanelClassStatus.ForceDisable);
    }

    public static void KillHud(IPlayerController client)
    {
        if (_hudList.TryGetValue(client, out var hud))
        {
            hud?.Kill();
            _hudList[client] = null;
        }
    }

    public static void RemovePlayerHud(IPlayerController client)
    {
        KillHud(client);
        _hudList.Remove(client);
    }

    public static void KillAllHud()
    {
        foreach (var key in _hudList.Keys.ToList())
        {
            _hudList[key]?.Kill();
            _hudList[key] = null;
        }
    }

    public static ICustomHudLayout? InitHud(IPlayerController client)
    {
        var path = "panorama/layout/custom_game/entwatch_ed.vxml_c";
        var hud = EntWatch._panorama.CreateLayout(path);
        hud?.SetClassOverrideForPlayer(client, "entwatch", "Disabled", HudPanelClassStatus.ForceDisable);
        return hud;
    }
}
